# Proposta - Trabalho Semestral – Arquitetura de Aplicações Web 2026.2
- **Aluno:** Joao Antonio De Souza Santos • **Curso/Turma:** Arquitetura de Aplicações Web - 2026.2
- **Repositório:** https://github.com/Charmed07/ra-2024510631111

## 1. Domínio e problema
O sistema **FleetLog** resolve o problema de sobrecarga e despacho indevido de mercadorias em frotas de transporte rodoviário para operadores logísticos de médio porte. A aplicação controla a capacidade operacional de cada veículo e impede a alocação de cargas que ultrapassem o limite de peso ou que sejam destinadas a veículos indisponíveis ou em manutenção. Isso garante o cumprimento de normas de trânsito, reduz custos de manutenção corretiva e otimiza a produtividade das viagens.

## 2. Entidades
- **Entidade A - `Veiculo`:**
  - `Id` (Identificador único - ObjectId)
  - `Placa` (Identificador alfanumérico do veículo, único)
  - `Modelo` (Descrição do modelo do veículo, ex: Caminhão Toco, VUC)
  - `CapacidadeCargaKg` (Capacidade de peso máxima suportada em kg)
  - `AnoFabricacao` (Ano de fabricação do veículo)
  - `StatusOperacional` (Estado do veículo: `Disponivel`, `EmViagem`, `EmManutencao`, `Inativo`)

- **Entidade B - `OrdemDeCarga`:**
  - `Id` (Identificador único - ObjectId)
  - `CodigoRastreio` (Código alfanumérico de identificação do frete, único)
  - `DestinoCidade` (Cidade/UF de entrega da carga)
  - `PesoCargaKg` (Peso total da mercadoria em kg)
  - `ValorFrete` (Valor monetário cobrado pelo transporte)
  - `StatusEntrega` (Estado do frete: `Pendente`, `EmTransito`, `Entregue`, `Cancelada`)
  - `DataCriacao` (Data e hora do registro da ordem)
  - `VeiculoId` (Identificador do veículo alocado - chave de referência)

- **Relacionamento e Modelagem NoSQL:**
  - **Tipo de Relacionamento:** $1\text{–}N$ (Um veículo pode receber múltiplas ordens de carga ao longo de sua vida operacional).
  - **Estratégia de Modelagem:** **Referência (Normalizada/Referenciada)**, mantendo uma coleção `veiculos` e uma coleção `ordens_carga`, vinculadas através do campo `VeiculoId`.
  - **Justificativa:** Em sistemas de transporte, o histórico de ordens de frete cresce continuamente. Modelar as ordens embutidas (*embedded*) dentro do documento do veículo levaria à saturação do limite de 16MB por documento do MongoDB, além de gerar concorrência excessiva de escrita ao atualizar o status de entregas simultâneas. A modelagem por referência desacopla as operações, viabiliza paginação eficiente e facilita relatórios independentes de frota e de fretes.

## 3. Endpoints previstos

### Veículos (`/api/veiculos`):
- `GET /api/veiculos` — Listar todos os veículos da frota (com suporte a filtro por status).
- `GET /api/veiculos/{id}` — Obter os detalhes cadastrais de um veículo específico por identificador.
- `POST /api/veiculos` — Cadastrar um novo veículo na frota.
- `PUT /api/veiculos/{id}` — Atualizar os dados de um veículo (modelo, capacidade ou status).
- `DELETE /api/veiculos/{id}` — Remover um veículo da frota (restrito ao perfil Admin).

### Ordens de Carga (`/api/ordens-carga`):
- `GET /api/ordens-carga` — Listar todas as ordens de carga registradas (ou filtrar por `veiculoId`).
- `GET /api/ordens-carga/{id}` — Obter os detalhes completos de uma ordem de carga específica.
- `POST /api/ordens-carga` — Criar uma nova ordem de carga e alocá-la a um veículo (aplica regras de negócio).
- `PUT /api/ordens-carga/{id}/status` — Atualizar a transição de estado da ordem (`EmTransito`, `Entregue`, `Cancelada`).
- `DELETE /api/ordens-carga/{id}` — Cancelar/remover uma ordem de carga (restrito ao perfil Admin).

## 4. Regra de negócio
A camada de serviço (`OrdemDeCargaService`) aplicará duas regras de negócio obrigatórias no momento da escrita (`POST` / criação da ordem):
1. **Validação de Capacidade de Carga:** O serviço consulta o veículo informado e calcula a soma dos pesos das ordens de carga ativas já vinculadas a ele. Se a nova carga fizer o total ultrapassar o atributo `CapacidadeCargaKg` do veículo, a alocação é bloqueada.
2. **Validação de Disponibilidade Operacional:** Ordens de carga só podem ser despachadas para veículos cujo `StatusOperacional` seja `Disponivel`. Tentativas de despachar mercadoria para veículos com status `EmManutencao` ou `Inativo` são imediatamente recusadas.

## 5. Casos de erro
1. **Status `400 Bad Request` (Violação de Regra de Negócio):** Ocorre ao tentar cadastrar uma ordem cujo peso exceda a capacidade máxima restante do veículo, ou caso o veículo esteja com o status operacional diferente de `Disponivel`. A API retorna uma mensagem descritiva do motivo da rejeição.
2. **Status `404 Not Found` (Registro Inexistente):** Ocorre ao tentar consultar, atualizar, remover ou associar uma ordem a um identificador de `Veiculo` ou `OrdemDeCarga` que não exista no banco de dados.

## 6. Stack escolhida
- **Backend:** C# .NET 10 (ASP.NET Core Web API).
- **Banco de Dados:** MongoDB (Driver oficial `MongoDB.Driver` com persistência parametrizada via variáveis de ambiente / `IConfiguration`).
- **Frontend:** HTML5, CSS3 e JavaScript Vanilla assíncrono via `fetch` API (navegação dinâmica sem recarregamento de página).

## 7. Segurança e testes
- **Autenticação & Autorização:** Autenticação baseada em tokens JWT (*JSON Web Tokens*) com tempo de expiração e controle de acesso RBAC (*Role-Based Access Control*) com perfis `Admin` e `Usuario`, aplicando o atributo `[Authorize(Roles = "...")]` nos endpoints de escrita e deleção sensíveis.
- **Testes Unitários:** Implementados com o framework `xUnit` na camada de serviço, cobrindo:
  - *Cenário de Sucesso 1:* Alocação bem-sucedida de ordem de carga respeitando o limite de peso em veículo disponível.
  - *Cenário de Sucesso 2:* Transição válida de status de entrega de `EmTransito` para `Entregue`.
  - *Cenário de Falha 1:* Rejeição de alocação de ordem de carga por excesso de peso em relação à capacidade máxima do veículo.
  - *Cenário de Falha 2:* Rejeição de alocação de ordem de carga para veículo com status `EmManutencao`.