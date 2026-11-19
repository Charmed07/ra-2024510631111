# FleetLog - Sistema de Gestão de Frotas e Despacho de Cargas

- **Aluno:** João Antonio De Souza Santos  
- **Disciplina:** Arquitetura de Aplicações Web (2026.2)  
- **Repositório:** [https://github.com/Charmed07/ra-2024510631111](https://github.com/Charmed07/ra-2024510631111)

---

## 1. Descrição do Projeto e Domínio

O **FleetLog** é uma solução para transportadoras e operadores logísticos que resolve o problema de sobrecarga e despacho indevido de mercadorias em frotas rodoviárias. 

O sistema controla a capacidade máxima de carga em quilogramas (`CapacidadeCargaKg`) de cada veículo e bloqueia o despacho de fretes que ultrapassem o limite de peso ou que sejam destinados a caminhões indisponíveis ou em manutenção (`StatusOperacional != Disponivel`).

---

## 2. Tecnologias Utilizadas

- **Linguagem & Runtime:** C# / .NET 11 (ASP.NET Core Web API)
- **Banco de Dados NoSQL:** MongoDB Atlas Cloud (com modelagem referenciada $1\text{–}N$)
- **Autenticação & RBAC:** JWT (*JSON Web Tokens*) com perfis `Admin` e `Usuario`
- **Testes Unitários:** Framework `xUnit`
- **Documentação Interativa:** Swagger / OpenAPI 3.0 (`Swashbuckle.AspNetCore`)
- **Frontend:** Single Page Application (SPA) em HTML5, CSS3 e JavaScript Vanilla assíncrono via `fetch` (sem recarregamento de página)

---

## 3. Pré-requisitos

- [.NET SDK 10 ou 11](https://dotnet.microsoft.com/download)
- Acesso à internet para comunicação com o cluster MongoDB Atlas

---

## 4. Variáveis de Ambiente

Por segurança e conformidade de boas práticas (sem credenciais fixadas no código-fonte), a conexão com o banco de dados é parametrizada via variável de ambiente:

| Variável | Descrição | Exemplo de Formato (Sem credenciais reais) |
| :--- | :--- | :--- |
| `MONGO_CONNECTION_STRING` | String de conexão do cluster MongoDB | `mongodb+srv://<usuario>:<senha>@<cluster>.mongodb.net/?retryWrites=true&w=majority` |

---

## 5. Passo a Passo para Executar Localmente

### 1. Clonar o repositório
```bash
git clone https://github.com/Charmed07/ra-2024510631111.git
cd ra-2024510631111
```

### 2. Configurar a variável de ambiente
No PowerShell (Windows), defina a sua string de conexão:
```powershell
$env:MONGO_CONNECTION_STRING="sua_string_de_conexao_mongodb_aqui"
```

### 3. Executar a API e Interface Web
Navegue até a pasta do projeto e inicialize o servidor Kestrel:
```powershell
cd Trabalhos/trabalho-01/src/FleetLog.Api
dotnet run
```

A aplicação estará disponível em:
- **Painel Web Operacional (Frontend SPA):** [http://localhost:5269](http://localhost:5269)
- **Documentação Interativa Swagger:** [http://localhost:5269/swagger](http://localhost:5269/swagger)

---

## 6. Como Executar os Testes Unitários

A suíte de testes unitários foi desenvolvida com `xUnit`, cobrindo os cenários de sucesso e erro das regras de negócio (capacidade de carga e disponibilidade operacional).

A partir da raiz do repositório, execute:
```powershell
cd Trabalhos/trabalho-01/tests/FleetLog.Test
dotnet test
```

---

## 7. Credenciais de Acesso Pré-configuradas (RBAC)

Para demonstrar os diferentes níveis de permissão no Swagger e no painel web:

| Perfil (Role) | E-mail | Senha | Permissões |
| :--- | :--- | :--- | :--- |
| **Público** | *(Sem login)* | *(Sem login)* | Apenas visualização da frota e cargas alocadas. |
| **Operador (Usuário)** | `operador@fleetlog.com` | `Operador@123` | Visualizar frota, cadastrar veículos, alocar ordens de carga e concluir entregas. |
| **Administrador (Admin)** | `admin@fleetlog.com` | `Admin@1234` | Controle total, incluindo exclusão definitiva de veículos (`DELETE /api/veiculos/{id}`). |

---

## 8. Documentação Complementar

- [Proposta Inicial do Trabalho (docs/proposta.md)](Trabalhos/trabalho-01/docs/proposta.md)

---

## Uso de IA

- **Ferramentas usadas:** Google Antigravity e Modelos Gemini.
- **Onde ajudou:** Auxiliou no desenho inicial da modelagem NoSQL referenciada, no scaffold dos controllers REST e na configuração do Swagger OpenAPI 3 com autenticação Bearer.
- **O que eu revisei/reescrevi:** Implementei e revisei todas as regras de negócio de cálculo de carga na camada de serviço, validei as asserções dos testes unitários em xUnit, configurei as permissões RBAC nos endpoints e refinei o tratamento de erros no frontend para exibir mensagens amigáveis.