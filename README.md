# Teste Técnico — Desenvolvedor(a) .NET Sênior (3 dias)

> **Objetivo:** avaliar arquitetura, qualidade de código, domínio, testes, performance, boas práticas e entrega “pronta para produção” (no essencial).
>
> **Tempo sugerido:** até **3 dias corridos**.

---

## Como entregar

1. Implemente a solução conforme o enunciado.
2. Ao final, envie:
   - Link do repositório
   - Instruções para rodar (README)
   - Observações/decisões técnicas (no README ou em `/docs/decisions.md`)

---

## Stack e restrições

- **.NET**: preferencialmente **.NET 8+**
- **Linguagem**: C#
- **Persistência**: **EF Core** com **migrations**
- **Banco**: Postgres (via Docker)
- **API**: requisito ASP.NET com Minimal API 
- **Async/await** end-to-end
- **Arquitetura**: separar claramente **Domain / Application / Infrastructure / API**
- **Testes**: requisito xUnit 
- **Docker**: `docker compose up` deve subir a API + banco
- **Mediator**: (Opcional) Saber utilizar mediator.
- **CQRS**: (Opcional) Demonstrar separação entre Command e Query (CQRS simplificado).
- **Behaviors**: (Opcional) Desejável uso de fastfail e problem details em tratamentos globais.

---

## Domínio: Order Service

Você irá construir uma API REST para gestão de **Pedidos** com itens e validação de estoque.

### Entidades (mínimo sugerido)

- **Order**
  - Id
  - CustomerId
  - Status (`Draft`, `Placed`, `Confirmed`, `Canceled`)
  - Currency
  - Itens
  - Total
  - CreatedAt
- **OrderItem**
  - ProductId
  - UnitPrice
  - Quantity
- **Product / Stock** (pode ser simples)
  - ProductId
  - UnitPrice
  - AvailableQuantity (ou modelo de estoque equivalente)

> Você tem liberdade para ajustar o modelo desde que as regras sejam atendidas.

---

## Requisitos funcionais (MUST)

### 1) Crud simples de Produto
- Dominio anêmico

### 2) Criar pedido

`POST /orders`

- Payload mínimo:
  - `customerId`
  - `currency`
  - `items: [{ productId, quantity }]`
- Regras:
  - Não pode criar pedido sem itens
  - Quantidade deve ser > 0
  - Produto deve existir
  - **Não pode exceder estoque disponível** (conforme seu modelo)
  - Total do pedido = soma (`unitPrice * quantity`) de cada item
- Estado:
  - pedido nasce como `Placed` (ou `Draft` -> `Placed`, se preferir justificar)

### 3) Confirmar pedido (idempotente)

`POST /orders/{id}/confirm`

- Regras:
  - Só confirma pedido em `Placed`
  - Confirmação deve reservar/baixar estoque (conforme sua modelagem)
  - Se o endpoint for chamado 2x, deve manter o mesmo resultado (idempotência)
- Estado:
  - `Placed` -> `Confirmed`

### 4) Cancelar pedido (idempotente)

`POST /orders/{id}/cancel`

- Regras:
  - Pode cancelar `Placed` e `Confirmed`
  - Cancelamento deve liberar estoque reservado (se aplicável)
  - Endpoint deve ser idempotente
- Estado:
  - `Placed/Confirmed` -> `Canceled`

### 5) Consultar pedido

`GET /orders/{id}`

- Deve retornar pedido + itens (DTO adequado)

### 6) Listar pedidos (com paginação e filtro)

`GET /orders?customerId=&status=&from=&to=&page=&pageSize=`

- Paginação obrigatória
- Filtros básicos:
  - `customerId`
  - `status`
  - intervalo de datas (criação)

---

## Requisitos não funcionais (MUST)

### Vertical Slices / SOLID

- A separação de responsabilidades existe (Domain / Application / Infrastructure), porém a organização do código deve ser feita por Vertical Slices (por feature).

### Concorrência

- A confirmação do pedido deve ser segura sob concorrência e não pode permitir estoque negativo/inconsistente. Descreva no README a estratégia adotada. Exemplos aceitos:
- - Controle otimista com ConcurrencyToken/RowVersion e retorno 409 Conflict em caso de conflito; ou
- - Atualização atômica no banco (ex.: UPDATE ... WHERE available >= quantity) validando “linhas afetadas”.

### CancellationToken

- Endpoints, handlers e repositórios devem aceitar CancellationToken e propagar para operações assíncronas (EF Core/I/O), mantendo a cadeia de cancelamento.

### Testes (TDD-friendly)

- Cobertura de testes para regras de negócio, casos de borda e cenários importantes.
- Testes devem rodar com `dotnet test`.

### Segurança (mínimo)

- Autenticação via **JWT**.

### Performance (mínimo)

- Modelagem e consultas devem ser eficientes.

### Operacional

- `docker compose up` sobe API + banco.
- Migrations aplicadas automaticamente.
- README com passos para rodar, testar e usar.

---

## Endpoints

- `POST /auth/token`
- `POST /orders`
- `POST /orders/{id}/confirm`
- `POST /orders/{id}/cancel`
- `GET /orders/{id}`
- `GET /orders`

---

## O que avaliamos

- Qualidade da arquitetura e separação de responsabilidades
- DDD prático: invariantes, coesão, modelagem, value objects (se aplicável)
- SOLID e Clean Code (nomes, acoplamento, legibilidade)
- Testes: cobertura de regras e casos importantes, legibilidade
- EF Core: modelagem, migrations, queries, tracking/performance, desejavel: sabe usar o Dapper?
- Segurança: auth JWT e autorização básica
- Entrega: Docker, README, experiência de rodar

---

## Checklist mínimo (para considerarmos “completo”)

- [ ] API roda local e via Docker
- [ ] Migrations aplicadas automaticamente
- [ ] Endpoints MUST implementados
- [ ] JWT + autorização básica funcionando
- [ ] `dotnet test` passando
- [ ] README com passo a passo

---

## Próximas etapas

- Após a entrega, avaliaremos o projeto e, após o cumprimento dos requisitos, teremos uma conversa técnica para discutir suas escolhas, desafios e aprendizados.

Boa sorte e boa entrega!
 
