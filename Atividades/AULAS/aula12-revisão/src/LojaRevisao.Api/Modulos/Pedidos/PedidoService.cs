using LojaRevisao.Api.Data;
using LojaRevisao.Api.Models;
using LojaRevisao.Api.Modulos.Catalogo;
using LojaRevisao.Api.Modulos.Notificacoes;
using Microsoft.EntityFrameworkCore;

namespace LojaRevisao.Api.Modulos.Pedidos;

public class PedidoService
{
    private readonly LojaDbContext _db;
    private readonly ICatalogo _catalogo;
    private readonly EmailGatewaySimulado _email;
    private readonly FilaDeNotificacoes _fila;

    public PedidoService(LojaDbContext db, ICatalogo catalogo, EmailGatewaySimulado email, FilaDeNotificacoes fila)
    {
        _db = db;
        _catalogo = catalogo;
        _email = email;
        _fila = fila;
    }

    public async Task<Pedido> CriarAsync(int usuarioId, NovoPedido novo)
    {
        var pedido = new Pedido { UsuarioId = usuarioId };

        foreach (var item in novo.Itens)
        {
            // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
            //  FALHA 01 Â· AULA 01 â€” MonÃ³lito e fronteiras (nÃ£o corrija ainda!)
            //  Sintoma: comprar 999.999 unidades funciona e o estoque fica
            //  NEGATIVO. O mÃ³dulo de Pedidos mexe direto nos dados do mÃ³dulo
            //  de CatÃ¡logo e pula a regra que mora lÃ¡.
            //  Pense: e no dia em que o CatÃ¡logo virar o ServicoProdutos?
            //  Sua tarefa: fazer Pedidos falar com o CatÃ¡logo sÃ³ pelo
            //  contrato dele (leia Modulos/Catalogo/ICatalogo.cs).
            // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
            var produto = _catalogo.Reservar(item.ProdutoId, item.Quantidade);

            // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
            //  FALHA 05 Â· AULA 05 â€” PersistÃªncia e ACID (nÃ£o corrija ainda!)
            //  Sintoma: num pedido com um item vÃ¡lido e outro inexistente, a
            //  API responde 404, mas o estoque do item vÃ¡lido jÃ¡ baixou.
            //  Nenhum pedido foi criado e a mercadoria "sumiu".
            //  Sua tarefa: garantir que o pedido seja TUDO OU NADA.
            // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

            pedido.Itens.Add(new ItemPedido
            {
                ProdutoId = produto.Id,
                Quantidade = item.Quantidade,
                PrecoUnitario = produto.Preco
            });
        }

        pedido.Subtotal = pedido.Itens.Sum(i => i.PrecoUnitario * i.Quantidade);
        pedido.Frete = CalculadoraDePedido.CalcularFrete(pedido.Subtotal);
        pedido.Total = CalculadoraDePedido.CalcularTotal(pedido.Subtotal);

        _db.Pedidos.Add(pedido);
        _db.SaveChanges();

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        //  FALHA 06 Â· AULA 06 â€” ComunicaÃ§Ã£o entre serviÃ§os (nÃ£o corrija ainda!)
        //  Sintoma: todo pedido demora mais de 3 segundos, porque o cliente
        //  fica esperando o provedor de e-mail responder (acoplamento temporal).
        //  Sua tarefa: o e-mail pode chegar depois. Publique um evento
        //  PedidoCriado na fila; o consumidor jÃ¡ existe e jÃ¡ estÃ¡ registrado
        //  (leia Modulos/Notificacoes/Notificacoes.cs).
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        await _fila.PublicarAsync(new PedidoCriado(pedido.Id));

        return pedido;
    }

    public Pedido? Obter(int id) =>
        _db.Pedidos.Include(p => p.Itens).FirstOrDefault(p => p.Id == id);

    public List<Pedido> ListarDoUsuario(int usuarioId) =>
        _db.Pedidos.Include(p => p.Itens).Where(p => p.UsuarioId == usuarioId).OrderBy(p => p.Id).ToList();

    /// <summary>Cancela o pedido e devolve as unidades ao estoque.</summary>
    public void Cancelar(Pedido pedido)
    {
        foreach (var item in pedido.Itens)
            _catalogo.Devolver(item.ProdutoId, item.Quantidade);

        _db.Pedidos.Remove(pedido);
        _db.SaveChanges();
    }
}
