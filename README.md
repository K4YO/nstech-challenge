# Nstech Challenge — Order Services API

API REST para gerenciamento de pedidos construída com **.NET 9**, **ASP.NET Core**, **Entity Framework Core** e **PostgreSQL**, seguindo os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**.

---

## Índice

- [Pré-requisitos](#pré-requisitos)
- [Início Rápido com Docker](#início-rápido-com-docker)
- [Execução Local (sem Docker)](#execução-local-sem-docker)
- [Migrations](#migrations)
- [Executando os Testes](#executando-os-testes)
- [Autenticação (JWT)](#autenticação-jwt)
- [Endpoints da API](#endpoints-da-api)
  - [POST /api/v1/auth/token](#1-gerar-token-jwt)
  - [POST /api/v1/orders](#2-criar-pedido)
  - [POST /api/v1/orders/{id}/confirm](#3-confirmar-pedido)
  - [POST /api/v1/orders/{id}/cancel](#4-cancelar-pedido)
  - [GET /api/v1/orders/{id}](#5-consultar-pedido)
  - [GET /api/v1/orders](#6-listar-pedidos)
- [Dados Iniciais (Seed)](#dados-iniciais-seed)
- [Arquitetura](#arquitetura)
- [Tecnologias](#tecnologias)

---

## Pré-requisitos

| Ferramenta | Versão mínima |
|------------|---------------|
| [Docker](https://docs.docker.com/get-docker/) | 20.10+ |
| [Docker Compose](https://docs.docker.com/compose/) | 2.0+ |
| [.NET SDK](https://dotnet.microsoft.com/download) (apenas para execução local / testes) | 9.0 |

---

## Início Rápido com Docker

```bash
# 1. Clone o repositório
git clone https://github.com/K4YO/nstech-challenge.git
cd nstech-challenge

# 2. Suba os containers (API + PostgreSQL)
docker compose up --build -d

# 3. Aguarde os containers ficarem prontos
docker compose logs -f api
```

A API estará disponível em **http://localhost:8080**.

O Swagger UI estará acessível em **http://localhost:8080/swagger**.

> **Nota:** As migrations e os dados iniciais (seed) são aplicados automaticamente na inicialização da API. Não é necessário executar nenhum comando adicional.

### Parar os containers

```bash
docker compose down
```

### Parar e remover os volumes (limpar banco)

```bash
docker compose down -v
```

---

## Execução Local (sem Docker)

### 1. Subir apenas o PostgreSQL via Docker

```bash
docker compose up postgres -d
```

### 2. Executar a API

```bash
dotnet run --project src/Nstech.Challenge.OrderServices.Presentation.Bff
```

A API estará disponível em **http://localhost:5000** (ou a porta definida em `launchSettings.json`).

---

## Migrations

As migrations do Entity Framework Core são **aplicadas automaticamente** ao iniciar a aplicação (tanto via Docker quanto localmente).

### Aplicar migrations manualmente (se necessário)

```bash
dotnet ef database update \
  --project src/Nstech.Challenge.OrderServices.Infrastructure.Migrations.PostgreSQL \
  --startup-project src/Nstech.Challenge.OrderServices.Presentation.Bff
```

### Criar uma nova migration

```bash
dotnet ef migrations add <NomeDaMigration> \
  --project src/Nstech.Challenge.OrderServices.Infrastructure.Migrations.PostgreSQL \
  --startup-project src/Nstech.Challenge.OrderServices.Presentation.Bff
```

---

## Executando os Testes

```bash
# Executar todos os testes unitários
dotnet test

# Executar com verbosidade detalhada
dotnet test --verbosity normal

# Executar com cobertura de código
dotnet tool install -g dotnet-coverage
dotnet-coverage collect -f cobertura -o coverage.cobertura.xml dotnet test
```

---

## Autenticação (JWT)

Todos os endpoints de pedidos (`/api/v1/orders/**`) exigem autenticação via **Bearer Token JWT**.

O fluxo é:

1. Gerar um token via `POST /api/v1/auth/token`
2. Incluir o token no header `Authorization: Bearer <token>` das requisições seguintes

---

## Endpoints da API

> **Base URL:** `http://localhost:8080/api/v1`

### 1. Gerar Token JWT

```
POST /api/v1/auth/token
```

**Request Body:**

```json
{
  "username": "admin",
  "password": "admin"
}
```

**Response `200 OK`:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

**cURL:**

```bash
curl -X POST http://localhost:8080/api/v1/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin"}'
```

> **Dica:** Salve o token retornado em uma variável para usar nas próximas requisições:
> ```bash
> TOKEN=$(curl -s -X POST http://localhost:8080/api/v1/auth/token \
>   -H "Content-Type: application/json" \
>   -d '{"username": "admin", "password": "admin"}' | jq -r '.token')
> ```

---

### 2. Criar Pedido

```
POST /api/v1/orders
```

**Headers:** `Authorization: Bearer <token>`

**Request Body:**

```json
{
  "customerId": "f0f0f0f0-f0f0-f0f0-f0f0-f0f0f0f0f0f0",
  "currency": "USD",
  "items": [
    {
      "productId": "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d",
      "quantity": 2
    },
    {
      "productId": "b2c3d4e5-f6a7-4b5c-8d9e-1f2a3b4c5d6e",
      "quantity": 1
    }
  ]
}
```

**Response `201 Created`:**

```json
{
  "orderId": "c8a7b6d5-e4f3-2a1b-9c8d-7e6f5a4b3c2d",
  "status": "Placed"
}
```

**Regras de negócio:**
- Pedido deve conter pelo menos um item
- Quantidade de cada item deve ser > 0
- Produto deve existir no catálogo
- Estoque disponível deve ser suficiente
- O total é calculado automaticamente (`unitPrice × quantity` de cada item)
- O pedido nasce com status `Placed`

**cURL:**

```bash
curl -X POST http://localhost:8080/api/v1/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "customerId": "f0f0f0f0-f0f0-f0f0-f0f0-f0f0f0f0f0f0",
    "currency": "USD",
    "items": [
      {"productId": "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d", "quantity": 2},
      {"productId": "b2c3d4e5-f6a7-4b5c-8d9e-1f2a3b4c5d6e", "quantity": 1}
    ]
  }'
```

---

### 3. Confirmar Pedido

```
POST /api/v1/orders/{id}/confirm
```

**Headers:** `Authorization: Bearer <token>`

**Response `200 OK`:**

```json
{
  "orderId": "c8a7b6d5-e4f3-2a1b-9c8d-7e6f5a4b3c2d",
  "status": "Confirmed"
}
```

**Regras de negócio:**
- Apenas pedidos com status `Placed` podem ser confirmados
- A confirmação reserva/baixa o estoque dos produtos
- **Idempotente:** chamar duas vezes retorna o mesmo resultado sem efeitos colaterais

**cURL:**

```bash
curl -X POST http://localhost:8080/api/v1/orders/{ORDER_ID}/confirm \
  -H "Authorization: Bearer $TOKEN"
```

---

### 4. Cancelar Pedido

```
POST /api/v1/orders/{id}/cancel
```

**Headers:** `Authorization: Bearer <token>`

**Response `200 OK`:**

```json
{
  "orderId": "c8a7b6d5-e4f3-2a1b-9c8d-7e6f5a4b3c2d",
  "status": "Canceled"
}
```

**Regras de negócio:**
- Pedidos com status `Placed` ou `Confirmed` podem ser cancelados
- Se o pedido estava `Confirmed`, o estoque reservado é liberado
- **Idempotente:** chamar duas vezes retorna o mesmo resultado sem efeitos colaterais

**cURL:**

```bash
curl -X POST http://localhost:8080/api/v1/orders/{ORDER_ID}/cancel \
  -H "Authorization: Bearer $TOKEN"
```

---

### 5. Consultar Pedido

```
GET /api/v1/orders/{id}
```

**Headers:** `Authorization: Bearer <token>`

**Response `200 OK`:**

```json
{
  "id": "11111111-1111-1111-1111-111111111111",
  "customerId": "f0f0f0f0-f0f0-f0f0-f0f0-f0f0f0f0f0f0",
  "status": "Placed",
  "currency": "USD",
  "items": [
    {
      "productId": "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d",
      "unitPrice": 99.99,
      "quantity": 2,
      "total": 199.98
    }
  ],
  "total": 199.98,
  "createdAt": "2025-01-15T12:00:00Z"
}
```

**cURL:**

```bash
curl -X GET http://localhost:8080/api/v1/orders/11111111-1111-1111-1111-111111111111 \
  -H "Authorization: Bearer $TOKEN"
```

---

### 6. Listar Pedidos

```
GET /api/v1/orders?customerId=&status=&fromDate=&toDate=&page=&pageSize=
```

**Headers:** `Authorization: Bearer <token>`

**Parâmetros de Query (todos opcionais):**

| Parâmetro    | Tipo     | Descrição                                             | Padrão |
|-------------|----------|-------------------------------------------------------|--------|
| `customerId` | `Guid`   | Filtrar por ID do cliente                             | —      |
| `status`     | `string` | Filtrar por status: `Draft`, `Placed`, `Confirmed`, `Canceled` | —      |
| `fromDate`   | `DateTime` | Filtrar pedidos criados a partir desta data         | —      |
| `toDate`     | `DateTime` | Filtrar pedidos criados até esta data               | —      |
| `page`       | `int`    | Número da página (mínimo: 1)                         | `1`    |
| `pageSize`   | `int`    | Itens por página (mínimo: 1, máximo: 100)            | `10`   |

**Response `200 OK`:**

```json
{
  "orders": [
    {
      "id": "11111111-1111-1111-1111-111111111111",
      "customerId": "f0f0f0f0-f0f0-f0f0-f0f0-f0f0f0f0f0f0",
      "status": "Placed",
      "currency": "USD",
      "total": 199.98,
      "createdAt": "2025-01-15T12:00:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

**Exemplos de cURL:**

```bash
# Listar todos os pedidos (página 1, 10 por página)
curl -X GET "http://localhost:8080/api/v1/orders" \
  -H "Authorization: Bearer $TOKEN"

# Filtrar por cliente
curl -X GET "http://localhost:8080/api/v1/orders?customerId=f0f0f0f0-f0f0-f0f0-f0f0-f0f0f0f0f0f0" \
  -H "Authorization: Bearer $TOKEN"

# Filtrar por status
curl -X GET "http://localhost:8080/api/v1/orders?status=Placed" \
  -H "Authorization: Bearer $TOKEN"

# Filtrar por período e paginação
curl -X GET "http://localhost:8080/api/v1/orders?fromDate=2025-01-01&toDate=2025-12-31&page=1&pageSize=5" \
  -H "Authorization: Bearer $TOKEN"

# Combinando filtros
curl -X GET "http://localhost:8080/api/v1/orders?customerId=f0f0f0f0-f0f0-f0f0-f0f0-f0f0f0f0f0f0&status=Placed&page=1&pageSize=20" \
  -H "Authorization: Bearer $TOKEN"
```

---

## Fluxo Completo de Teste

Execute os comandos abaixo em sequência para testar o fluxo completo de um pedido:

```bash
# 1. Gerar token
TOKEN=$(curl -s -X POST http://localhost:8080/api/v1/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin"}' | jq -r '.token')

echo "Token: $TOKEN"

# 2. Criar pedido
ORDER=$(curl -s -X POST http://localhost:8080/api/v1/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "customerId": "f0f0f0f0-f0f0-f0f0-f0f0-f0f0f0f0f0f0",
    "currency": "USD",
    "items": [
      {"productId": "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d", "quantity": 1},
      {"productId": "d4e5f6a7-b8c9-4d5e-8f9a-3b4c5d6e7f8a", "quantity": 3}
    ]
  }')

ORDER_ID=$(echo $ORDER | jq -r '.orderId')
echo "Pedido criado: $ORDER_ID"
echo $ORDER | jq .

# 3. Consultar pedido
echo "--- Consultar pedido ---"
curl -s -X GET "http://localhost:8080/api/v1/orders/$ORDER_ID" \
  -H "Authorization: Bearer $TOKEN" | jq .

# 4. Confirmar pedido
echo "--- Confirmar pedido ---"
curl -s -X POST "http://localhost:8080/api/v1/orders/$ORDER_ID/confirm" \
  -H "Authorization: Bearer $TOKEN" | jq .

# 5. Confirmar novamente (idempotência)
echo "--- Confirmar novamente (idempotente) ---"
curl -s -X POST "http://localhost:8080/api/v1/orders/$ORDER_ID/confirm" \
  -H "Authorization: Bearer $TOKEN" | jq .

# 6. Cancelar pedido
echo "--- Cancelar pedido ---"
curl -s -X POST "http://localhost:8080/api/v1/orders/$ORDER_ID/cancel" \
  -H "Authorization: Bearer $TOKEN" | jq .

# 7. Cancelar novamente (idempotência)
echo "--- Cancelar novamente (idempotente) ---"
curl -s -X POST "http://localhost:8080/api/v1/orders/$ORDER_ID/cancel" \
  -H "Authorization: Bearer $TOKEN" | jq .

# 8. Listar pedidos do cliente
echo "--- Listar pedidos do cliente ---"
curl -s -X GET "http://localhost:8080/api/v1/orders?customerId=f0f0f0f0-f0f0-f0f0-f0f0-f0f0f0f0f0f0" \
  -H "Authorization: Bearer $TOKEN" | jq .
```

> **Pré-requisito:** [jq](https://jqlang.github.io/jq/download/) para formatação do JSON no terminal. Caso não tenha `jq`, remova `| jq .` dos comandos.

---

## Dados Iniciais (Seed)

A migration de seed insere dados para facilitar os testes:

### Produtos disponíveis

| SKU       | ID                                     | Preço    | Estoque |
|-----------|----------------------------------------|----------|---------|
| PROD-001  | `a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d` | 99.99   | 100     |
| PROD-002  | `b2c3d4e5-f6a7-4b5c-8d9e-1f2a3b4c5d6e` | 49.99   | 50      |
| PROD-003  | `c3d4e5f6-a7b8-4c5d-8e9f-2a3b4c5d6e7f` | 149.99  | 25      |
| PROD-004  | `d4e5f6a7-b8c9-4d5e-8f9a-3b4c5d6e7f8a` | 29.99   | 200     |
| PROD-005  | `e5f6a7b8-c9d0-4e5f-9a0b-4c5d6e7f8a9b` | 199.99  | 15      |
| PROD-006  | `f6a7b8c9-d0e1-4f5a-0b1c-5d6e7f8a9b0c` | 9.99    | 500     |
| PROD-007  | `a7b8c9d0-e1f2-4a5b-1c2d-6e7f8a9b0c1d` | 74.50   | 60      |
| PROD-008  | `b8c9d0e1-f2a3-4b5c-2d3e-7f8a9b0c1d2e` | 349.90  | 10      |

### Clientes de teste

| Cliente   | ID                                     |
|-----------|----------------------------------------|
| Cliente 1 | `f0f0f0f0-f0f0-f0f0-f0f0-f0f0f0f0f0f0` |
| Cliente 2 | `a0a0a0a0-a0a0-a0a0-a0a0-a0a0a0a0a0a0` |
| Cliente 3 | `b1b1b1b1-b1b1-b1b1-b1b1-b1b1b1b1b1b1` |

### Pedidos pré-existentes

| Order ID | Cliente   | Status      | Moeda | Total    |
|----------|-----------|-------------|-------|----------|
| `11111111-...` | Cliente 1 | Placed    | USD   | 199.98  |
| `22222222-...` | Cliente 1 | Confirmed | USD   | 249.97  |
| `33333333-...` | Cliente 2 | Canceled  | BRL   | 59.98   |
| `44444444-...` | Cliente 2 | Placed    | BRL   | 399.98  |
| `55555555-...` | Cliente 3 | Confirmed | EUR   | 224.49  |
| `66666666-...` | Cliente 3 | Canceled  | EUR   | 349.90  |
| `77777777-...` | Cliente 1 | Placed    | USD   | 89.97   |
| `88888888-...` | Cliente 2 | Confirmed | BRL   | 449.85  |

---

## Arquitetura

O projeto segue **Clean Architecture** com **DDD Tático**:

```
src/
??? Nstech.Challenge.OrderServices.AppCore.Domain          # Entidades, Value Objects, Aggregates, Interfaces de repositório
??? Nstech.Challenge.OrderServices.AppCore.UseCases         # Casos de uso, DTOs, Validators (FluentValidation)
??? Nstech.Challenge.OrderServices.Infrastructure           # EF Core, Repositórios, Unit of Work
??? Nstech.Challenge.OrderServices.Infrastructure.Migrations.PostgreSQL  # Migrations EF Core
??? Nstech.Challenge.OrderServices.Presentation.Bff         # Controllers, Configuração da API, JWT

tests/
??? Nstech.Challenge.OrderServices.UnitTests                # Testes unitários (xUnit + FluentAssertions + NSubstitute)
```

### Fluxo de dependência

```
Presentation ? UseCases ? Domain
                  ?
Infrastructure ????
```

### Padrões utilizados

- **Repository Pattern** — Acesso a dados via interfaces
- **Unit of Work** — Transações atômicas
- **CQRS** — Separação de comandos e consultas
- **Result Pattern** — Tratamento de erros sem exceções
- **Factory Pattern** — Criação de entidades do domínio via métodos estáticos `Create`
- **Builder Pattern** — Construção de objetos complexos nos testes
- **Idempotency** — Endpoints de confirmar e cancelar são idempotentes

---

## Tecnologias

| Tecnologia | Versão |
|------------|--------|
| .NET | 9.0 |
| ASP.NET Core | 9.0 |
| Entity Framework Core | 9.x |
| PostgreSQL | 16 |
| FluentValidation | 11.x |
| JWT (Microsoft.IdentityModel) | — |
| xUnit | 2.8 |
| FluentAssertions | 6.12 |
| NSubstitute | 5.1 |
| Bogus | 35.5 |
| Docker | — |
