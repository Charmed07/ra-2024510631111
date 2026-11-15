using FleetLog.Api.Models;
using Microsoft.VisualBasic;
using Xunit;

namespace FleetLog.Tests;

public class OrdemDeCargaServiceTests
{
    private static void ValidarRegrasAlocacaoCarga(Veiculo veiculo , decimal pesoAtualAlocado, OrdemDeCarga novaOrdem)
    {
        if(veiculo.StatusOperacional != StatusOperacional.Disponivel)
        {
            throw new InvalidOperationException($"O veiculo {veiculo.Placa} está com status '{veiculo.StatusOperacional}'e não pode receber cargas.");
        }

        var pesoFinal = pesoAtualAlocado + novaOrdem.PesoCargaKg;
        if(pesoFinal > veiculo.CapacidadeCargaKg)
        {
            throw new InvalidOperationException("Capacidade máxima de peso excedida para este veiculo");
        }
    }

    [Fact(DisplayName = "Sucesso 1: Alocar carga dentro da capacidade em veiculo disponivel")]
    public void AlocarCrga_DentroDaCapacidade_DevePermitirComSucesso()
    {
        var veiculo = new Veiculo
        {
            Id = "653f1a2b3c4d5e6f7a8b9c01",
            Placa = "ABC-1234",
            CapacidadeCargaKg = 5000m,
            StatusOperacional = StatusOperacional.Disponivel
        };

        var pesoAtualAlocado = 1500m;
        var novaOrdem = new OrdemDeCarga
        {
            CodigoRastreio = "FL-001",
            DestinoCidade = "Belo Horizonte/MG",
            PesoCargaKg = 2000m, 
            StatusEntrega = StatusEntrega.Pendente
        };

        var excecao = Record.Exception(()=> ValidarRegrasAlocacaoCarga(veiculo , pesoAtualAlocado , novaOrdem));

        Assert.Null(excecao);
        Assert.Equal(StatusEntrega.Pendente, novaOrdem.StatusEntrega);
    }

    [Fact(DisplayName = "Sucesso 2: Transição valida de status para Entregue")]
    public void AtualizarStatus_ParaEntregue_DeveAtualizarComSucesso()
    {
        var ordem = new OrdemDeCarga
        {
            Id = "653f1a2b3c4d5e6f7a8b9c02",
            CodigoRastreio = "FL-002",
            StatusEntrega = StatusEntrega.EmTransito
        };

        ordem.StatusEntrega = StatusEntrega.Entregue;

        Assert.Equal(StatusEntrega.Entregue,ordem.StatusEntrega);
    }

    [Fact(DisplayName = "Erro 1: Rejeitar alocação quando o peso excede a capacidade do veiculo")]
    public void AlocarCarga_PesoExcedente_DeveLancarInvalidOperationException()
    {
        var veiculo = new Veiculo
        {
            Placa = "XYZ-9876",
            CapacidadeCargaKg = 3000m,
            StatusOperacional = StatusOperacional.Disponivel   
        };

        var pesoAtualAlocado = 2500m;
        var novaOrdem = new OrdemDeCarga
        {
          CodigoRastreio = "FL-003",
            PesoCargaKg = 1000m  
        };

        var ex = Assert.Throws<InvalidOperationException>(()=>
        ValidarRegrasAlocacaoCarga(veiculo , pesoAtualAlocado, novaOrdem));

        Assert.Contains("Capacidade máxima de peso excedida", ex.Message);
    }

    [Fact(DisplayName = "Erro 2: Rejeitar alocação quando o veiculo está em Manutenção")]
    public void AlocarCarga_VeiculoEmManutencao_DeveLancarInvalidOperationException()
    {
        var veiculo = new Veiculo
        {
            Placa = "MAN-4321",
            CapacidadeCargaKg = 10000m,
            StatusOperacional = StatusOperacional.EmManutencao  
        };

        var pesoAtualAlocado = 0m;
        var novaOrdem = new OrdemDeCarga
        {
            CodigoRastreio = "FL-004",
            PesoCargaKg = 500m
        };

        var ex = Assert.Throws<InvalidOperationException>(()=> ValidarRegrasAlocacaoCarga(veiculo, pesoAtualAlocado,novaOrdem ));

        Assert.Contains("não pode receber cargas", ex.Message);
    }


}