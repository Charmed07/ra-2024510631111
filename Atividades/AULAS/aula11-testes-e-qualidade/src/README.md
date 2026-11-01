# Aula 11 — Testes e Qualidade dos Serviços • Caçada ao Bug

Projeto da atividade prática da **Aula 11** da disciplina *Arquitetura de
Aplicações Web* (Newton/Wyden).

> Stack: **.NET 8 • C# • xUnit • Moq • FluentAssertions • WebApplicationFactory**

---

## 🎯 Missão

Você recebe uma API de pedidos funcional (`ResilientOrders.Api`) que **contém um
bug escondido** em uma regra de negócio. Ninguém vai te contar onde ele está: os
seus testes é que vão encontrá-lo.

As regras do `PedidoService` (as mesmas do handout impresso):

| # | Regra |
|---|-------|
| R1 | Item com quantidade <= 0 torna o pedido inválido |
| R2 | Desconto: **5%** a partir de R$200 (inclusive) • **10%** a partir de R$500 (inclusive) |
| R3 | Frete fixo de **R$25**; grátis a partir de R$300 (inclusive) |
| R4 | Total = subtotal - desconto + frete |

## ⏱️ Roteiro (55 min, em duplas)

A prática começa logo depois da atividade em papel (handout, 20 min): a tabela
de casos que a dupla preencheu é o ponto de partida da Etapa 1.

| Etapa | Tempo | O que fazer |
|-------|-------|-------------|
| 0 | 5 min | `dotnet test` — os 2 exemplos prontos passam; os TODOs aparecem como *Skipped* (já rodado no início da aula, durante o estudo) |
| 1 | 15 min | `PedidoServiceTests.cs` → TODO 1: transformar a **tabela de casos do handout** em `[Theory]`/`[InlineData]` (não esqueça as bordas exatas: 199,99 • 200 • 299,99 • 300 • 499,99 • **500**) |
| 2 | 10 min | Algum caso de borda falhou? **Você achou o bug.** Corrija o `PedidoService` e deixe tudo verde |
| 3 | 10 min | TODOs 2 e 3: validações com `Mock.Verify` e exceções |
| 4 | 15 min | `PedidosEndpointTests.cs` → teste de integração do `POST /api/pedidos` (201 e 400) com `WebApplicationFactory` |
| ⭐ | bônus | `CriarPedido` válido salva exatamente 1 vez com valores coerentes |

## 🚀 Como rodar

```bash
dotnet build                                  # compila tudo
dotnet test                                   # roda a suíte completa
dotnet test tests/ResilientOrders.UnitTests   # só os unitários
dotnet run --project src/ResilientOrders.Api  # sobe a API (Swagger em /swagger)
```

## 📂 Estrutura

```
src/
├── ResilientOrders.sln
├── src/ResilientOrders.Api/                  ← API com o bug escondido
│   ├── Controllers/PedidosController.cs
│   ├── Models/Pedido.cs
│   └── Services/PedidoService.cs             ← classe sob teste
└── tests/
    ├── ResilientOrders.UnitTests/            ← ETAPA 1-3 (você implementa)
    │   └── PedidoServiceTests.cs
    └── ResilientOrders.IntegrationTests/     ← ETAPA 4 (você implementa)
        ├── ApiFactoryFixture.cs              ← fixture pronta (sobe a API em memória)
        └── PedidosEndpointTests.cs
```

## 📦 Critérios de aceite (entregável)

- [x] `[Theory]` com a tabela completa do handout, incluindo todas as bordas
- [x] Bug localizado, corrigido e **documentado no exit ticket** (qual caso o revelou?)
- [x] Validações com `Mock.Verify(..., Times.Never)` nos TODOs 2 e 3
- [x] Teste de integração cobrindo 201 (com `Location`) e 400
- [x] `dotnet test` 100% verde ao final, sem testes *Skipped*

---

## 🎟️ Exit Ticket: Relatório do Bug Encontrado

### 1. Caso de teste que revelou o bug
- **Entrada:** `subtotal = 500,00`
- **Resultado Esperado (Regra R2 + R3):**
  - Desconto de 10% (R$ 50,00) pois a regra define "10% a partir de R$ 500 (inclusive)".
  - Frete grátis (R$ 0,00) pois subtotal >= 300.
  - Total esperado = 500 - 50 + 0 = **R$ 450,00**.
- **Resultado Obtido antes da correção:**
  - O sistema calculou `Total = R$ 475,00` (aplicou apenas 5% de desconto: 500 - 25 = 475).

### 2. Causa raiz no código
No arquivo `src/ResilientOrders.Api/Services/PedidoService.cs`, dentro do método `CalcularDesconto`:
```csharp
// CÓDIGO COM BUG:
if (subtotal > 500m) // Não incluía o valor exato de R$ 500
{
    return Math.Round(subtotal * 0.10m, 2);
}
```

### 3. Correção aplicada
Alterada a condição de maior estrito (`>`) para maior ou igual (`>=`):
```csharp
// CÓDIGO CORRIGIDO:
if (subtotal >= 500m)
{
    return Math.Round(subtotal * 0.10m, 2);
}
```
Após o ajuste, a suíte completa de testes passou com **100% de sucesso**.