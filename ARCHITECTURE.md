# Documentação Arquitetural - IdentityService

## 📋 Índice
1. [Visão Geral](#visão-geral)
2. [Padrão Arquitetural](#padrão-arquitetural)
3. [Estrutura do Projeto](#estrutura-do-projeto)
4. [Stack Tecnológico](#stack-tecnológico)
5. [Componentes Principais](#componentes-principais)
6. [Fluxo de Dados](#fluxo-de-dados)
7. [Autenticação e Autorização](#autenticação-e-autorização)
8. [Camadas da Aplicação](#camadas-da-aplicação)
9. [Padrões e Convenções](#padrões-e-convenções)
10. [Injeção de Dependência](#injeção-de-dependência)
11. [Testes](#testes)
12. [Configuração e Deployment](#configuração-e-deployment)

---

## 🎯 Visão Geral

**IdentityService** é um serviço de identidade construído com **Clean Architecture**, seguindo princípios de design orientado por domínio (DDD) e separação clara de responsabilidades. O projeto implementa um sistema robusto de autenticação e autorização baseado em JWT com suporte a tokens Bearer.

**Versão do Template**: Clean.Architecture.Solution.Template 10.8.0  
**Framework**: .NET 9.0  
**Banco de Dados**: PostgreSQL 16  
**Runtime**: Docker & .NET Aspire

---

## 🏗️ Padrão Arquitetural

### Clean Architecture

O projeto segue os princípios de **Clean Architecture** proposto por Robert C. Martin, organizando o código em camadas concêntricas com dependências apontando sempre para o centro:

```
┌─────────────────────────────────────────┐
│         Presentation (Web/API)          │
├─────────────────────────────────────────┤
│     Application (Use Cases/Business)    │
├─────────────────────────────────────────┤
│         Domain (Entities/Rules)         │
├─────────────────────────────────────────┤
│     Infrastructure (External Services)  │
└─────────────────────────────────────────┘
```

### Benefícios

- ✅ **Independência de Framework**: A lógica de negócio não depende do framework ASP.NET
- ✅ **Testabilidade**: Fácil de testar, especialmente a camada de negócio
- ✅ **Manutenibilidade**: Código organizado e bem estruturado
- ✅ **Escalabilidade**: Fácil adicionar novas funcionalidades sem quebrar existentes
- ✅ **Flexibilidade**: Troca de tecnologias sem impacto na lógica de negócio

---

## 📁 Estrutura do Projeto

```
IdentityService/
├── src/
│   ├── Domain/                    # Camada de Domínio (Entidades, Regras de Negócio)
│   │   ├── Entities/              # Entidades de domínio
│   │   ├── ValueObjects/          # Objetos de valor
│   │   ├── Events/                # Eventos de domínio
│   │   ├── Enums/                 # Enumerações
│   │   ├── Exceptions/            # Exceções de negócio
│   │   └── Constants/             # Constantes
│   │
│   ├── Application/               # Camada de Aplicação (Use Cases)
│   │   ├── Auth/                  # Use cases de autenticação
│   │   ├── Common/                # Comportamentos, interfaces, utilitários
│   │   │   ├── Behaviours/        # Middlewares MediatR
│   │   │   ├── Interfaces/        # Contratos/Interfaces
│   │   │   └── Exceptions/        # Exceções da aplicação
│   │   ├── WeatherForecasts/      # Exemplo: use cases de exemplo
│   │   └── DependencyInjection.cs # Configuração de serviços
│   │
│   ├── Infrastructure/            # Camada de Infraestrutura
│   │   ├── Data/                  # DbContext, Migrations, Seeds
│   │   ├── Identity/              # Implementação de Identity
│   │   ├── Auth/                  # Serviços de autenticação (JWT, etc)
│   │   └── DependencyInjection.cs # Configuração de serviços
│   │
│   ├── Web/                       # Camada de Apresentação (API)
│   │   ├── Endpoints/             # Endpoints da API
│   │   ├── Services/              # Serviços da API (ex: CurrentUser)
│   │   ├── ClientApp/             # SPA Frontend (opcional)
│   │   ├── Program.cs             # Configuração e setup da API
│   │   ├── DependencyInjection.cs # Injeção de dependência
│   │   └── appsettings.json       # Configurações
│   │
│   ├── Shared/                    # Código compartilhado
│   │   └── Services.cs            # Constantes compartilhadas
│   │
│   └── AppHost/                   # .NET Aspire App Host
│       ├── Program.cs             # Orquestração de serviços
│       └── appsettings.json       # Configurações
│
├── tests/
│   ├── Domain.UnitTests/          # Testes unitários do domínio
│   ├── Application.UnitTests/     # Testes unitários da aplicação
│   ├── Infrastructure.IntegrationTests/  # Testes de integração
│   ├── Application.FunctionalTests/      # Testes E2E
│   └── TestAppHost/               # Aspire para testes
│
├── artifacts/                     # Artefatos de build
├── Dockerfile                     # Configuração Docker
├── docker-compose.yml             # Orquestração local
├── Directory.Build.props          # Propriedades globais de build
├── Directory.Packages.props       # Versões centralizadas de pacotes
├── global.json                    # Versão do .NET SDK
├── IdentityService.slnx           # Solução (.slnx)
└── README.md                      # Guia de início rápido
```

---

## 🔧 Stack Tecnológico

### Backend
| Tecnologia | Versão | Propósito |
|-----------|--------|----------|
| **.NET** | 9.0 | Framework principal |
| **ASP.NET Core** | 9.0 | Web framework |
| **Entity Framework Core** | 9.0 | ORM para acesso a dados |
| **PostgreSQL** | 16 | Banco de dados relacional |
| **MediatR** | 12.x | CQRS, mediação de comandos/queries |
| **FluentValidation** | 11.x | Validação de dados |
| **AutoMapper** | 13.x | Mapeamento de objetos |
| **Scalar** | - | Documentação OpenAPI interativa |

### Frontend
| Tecnologia | Propósito |
|-----------|----------|
| **Node.js** | 20.x - Runtime para build SPA |
| **SPA** | Cliente web (opcional) |

### DevOps & Observabilidade
| Tecnologia | Propósito |
|-----------|----------|
| **Docker** | Containerização |
| **.NET Aspire** | Orquestração de serviços e observabilidade |
| **Azure Container Apps** | Deployment em produção |
| **Azure Key Vault** | Gerenciamento de secrets |
| **Azure PostgreSQL** | Banco de dados gerenciado |

---

## 🧩 Componentes Principais

### 1. **Domain Layer** (src/Domain)
Contém a lógica pura de negócio, independente de qualquer framework.

**Responsabilidades:**
- Definir entidades de domínio
- Definir regras de negócio
- Lançar eventos de domínio
- Definir exceções de negócio

**⚠️ Boas Práticas de Chave Primária:**
- ✅ Use **GUID (Guid)** como chave primária em todas as entidades
- ✅ Benefícios: distribuição, sem conflitos em replicação, segurança
- ❌ Evite usar inteiros sequenciais (int) em produção

**Exemplo de Entidade:**
```csharp
// Exemplo teórico - com GUID como chave primária
public class User : BaseEntity
{
    // ✅ BaseEntity já fornece Id como Guid
    // public Guid Id { get; set; } // Herdado
    
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public bool IsActive { get; set; }
    
    // Métodos de domínio
    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        AddDomainEvent(new PasswordChangedEvent(Id)); // Id é Guid
    }
}

// ✅ SEMPRE use Guid como chave primária
// Benefits:
// - Segurança: não expõe sequência previsível
// - Distribuição: sem conflitos em sistemas distribuídos
// - Replicação: suporta merge sem conflitos```
```

### 2. **Application Layer** (src/Application)
Implementa os casos de uso e regras de negócio da aplicação.

**Componentes:**
- **Commands & Queries**: Implementam CQRS
- **Handlers**: Processam commands/queries
- **DTOs**: Data Transfer Objects
- **Validators**: Validação de entrada
- **Behaviours**: Cross-cutting concerns (logging, autorização, validação)

**Exemplo de Command:**
```csharp
// Exemplo teórico
public class CreateTodoItemCommand : IRequest<int>
{
    public string Title { get; set; }
}

public class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, int>
{
    private readonly IApplicationDbContext _context;
    
    public async Task<int> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var entity = new TodoItem { Title = request.Title };
        _context.TodoItems.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
```

### 3. **Infrastructure Layer** (src/Infrastructure)
Implementa detalhes técnicos e serviços externos.

**Responsabilidades:**
- **Data**: DbContext, Migrations, Seeds, Interceptadores
- **Identity**: Gerenciamento de usuários e roles
- **Auth**: JWT Tokens, Token Service
- **Integração com serviços externos**: APIs, cache, etc.

**DbContext:**
```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    // DbSets para entidades
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<TodoList> TodoLists => Set<TodoList>();
}
```

### 4. **Web Layer** (src/Web)
API REST e apresentação.

**Responsabilidades:**
- Definir endpoints da API
- Mapear requisições HTTP para use cases
- Retornar respostas formatadas
- Autenticação/Autorização

**Exemplo de Endpoint:**
```csharp
// Endpoints seriam mapeados na camada Web
public class TodoEndpoints
{
    public static void MapTodoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/todos")
            .WithName("Todos")
            .RequireAuthorization();
        
        group.MapGet("/", GetTodos);
        group.MapPost("/", CreateTodo);
    }
    
    private static async Task<IResult> GetTodos(ISender sender)
    {
        var result = await sender.Send(new GetTodosQuery());
        return Results.Ok(result);
    }
}
```

### 5. **AppHost** (src/AppHost)
Orquestração de serviços usando .NET Aspire.

```csharp
var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment("aca-env");

var databaseServer = builder
    .AddAzurePostgresFlexibleServer("database-server")
    .WithPasswordAuthentication()
    .AddDatabase("identityservice-db");

var web = builder.AddProject<Projects.Web>("web-api")
    .WithReference(databaseServer)
    .WaitFor(databaseServer);

builder.Build().Run();
```

---

## 🔄 Fluxo de Dados

### Requisição HTTP → Resposta

```
┌─────────────────────────────────────────────────────────────┐
│ 1. HTTP Request → Endpoint (Web Layer)                     │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. Mapear Request → MediatR Command/Query                  │
│    (Program.MapEndpoints() → Endpoints)                    │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. MediatR Pipeline (Behaviours)                           │
│    ├─ LoggingBehaviour          (Logging)                  │
│    ├─ AuthorizationBehaviour    (Autorização)             │
│    ├─ ValidationBehaviour       (Validação)               │
│    └─ PerformanceBehaviour      (Performance)             │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. Handler (Application Layer)                             │
│    ├─ Buscar dados (IApplicationDbContext)                │
│    ├─ Executar lógica de negócio                          │
│    └─ Publicar eventos de domínio                         │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. Infrastructure Services                                 │
│    ├─ Entity Framework (Data Access)                      │
│    ├─ Identity Service (Usuários/Roles)                   │
│    └─ Token Service (JWT)                                 │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. Database Operations (PostgreSQL)                        │
│    └─ SaveChangesAsync() → Persist Data                   │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 7. HTTP Response (Web Layer)                              │
│    └─ Mapper (AutoMapper) DTO → JSON                      │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔐 Autenticação e Autorização

### Estratégia de Segurança

#### Autenticação: JWT Bearer Tokens
- **Access Token**: Duração curta (padrão: 15 minutos)
- **Refresh Token**: Duração longa (padrão: 7 dias)

#### Autorização: Role-Based (RBAC)
```csharp
// No Endpoint
group.RequireAuthorization() // Autenticado
group.RequireAuthorization("admin") // Role específico
group.RequireAuthorization("admin", "manager") // Multiple roles
```

### Configuração de Autenticação (Infrastructure)

```csharp
// src/Infrastructure/DependencyInjection.cs
builder.Services.AddAuthentication()
    .AddBearerToken(IdentityConstants.BearerScheme);

builder.Services.AddAuthorizationBuilder();

builder.Services
    .AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints(); // Mapeia automaticamente endpoints de identity
```

### Fluxo de Login

```
┌──────────────────────────────────────────────┐
│ 1. User: POST /login                         │
│    { email, password }                       │
└──────────────────────────────────────────────┘
            ↓
┌──────────────────────────────────────────────┐
│ 2. Validar credenciais (Identity Service)   │
│    └─ Verificar hash de senha                │
└──────────────────────────────────────────────┘
            ↓
┌──────────────────────────────────────────────┐
│ 3. Gerar JWT Token                          │
│    └─ TokenService.GenerateToken()           │
└──────────────────────────────────────────────┘
            ↓
┌──────────────────────────────────────────────┐
│ 4. Retornar Tokens                          │
│    {                                         │
│      "access_token": "...",                 │
│      "refresh_token": "...",                │
│      "expires_in": 900                      │
│    }                                         │
└──────────────────────────────────────────────┘
```

### Variáveis de Configuração (docker-compose.yml)

```env
Jwt__Secret=my_super_secret_jwt_key_that_is_long_enough_32bytes!
Jwt__Issuer=IdentityService
Jwt__Audience=IdentityService.Clients
Jwt__AccessTokenExpirationMinutes=15
Jwt__RefreshTokenExpirationDays=7
```

---

## 🏢 Camadas da Aplicação

### **Domain Layer** (Camada de Domínio)

Responsabilidade: **Lógica de negócio pura**

```
Domain/
├── Entities/              # Entidades com identidade (tem ciclo de vida)
├── ValueObjects/          # Objetos sem identidade (imutáveis)
├── Events/               # Eventos de domínio
├── Exceptions/           # Exceções de negócio
├── Enums/                # Enumerações
└── Constants/            # Constantes do domínio
```

**Características:**
- ❌ Sem dependências externas (exceto C# base)
- ❌ Sem Entity Framework
- ✅ Métodos que implementam regras de negócio
- ✅ Lança eventos de domínio
- ✅ Pode ser testado sem dependências

---

### **Application Layer** (Camada de Aplicação)

Responsabilidade: **Orquestração de use cases**

```
Application/
├── Auth/                 # Use cases de autenticação
├── WeatherForecasts/     # Exemplo de feature
│   ├── Commands/         # Comandos (escrita)
│   ├── Queries/          # Queries (leitura)
│   ├── EventHandlers/    # Manipuladores de eventos de domínio
│   └── Validators/       # Validadores
├── Common/
│   ├── Behaviours/       # MediatR Behaviours (pipeline)
│   ├── Interfaces/       # Contratos
│   ├── Exceptions/       # Exceções da aplicação
│   ├── Mappings/         # AutoMapper profiles
│   └── Wrappers/         # Wrappers para resultados
└── DependencyInjection.cs
```

**MediatR Behaviours (Pipeline):**

1. **LoggingBehaviour**: Registra entrada/saída de requests
2. **AuthorizationBehaviour**: Valida permissões
3. **ValidationBehaviour**: Executa validações (FluentValidation)
4. **PerformanceBehaviour**: Monitora performance
5. **UnhandledExceptionBehaviour**: Trata exceções não capturadas

---

### **Infrastructure Layer** (Camada de Infraestrutura)

Responsabilidade: **Detalhes técnicos e serviços externos**

```
Infrastructure/
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── Migrations/          # EF Core Migrations
│   ├── Configurations/      # Entity type configurations
│   ├── Interceptors/        # Interceptadores (Audit, Events)
│   └── Seeding/             # Dados iniciais
├── Identity/
│   ├── ApplicationUser.cs   # User customizado
│   └── IdentityService.cs   # Serviço de Identity
├── Auth/
│   └── TokenService.cs      # Geração de JWT
└── DependencyInjection.cs
```

**Serviços Principais:**

```csharp
// Interceptadores
- AuditableEntityInterceptor      // Auditoria automática
- DispatchDomainEventsInterceptor // Despacho de eventos

// Serviços
- IdentityService                 // Gerenciamento de usuários
- TokenService                    // Geração de JWT
- ApplicationDbContextInitialiser // Inicialização de banco
```

---

### **Web Layer** (Camada de Apresentação)

Responsabilidade: **API REST e HTTP**

```
Web/
├── Endpoints/               # Endpoints da API
├── Services/               # Serviços (ex: CurrentUser)
├── ClientApp/              # SPA (opcional)
├── wwwroot/                # Arquivos estáticos
├── Program.cs              # Setup da aplicação
├── DependencyInjection.cs  # Injeção de dependência
└── appsettings.json        # Configurações
```

**Handlers Web:**
- **ProblemDetailsExceptionHandler**: Trata exceções e retorna ProblemDetails
- **IdentityApiOperationTransformer**: Customiza OpenAPI schema
- **ApiExceptionOperationTransformer**: Documenta exceções em OpenAPI

---

## 📋 Padrões e Convenções

### CQRS (Command Query Responsibility Segregation)

```csharp
// COMMAND (Escrita)
public class CreateUserCommand : IRequest<int>
{
    public string Email { get; set; }
    public string Password { get; set; }
}

// QUERY (Leitura)
public class GetUserByIdQuery : IRequest<UserVm>
{
    public int Id { get; set; }
}

// Handlers
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
{
    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Implementação...
    }
}

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserVm>
{
    public async Task<UserVm> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        // Implementação...
    }
}
```

### Validação com FluentValidation

```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido");
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8);
    }
}
```

### Mapeamento com AutoMapper

```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserVm>();
        CreateMap<CreateUserCommand, User>();
    }
}
```

### Entity Auditing

Interceptador automático rastreia:
- `Created`: Timestamp de criação
- `CreatedBy`: Usuário criador
- `LastModified`: Timestamp de modificação
- `LastModifiedBy`: Usuário modificador

---

## 💉 Injeção de Dependência

### Configuração Automática

As dependências são registradas em `DependencyInjection.cs` em cada camada:

#### Application Layer
```csharp
// src/Application/DependencyInjection.cs
public static void AddApplicationServices(this IHostApplicationBuilder builder)
{
    // AutoMapper
    builder.Services.AddAutoMapper(cfg => 
        cfg.AddMaps(Assembly.GetExecutingAssembly()));
    
    // Validadores FluentValidation
    builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    
    // MediatR
    builder.Services.AddMediatR(cfg => {
        cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        cfg.AddOpenRequestPreProcessor(typeof(LoggingBehaviour<>));
        cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
        cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
        cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
    });
}
```

#### Infrastructure Layer
```csharp
// src/Infrastructure/DependencyInjection.cs
public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
{
    // DbContext
    builder.Services.AddDbContext<ApplicationDbContext>(...);
    
    // Identity
    builder.Services
        .AddIdentityCore<ApplicationUser>()
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();
    
    // Serviços customizados
    builder.Services.AddTransient<IIdentityService, IdentityService>();
    builder.Services.AddTransient<ITokenService, TokenService>();
}
```

#### Web Layer
```csharp
// src/Web/DependencyInjection.cs
public static void AddWebServices(this IHostApplicationBuilder builder)
{
    builder.Services.AddScoped<IUser, CurrentUser>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
}
```

#### Uso no Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();
app.Run();
```

---

## 🧪 Testes

### Estrutura de Testes

```
tests/
├── Domain.UnitTests/              # Testes de Entidades e Value Objects
├── Application.UnitTests/         # Testes de Commands, Queries, Validators
├── Infrastructure.IntegrationTests/  # Testes de Data Access
├── Application.FunctionalTests/   # Testes E2E da API
└── TestAppHost/                   # Aspire para ambiente de testes
```

### Tipos de Testes

#### 1. **Unit Tests** (Domínio)
```csharp
// Domain.UnitTests
[TestFixture]
public class ValueObjectTests
{
    [Test]
    public void Should_Create_Valid_ValueObject()
    {
        // Arrange
        var value = "test";
        
        // Act
        var vo = new MyValueObject(value);
        
        // Assert
        Assert.That(vo.Value, Is.EqualTo(value));
    }
}
```

#### 2. **Unit Tests** (Aplicação)
```csharp
// Application.UnitTests
[TestFixture]
public class CreateUserCommandHandlerTests
{
    [Test]
    public async Task Should_Create_User_Successfully()
    {
        // Arrange
        var handler = new CreateUserCommandHandler(mockDbContext);
        var command = new CreateUserCommand { Email = "test@example.com" };
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.GreaterThan(0));
    }
}
```

#### 3. **Integration Tests** (Infraestrutura)
```csharp
// Infrastructure.IntegrationTests
[TestFixture]
public class DatabaseTests
{
    private ApplicationDbContext _context;
    
    [SetUp]
    public async Task Setup()
    {
        // Setup banco de dados em memória
        _context = CreateTestDbContext();
    }
    
    [Test]
    public async Task Should_Save_And_Retrieve_User()
    {
        // Arrange
        var user = new ApplicationUser { Email = "test@example.com" };
        
        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        // Assert
        var retrieved = await _context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        Assert.That(retrieved, Is.Not.Null);
    }
}
```

#### 4. **Functional Tests** (E2E)
```csharp
// Application.FunctionalTests
[TestFixture]
public class UserEndpointTests
{
    private HttpClient _client;
    
    [SetUp]
    public void Setup()
    {
        _client = CreateTestHttpClient();
    }
    
    [Test]
    public async Task Should_Create_User_Via_API()
    {
        // Arrange
        var request = new CreateUserCommand { Email = "test@example.com", Password = "password123" };
        
        // Act
        var response = await _client.PostAsJsonAsync("/users", request);
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
    }
}
```

### Executar Testes

```bash
# Executar todos os testes
dotnet test

# Executar testes específicos
dotnet test --filter "FullyQualifiedName~Domain.UnitTests"

# Com relatório de cobertura
dotnet test /p:CollectCoverage=true
```

---

## 🚀 Configuração e Deployment

### Desenvolvimento Local

#### Pré-requisitos
- .NET SDK 10.0.302
- Docker Desktop
- PostgreSQL (via Docker Compose)

#### Setup

```bash
# 1. Clone o repositório
git clone <repo>
cd IdentityService

# 2. Restaure pacotes
dotnet restore

# 3. Inicie o banco de dados
docker-compose up -d postgres

# 4. Aplique migrações
dotnet ef database update --project src/Infrastructure --startup-project src/Web

# 5. Execute a aplicação
dotnet run --project src/AppHost
```

O dashboard do Aspire abrirá automaticamente em `http://localhost:15000`

### Docker Compose (Desenvolvimento)

```yaml
version: '3.8'

services:
  identity-api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5001:8080"
    depends_on:
      - postgres
    environment:
      - ConnectionStrings__IdentityServiceDb=Host=postgres;Port=5432;Database=identityservice;Username=identity;Password=supersecret
      - ASPNETCORE_ENVIRONMENT=Development
      - Jwt__Secret=my_super_secret_jwt_key_that_is_long_enough_32bytes!
      - Jwt__Issuer=IdentityService
      - Jwt__Audience=IdentityService.Clients

  postgres:
    image: postgres:16-alpine
    ports:
      - "5432:5432"
    environment:
      - POSTGRES_DB=identityservice
      - POSTGRES_USER=identity
      - POSTGRES_PASSWORD=supersecret
    volumes:
      - pgdata:/var/lib/postgresql/data

volumes:
  pgdata:
```

#### Executar com Docker Compose

```bash
# Build e inicie os containers
docker-compose up -d

# Logs da aplicação
docker-compose logs -f identity-api

# Parar containers
docker-compose down

# Remover volumes (banco de dados)
docker-compose down -v
```

### Produção (Azure)

#### Arquitetura

```
┌──────────────────────────┐
│   Azure Container Apps   │
│  (Web API Service)       │
└──────────┬───────────────┘
           │
    ┌──────▼────────┐
    │ Azure Database│
    │  PostgreSQL   │
    └───────────────┘
           ▲
           │
┌──────────┴─────────────┐
│  Azure Key Vault       │
│ (Secrets & Configs)    │
└────────────────────────┘
```

#### Deployment com Azure Container Apps

```bash
# Login no Azure
az login

# Deploy
az containerapp up \
  --name identity-service \
  --resource-group myResourceGroup \
  --image myregistry.azurecr.io/identity-service:latest \
  --environment myEnvironment \
  --ingress external \
  --target-port 8080
```

#### Variáveis de Ambiente (Produção)

```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__IdentityServiceDb=Server=...
Jwt__Secret=<use-azure-keyvault>
Jwt__Issuer=IdentityService
Jwt__Audience=IdentityService.Clients
Jwt__AccessTokenExpirationMinutes=15
Jwt__RefreshTokenExpirationDays=7
```

### Migrações de Banco de Dados

```bash
# Criar nova migration
dotnet ef migrations add "MigrationName" \
  --project src/Infrastructure \
  --startup-project src/Web

# Aplicar migrações
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/Web

# Remover última migration
dotnet ef migrations remove \
  --project src/Infrastructure \
  --startup-project src/Web
```

---

## 📊 Monitoramento e Logs

### .NET Aspire Dashboard

Acesso: `http://localhost:15000`

**Monitora:**
- Logs em tempo real
- Traces distribuídos
- Métricas de performance
- Health checks

### Estruturado de Logs

```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;
    
    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }
    
    public void DoSomething()
    {
        _logger.LogInformation("Operação iniciada: {@Details}", new { Timestamp = DateTime.Now });
    }
}
```

---

## 🔧 Scaffolding de Código

### Criar novo Use Case (Command)

```bash
cd src/Application

dotnet new ca-usecase \
  --name CreateTodoItem \
  --feature-name TodoItems \
  --usecase-type command \
  --return-type Guid
```

Gera:
- `CreateTodoItemCommand.cs`
- `CreateTodoItemCommandHandler.cs`
- `CreateTodoItemCommandValidator.cs`

### Criar novo Use Case (Query)

```bash
dotnet new ca-usecase \
  --name GetTodoItems \
  --feature-name TodoItems \
  --usecase-type query \
  --return-type TodoItemsVm
```

---

## 📚 Referências e Recursos

- **Clean Architecture**: https://cleanarchitecture.jasontaylor.dev
- **MediatR**: https://github.com/jbogard/MediatR
- **Entity Framework Core**: https://learn.microsoft.com/en-us/ef/core/
- **FluentValidation**: https://fluentvalidation.net/
- **AutoMapper**: https://automapper.org/
- **.NET Aspire**: https://learn.microsoft.com/en-us/dotnet/aspire/

---

## 📞 Suporte e Contribuição

Para mais informações, consulte o [README.md](./README.md) e a documentação oficial do template.
