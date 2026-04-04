# Copilot Instructions

## General Guidelines
- **Projeto**: Nstech Challenge - Order Services
- **.NET Target**: .NET 9
- **C# Version**: 13.0
- **Persistência**: **EF Core** com **migrations**
- **Banco**: Postgres (via Docker)
- **API**: ASP.NET Core Web API OpenAPI com Swagger
- **Async/await** end-to-end
- **Arquitetura**: Clean Architecture
- **Testes**: xUnit, Bogus, NSubstitute, FluentAssertions
- **Docker**: `docker compose up` deve subir a API + banco

## Code Style
### Clean Architecture:
- Arquitetura Hexagonal (Ports e Adapters)
- Arquitetura Cebola. (Application Core, Infrastructure, Domain Layer, Presentation)
- Arquitetura Limpa. (Use Cases)
- Domain-driven Design (Aggregates, Entities, Value Objects, Repositories e Domain Service)
- Screaming Architecture
- Use PascalCase para classes, métodos e propriedades

### Design Patterns
- **Repository Pattern**: Implementação de repositórios para acesso a dados
- **Unit of Work**: Gerenciamento de transações e coordenação de repositórios
- **CQRS (Command Query Responsibility Segregation)**: Separação de comandos e consultas
- **Migrate Pattern**: Uso de migrations para gerenciamento de schema do banco
- **Result Pattern**: Uso de tipos de resultado para operações que podem falhar, evitando exceções para controle de fluxo
- **Validation**: Validação de entrada usando FluentValidation ou similar
- **Dependency Injection**: Uso de DI para resolver dependências e promover testabilidade
- **MediatR**: Para orquestração de casos de uso (UseCase) e comunicação entre camadas de forma desacoplada
- **Factory Pattern**: Para criação de objetos do Domain
- **Builder Pattern**: Para construção de objetos complexos, especialmente em testes
- **Dto Pattern**: Para transferência de dados entre camadas, especialmente entre Presentation e Application Core
- **Adapter Pattern**: Para integração com serviços externos ou camadas de infraestrutura
- **Singleton Pattern**: Para serviços que devem ter uma única instância, como gerenciadores de configuração ou loggers
- **TripleA (Arrange, Act, Assert)**: Para estruturação de testes unitários, garantindo clareza e organização
- **Idempotency**: Implementação de endpoints idempotentes para operações de confirmação e cancelamento de pedidos, garantindo que múltiplas chamadas resultem no mesmo estado final sem efeitos colaterais adicionais

### 1. AppCore - Domain Layer
**Projeto**: `Nstech.Challenge.OrderServices.AppCore.Domain`
- **Responsabilidade**: Entidades, Value Objects e regras de negócio core
- **Arquivos principais**:
  - `OrderAggregate/Order.cs` - Entidade agregada de Pedido
  - `OrderAggregate/IOrderRepository.cs` - Interface de repositório
  - `Shared_/Entity.cs` - Classe base para entidades
  - `Shared_/ValueObject.cs` - Classe base para value objects
  - `Shared_/ValueResult.cs` - Tipo para resultados de valor

### 2. AppCore - Use Cases Layer
**Projeto**: `Nstech.Challenge.OrderServices.AppCore.UseCases`
- **Responsabilidade**: Casos de uso e lógica de aplicação
- **Arquivos principais**:
  - `Order/Create/CreateOrderUseCase.cs` - Use case para criar pedido
  - `Order/Cancel/CancelOrderUseCase.cs` - Use case para cancelar pedido
  - `Order/Cancel/Dtos_/CancelOrderDto.cs` - DTO para cancelamento (definido como 'sealed record' para melhor imutabilidade e performance)
  - `Order/Cancel/Validators_/CancelOrderDtoValidator.cs` - Validação de DTO com FluentValidation
  - `Shared_/UseCase.cs` - Classe base para casos de uso
  - `Shared_/UseCaseResult.cs` - Tipo de resultado padrão

### 3. Infrastructure
**Projeto**: `Nstech.Challenge.OrderServices.Infrastructure`
- **Responsabilidade**: Implementações de persistência e serviços externos
- **Arquivos principais**:
  - `Database/EfCore/Repositories_/IOrderRepository.cs` - Implementação do repositório com EF Core
  - `DI_/InfraServiceCollectionExtensions.cs` - Injeção de dependências

**Projeto**: `Nstech.Challenge.OrderServices.Infrastructure.Migrations.PostgreSQL`
- **Responsabilidade**: Migrations do Entity Framework Core para PostgreSQL

### 4. Presentation - API
**Projeto**: `Nstech.Challenge.OrderServices.Http.Bff`
- **Responsabilidade**: Controllers e configuração da API
- **Arquivos principais**:
  - `OrderServices/V1/OrderController.cs` - Controller para gerenciamento de pedidos
  - `Auth/AuthController.cs` - Controller de autenticação JWT
  - `Program.cs` - Configuração da aplicação
  - `appsettings.json` - Configurações da aplicação
  - `Properties/launchSettings.json` - Configurações de launch

### 5. Tests
**Projeto**: `Nstech.Challenge.OrderServices.UnitTests`
- **Responsabilidade**: Testes unitários da solução
- **Framework**: xUnit
- **Arquivos principais**:
  - `AppCore/Domain/OrderAggregate/Builders_/OrderBuilder.cs` - Builder para criação de pedidos
  - `AppCore/Domain/OrderAggregate/OrderTests.cs` - Unit tests para a entidade Order
  - `AppCore/UseCases/Order/Create/Builders_/CreateOrderDtoBuilder.cs` - Builder para criação de DTO de criação de pedido
  - `AppCore/UseCases/Order/Create/CreateOrderUseCaseTests.cs` - Testes para criação de pedido

## Project-Specific Rules
### Requisitos não funcionais (MUST)
- Usa Tactical DDD patterns: Entities, Value Objects e Aggregates no projeto Domain (Nstech.Challenge.OrderServices.AppCore.Domain).
- Use Cases implementados no projeto Application Core (Nstech.Challenge.OrderServices.AppCore.UseCases) seguindo o padrão de UseCase e UseCaseResult.
- Repositórios implementados no projeto Infrastructure (Nstech.Challenge.OrderServices.Infrastructure) usando EF Core, com migrations para gerenciamento de schema.
- Controllers e configuração da API no projeto Presentation (Nstech.Challenge.OrderServices.Http.Bff), seguindo as melhores práticas de ASP.NET Core Web API.
- Testes unitários implementados no projeto de testes (Nstech.Challenge.OrderServices.UnitTests) usando xUnit e TripleA(Arrange,Act,Assert), com foco em cobertura de casos de uso e regras de negócio.
- Uso de Docker para facilitar a execução da aplicação e do banco de dados, garantindo que `docker compose up` inicie a API e o PostgreSQL corretamente.
- Migrations do Entity Framework Core devem ser usadas para criar e atualizar o schema do banco de dados, garantindo que a estrutura do banco esteja alinhada com as entidades do Domain.
- Validação de entrada deve ser implementada usando FluentValidation, garantindo que os dados recebidos pela API sejam válidos antes de serem processados pelos casos de uso.
- Autenticação via **JWT**.
- Modelagem e consultas devem ser eficientes, garantindo boa performance da aplicação.
- README com passos para rodar, testar e usar a aplicação, incluindo instruções para configuração do ambiente, execução dos testes e uso da API.

## Requisitos funcionais (MUST)
## Domínio: Order Service
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
	
### 1) Criar pedido

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

### 2) Confirmar pedido (idempotente)

`POST /orders/{id}/confirm`

- Regras:
  - Só confirma pedido em `Placed`
  - Confirmação deve reservar/baixar estoque (conforme sua modelagem)
  - Se o endpoint for chamado 2x, deve manter o mesmo resultado (idempotência)
- Estado:
  - `Placed` -> `Confirmed`

### 3) Cancelar pedido (idempotente)

`POST /orders/{id}/cancel`

- Regras:
  - Pode cancelar `Placed` e `Confirmed`
  - Cancelamento deve liberar estoque reservado (se aplicável)
  - Endpoint deve ser idempotente
- Estado:
  - `Placed/Confirmed` -> `Canceled`

### 4) Consultar pedido

`GET /orders/{id}`

- Deve retornar pedido + itens (DTO adequado)

### 5) Listar pedidos (com paginação e filtro)

`GET /orders?customerId=&status=&from=&to=&page=&pageSize=`

- Paginação obrigatória
- Filtros básicos:
  - `customerId`
  - `status`
  - intervalo de datas (criação)

### Endpoints

- `POST /auth/token`
- `POST /orders`
- `POST /orders/{id}/confirm`
- `POST /orders/{id}/cancel`
- `GET /orders/{id}`
- `GET /orders`
