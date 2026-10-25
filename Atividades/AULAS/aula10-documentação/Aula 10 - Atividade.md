# HANDOUT — AULA 10

## Decifrando o Contrato

*Lendo uma especificação OpenAPI — Arquitetura de Aplicações Web*

## 🎯 MISSÃO

Vocês receberam o contrato OpenAPI de uma API que NUNCA viram — e nada mais: sem código, sem wiki, sem ninguém para perguntar. Provem que o contrato basta:

- Parte 1 — Interroguem a especificação YAML do Card 1 e respondam às perguntas do Card 2
- Parte 2 — Comparem com a “documentação em prosa” do Card 3 e registrem o que só a spec responde

*⏱️ Tempo: 30 minutos  |  👥 Formato: em duplas  |  Só vale usar o que está impresso neste handout.*

> **Nomes:** João Antonio   **Turma:** ____________________   **Data:** 22 / 10 / 2026

## CARD 01 — O contrato — especificação OpenAPI (YAML)

Este é TODO o material que o time parceiro enviou. Leiam com calma — cada linha responde a uma pergunta do Card 2.

```text
openapi: 3.1.0
info:
  title: API Biblioteca
  version: 1.0.0
servers:
  - url: https://api.biblioteca.newton.br
paths:
  /api/v1/livros:
    get:
      summary: Lista os livros (paginado)
      parameters:
        - name: page
          in: query
          schema: { type: integer, default: 1 }
      responses:
        "200": { description: Lista de livros }
    post:
      summary: Cadastra um livro
      security:
        - bearerAuth: []
      requestBody:
        required: true
        content:
          application/json:
            schema: { $ref: "#/components/schemas/NovoLivro" }
      responses:
        "201": { description: Livro criado }
        "400": { description: Campos inválidos (ProblemDetails) }
        "401": { description: Token ausente ou inválido }
components:
  securitySchemes:
    bearerAuth:
      type: http
      scheme: bearer
      bearerFormat: JWT
  schemas:
    NovoLivro:
      type: object
      required: [titulo, autor]
      properties:
        titulo: { type: string, maxLength: 120 }
        autor:  { type: string }
        ano:    { type: integer, minimum: 1450 }
```

## CARD 02 — Interrogatório do contrato

Respondam usando SOMENTE o Card 1:

1. Que endpoints/operações existem e o que cada um faz? 
  GET /api/v1/livros: Lista os livros do acervo de forma paginada (aceita o parâmetro de query opcional page, cujo valor padrão é 1). Retorna status 200 (OK) com a lista de livros.

  
  POST /api/v1/livros: Cadastra um novo livro no acervo. Requer autenticação via token Bearer JWT e um corpo de requisição em formato JSON validado pelo schema NovoLivro. Pode retornar 201 (Created) em caso de sucesso, 400 (Bad Request) para campos inválidos ou 401 (Unauthorized) se o token for ausente/inválido.



2. Quais campos são OBRIGATÓRIOS para cadastrar um livro? Que restrições os campos têm?

R:  Titulo: tipo texto (string), com tamanho máximo de 120 caracteres (maxLength: 120).
    Autor: tipo texto (string).
    
3. Em que situações a API devolve 400? E 401? Qual operação exige token — e de que tipo?

R: Status 400 (Bad Request - ProblemDetails): Devolvido no POST /api/v1/livros quando o corpo da requisição contém dados inválidos ou viola as regras do schema (por exemplo: ausência de titulo ou autor, titulo com mais de 120 caracteres, ano inferior a 1450 ou tipos incompatíveis).
Status 401 (Unauthorized): Devolvido no POST /api/v1/livros quando o cabeçalho de autorização não for enviado, o token estiver expirado ou for inválido.
Operação que exige token: Apenas o POST /api/v1/livros (o GET é público e não tem a seção security).
Tipo do token: Token Bearer no formato JWT

4. Montem a requisição POST completa no papel (método, URL, headers, body JSON válido):

R: POST /api/v1/livros HTTP/1.1
Host: api.biblioteca.newton.br
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
{
  "titulo": "Dom Casmurro",
  "autor": "Machado de Assis",
  "ano": 1899
}

## CARD 03 — A mesma API, documentada “em prosa”

Versão que o parceiro tinha antes de adotar OpenAPI:

*“Para cadastrar um livro, envie os dados do livro para o endpoint de livros. É preciso estar autenticado. Os campos incluem título, autor e ano. Em caso de erro, a API retorna uma mensagem apropriada. Também é possível listar os livros cadastrados.”*

1. Listem 3 informações que vocês usaram no Card 2 e que esta prosa NÃO responde com precisão:

R: URL e rota exata: A prosa diz apenas "endpoint de livros", sem informar o caminho /api/v1/livros nem a URL base.

Campos obrigatórios vs opcionais e regras de validação: A prosa afirma que "os campos incluem título, autor e ano", mas omite que apenas título e autor são obrigatórios, que o título tem limite de 120 caracteres e que o ano deve ser no mínimo 1450.

Padrão de autenticação e códigos de erro: A prosa apenas menciona "é preciso estar autenticado" e "retorna mensagem apropriada", sem definir que o token é do tipo Bearer JWT no header Authorization, nem que os códigos HTTP são 201, 400 (ProblemDetails) e 401.

## DESAFIO

1. Mesmo com a spec em mãos, que pergunta a dupla AINDA precisaria fazer ao time da API? (Dica: o contrato diz o que a API promete — não diz tudo sobre como ela se comporta.)

R: Como obter o token JWT? , Qual o formato de resposta do GET /api/v1/livros e o limite de itens por pagina? , Se enviarmos um livro com o mesmo titulo e autor já caadastrado, a API aceita duplicata ou retorna erro de conflito?