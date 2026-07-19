# Boilerplate & Template - IdentityService

## 📌 O Que é Boilerplate?

**Boilerplate** é código pré-configurado e reutilizável que vem com o template. O IdentityService usa o **Clean Architecture Solution Template v10.8.0**, que fornece uma estrutura pronta para desenvolvimento, seguindo as melhores práticas.

---

## 🎁 O Que Vem Pronto (Boilerplate)

### ✅ Já Implementado no Template

| Componente | Status | Localização |
|-----------|--------|------------|
| **Estrutura em camadas** | ✅ Pronto | `src/` |
| **Injeção de Dependência** | ✅ Pronto | `DependencyInjection.cs` |
| **MediatR Setup** | ✅ Pronto | `Application/DependencyInjection.cs` |
| **Entity Framework Core** | ✅ Pronto | `Infrastructure/Data/` |
| **PostgreSQL Connection** | ✅ Pronto | `appsettings.json` |
| **Identity (Usuários/Roles)** | ✅ Pronto | `Infrastructure/Identity/` |
| **JWT Authentication** | ✅ Pronto | `Infrastructure/Auth/TokenService.cs` |
| **FluentValidation** | ✅ Pronto | `Application/Common/Behaviours/ValidationBehaviour.cs` |
| **AutoMapper** | ✅ Pronto | `Application/Common/` |
| **Exception Handling** | ✅ Pronto | `Web/ProblemDetailsExceptionHandler.cs` |
| **OpenAPI/Swagger** | ✅ Pronto | `Web/Program.cs` |
| **Scalar (API Docs)** | ✅ Pronto | Mapeado em `/scalar` |
| **Health Checks** | ✅ Pronto | `.AddServiceDefaults()` |
| **Aspire Orchestration** | ✅ Pronto | `AppHost/Program.cs` |
| **Docker Support** | ✅ Pronto | `Dockerfile` + `docker-compose.yml` |
| **Logging Estruturado** | ✅ Pronto | MediatR Behaviours |
| **Auditoria de Entidades** | ✅ Pronto | `Infrastructure/Data/Interceptors/AuditableEntityInterceptor.cs` |
| **Domain Events** | ✅ Pronto | `Infrastructure/Data/Interceptors/DispatchDomainEventsInterceptor.cs` |
| **Testes** | ✅ Pronto | Estrutura em `tests/` |
| **CORS** | ✅ Pronto | Configurado no `Program.cs` |
| **HTTPS** | ✅ Pronto | Redirecionamento configurado |
| **Database Seeding** | ✅ Pronto | `ApplicationDbContextInitialiser.cs` |

---

## 🏗️ Estrutura do Boilerplate

### Domain Layer (src/Domain)

Responsável por lógica de negócio pura.

```
Domain/
├── Common/
│   ├── BaseEntity.cs           # Base para todas as entidades
│   │   ├── Id: int
│   │   ├── Created: DateTime
│   │   ├── CreatedBy: string
│   │   ├── LastModified: DateTime
│   │   ├── LastModifiedBy: string
│   │   └── DomainEvents: List<BaseDomainEvent>
│   └── ...
├── Constants/                  # Constantes do domínio
├── Entities/                   # Suas entidades aqui
├── Enums/                      # Enumerações
├── Events/                     # Eventos de domínio
├── Exceptions/                 # Exceções customizadas
├── ValueObjects/               # Objetos de valor
└── GlobalUsings.cs            # Imports globais
```

**Exemplo de Entidade Herdando BaseEntity:**

```csharp
public class TodoItem : BaseEntity
{
    public string? Title { get; set; }
    public string? Note { get; set; }
    public int ListId { get; set; }
    public bool Done { get; set; }
    public int Priority { get; set; }
}
```

### Application Layer (src/Application)

Use cases e orquestração.

```
Application/
├── Auth/                       # Use cases de autenticação (pronto)
├── Common/
│   ├── Behaviours/            # MediatR Pipeline Behaviours
│   │   ├── AuthorizationBehaviour.cs      # Valida autorização
│   │   ├── LoggingBehaviour.cs            # Registra requests
│   │   ├── PerformanceBehaviour.cs        # Monitora performance
│   │   ├── UnhandledExceptionBehaviour.cs # Trata exceções
│   │   └── ValidationBehaviour.cs         # Valida com FluentValidation
│   ├── Exceptions/
│   │   ├── ValidationException.cs
│   │   └── ...
│   ├── Interfaces/            # Contratos
│   │   ├── IApplicationDbContext.cs
│   │   ├── IUser.cs            # Usuário atual
│   │   └── ...
│   ├── Mappings/              # AutoMapper Profiles
│   └── ...
├── WeatherForecasts/          # Exemplo: deletar depois
├── DependencyInjection.cs     # Configuração de serviços
├── GlobalUsings.cs
└── Application.csproj
```

**Behaviours (Pipeline MediatR):**

```csharp
// Ordem de execução:
1. LoggingBehaviour<>          // Log entrada
2. AuthorizationBehaviour<>    // Verifica [Authorize]
3. ValidationBehaviour<>       // Executa validadores
4. PerformanceBehaviour<>      // Monitora tempo
5. UnhandledExceptionBehaviour // Trata exceções
6. ➜ Handler                   // Executa handler
7. ⬅ Response
```

### Infrastructure Layer (src/Infrastructure)

Implementações técnicas.

```
Infrastructure/
├── Data/
│   ├── ApplicationDbContext.cs         # DbContext (adicione DbSets aqui)
│   ├── ApplicationDbContextInitialiser.cs # Seed & Migrations
│   ├── Configurations/                 # Entity Configurations (EF)
│   ├── Interceptors/
│   │   ├── AuditableEntityInterceptor.cs      # Auditoria automática
│   │   └── DispatchDomainEventsInterceptor.cs # Dispatch de eventos
│   └── Migrations/                    # EF Core Migrations
├── Auth/
│   └── TokenService.cs               # Geração de JWT (pronto)
├── Identity/
│   ├── ApplicationUser.cs            # User customizado (pode estender)
│   └── IdentityService.cs            # Serviço de Identity
├── DependencyInjection.cs            # Registro de serviços
├── GlobalUsings.cs
└── Infrastructure.csproj
```

**DbContext Padrão:**

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
    
    // Adicione seus DbSets aqui
    public DbSet<TodoList> TodoLists => Set<TodoList>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Suas configurations aqui
        modelBuilder.ApplyConfigurationsFromAssembly(
            Assembly.GetExecutingAssembly());
    }
}
```

### Web Layer (src/Web)

API REST.

```
Web/
├── Program.cs                         # Setup completo (pronto)
├── DependencyInjection.cs            # Configurações Web
├── appsettings.json                  # Config local
├── appsettings.Development.json      # Config dev
├── Endpoints/                        # Sua API aqui
│   ├── Health.cs                     # Health endpoints (pronto)
│   └── YourFeature.cs                # Seus endpoints
├── Services/
│   ├── CurrentUser.cs                # Usuário atual (pronto)
│   ├── ProblemDetailsExceptionHandler.cs # Exception handler (pronto)
│   └── ...
├── Infrastructure/
├── ClientApp/                        # SPA (opcional)
├── wwwroot/                          # Arquivos estáticos
└── Web.csproj
```

**Program.cs Pronto para Uso:**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Builders de terceiros já registrados
builder.AddServiceDefaults();                   // ✅
builder.AddApplicationServices();               // ✅
builder.AddInfrastructureServices();            // ✅
builder.AddWebServices();                       // ✅

var app = builder.Build();

// Pipeline configurado
app.UseHttpsRedirection();
app.UseCors(...);
app.MapOpenApi();
app.MapScalarApiReference();
app.UseExceptionHandler(...);
app.MapEndpoints(typeof(Program).Assembly);    // ✅ Auto-discovery

app.Run();
```

### AppHost (src/AppHost)

Orquestração de Serviços (Aspire).

```
AppHost/
├── Program.cs                    # Orquestração Aspire
├── appsettings.json
├── Properties/
└── AppHost.csproj
```

**Aspire Setup Pronto:**

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Azure Container App Environment
builder.AddAzureContainerAppEnvironment("aca-env");

// PostgreSQL
var databaseServer = builder
    .AddAzurePostgresFlexibleServer("DatabaseServer")
    .WithPasswordAuthentication()
    .AddDatabase("Database");

// Web API
var web = builder.AddProject<Projects.Web>("WebAPI")
    .WithReference(databaseServer)
    .WaitFor(databaseServer)
    .WithExternalHttpEndpoints();

builder.Build().Run();
```

---

## 🔧 Boilerplate de Autenticação

### Fluxo de Login/Registro (Pronto)

```csharp
// Infrastructure/Identity/IdentityService.cs
public class IdentityService : IIdentityService
{
    public async Task<(Result Result, string UserId)> CreateUserAsync(
        string email, 
        string password)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email
        };
        
        var result = await _userManager.CreateAsync(user, password);
        return (result.ToApplicationResult(), user.Id);
    }
    
    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is null
            ? Result.Failure(new[] { "Usuário não encontrado" })
            : (await _userManager.DeleteAsync(user)).ToApplicationResult();
    }
}
```

### JWT Token Service (Pronto)

```csharp
// Infrastructure/Auth/TokenService.cs
public class TokenService : ITokenService
{
    public string GenerateAccessToken(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email)
        };
        
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpiration),
            signingCredentials: credentials);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

---

## 🧪 Boilerplate de Testes

### Estrutura Pronta

```
tests/
├── Domain.UnitTests/
│   ├── Domain.UnitTests.csproj
│   ├── GlobalUsings.cs
│   └── ValueObjects/              # Testes de Value Objects
│
├── Application.UnitTests/
│   ├── Application.UnitTests.csproj
│   ├── GlobalUsings.cs
│   └── Common/                    # Testes de comportamentos
│
├── Infrastructure.IntegrationTests/
│   ├── Infrastructure.IntegrationTests.csproj
│   ├── GlobalUsings.cs
│   └── (Testes de Data Access)
│
├── Application.FunctionalTests/
│   ├── Application.FunctionalTests.csproj
│   ├── FunctionalTestSetup.cs    # Setup comum
│   ├── GlobalUsings.cs
│   └── Endpoints/                # Testes E2E
│
└── TestAppHost/
    ├── TestAppHost.csproj        # Aspire para testes
    └── Program.cs
```

### Exemplo de Teste Unitário (Pronto para Copiar)

```csharp
[TestFixture]
public class ValueObjectTests
{
    [Test]
    public void Should_Create_Valid_ValueObject()
    {
        // Arrange
        var value = "test value";
        
        // Act
        var vo = new MyValueObject(value);
        
        // Assert
        Assert.That(vo.Value, Is.EqualTo(value));
    }
    
    [Test]
    public void Should_Throw_When_Invalid()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => new MyValueObject(""));
    }
}
```

---

## 🚀 Boilerplate de Deployment

### Dockerfile (Pronto)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Install Node.js for SPA build
RUN apt-get update && apt-get install -y curl \
    && curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y nodejs \
    && rm -rf /var/lib/apt/lists/*

# Copy and build
COPY . ./
RUN dotnet restore
RUN dotnet publish src/Web/Web.csproj -c Release -o out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 8080
ENTRYPOINT ["dotnet", "IdentityService.Web.dll"]
```

### Docker Compose (Pronto)

```yaml
version: '3.8'

services:
  identity-api:
    build: .
    ports:
      - "5001:8080"
    depends_on:
      - postgres
    environment:
      - ConnectionStrings__IdentityServiceDb=Host=postgres;...
      - ASPNETCORE_ENVIRONMENT=Development
    
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

---

## 🎯 O Que Você Precisa Customizar

### ✏️ Adicione Suas Entidades

**Arquivo**: `src/Domain/Entities/YourEntity.cs`

```csharp
public class Invoice : BaseEntity  // Herda de BaseEntity (boilerplate)
{
    // ✅ IMPORTANTE: BaseEntity já fornece Id como Guid
    // public Guid Id { get; set; } // Herdado - NUNCA mude para int!
    // public DateTime Created { get; set; }
    // public string CreatedBy { get; set; }
    // public DateTime? LastModified { get; set; }
    // public string? LastModifiedBy { get; set; }
    
    // Suas propriedades aqui
    public string InvoiceNumber { get; set; }
    public decimal Amount { get; set; }
    
    // ✅ Foreign Keys também devem ser Guid
    public Guid UserId { get; set; }
}
```

### ✏️ Crie Seus Use Cases

**Arquivo**: `src/Application/Invoices/Commands/CreateInvoice/CreateInvoiceCommand.cs`

```csharp
// Command & Handler vazio pronto para implementação
public class CreateInvoiceCommand : IRequest<Guid> { } // ✅ Retorna Guid, não int

public class CreateInvoiceCommandHandler 
    : IRequestHandler<CreateInvoiceCommand, Guid>
{
    public async Task<Guid> Handle(CreateInvoiceCommand request, ...)
    {
        // Sua implementação aqui
    }
}
```

### ✏️ Mapeie no AutoMapper

**Arquivo**: `src/Application/Common/Mappings/MappingProfile.cs`

```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Adicione seus mappings
        CreateMap<Invoice, InvoiceDto>();
        CreateMap<CreateInvoiceCommand, Invoice>();
    }
}
```

### ✏️ Configure DbSet

**Arquivo**: `src/Infrastructure/Data/ApplicationDbContext.cs`

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    // Adicione aqui
    public DbSet<Invoice> Invoices => Set<Invoice>();
}
```

### ✏️ Crie Endpoints

**Arquivo**: `src/Web/Endpoints/Invoices/InvoiceEndpoints.cs`

```csharp
public class InvoiceEndpoints
{
    public static void MapInvoiceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/invoices")
            .RequireAuthorization();
        
        group.MapGet("/", GetInvoices);
        group.MapPost("/", CreateInvoice);
    }
}
```

### ✏️ Registre Endpoints

**Arquivo**: `src/Web/Program.cs`

```csharp
// Adicione esta linha (auto-discovery já está configurado)
app.MapInvoiceEndpoints();
```

---

## 📋 Checklist: Do Zero à Produção

### 1. Setup (5 minutos)
- [ ] Clone o repositório
- [ ] `dotnet restore`
- [ ] `docker-compose up -d postgres`
- [ ] `dotnet ef database update`

### 2. Implementar Feature (30 minutos)
- [ ] Criar Entidade em `Domain/Entities/`
- [ ] Criar DTOs em `Application/YourFeature/DTOs/`
- [ ] Criar Commands/Queries em `Application/YourFeature/`
- [ ] Criar Handlers
- [ ] Criar Validators
- [ ] Criar EF Configuration em `Infrastructure/Data/Configurations/`
- [ ] Adicionar DbSet ao DbContext
- [ ] Criar Migration
- [ ] Criar Endpoints em `Web/Endpoints/`
- [ ] Registrar Endpoints

### 3. Testar (10 minutos)
- [ ] `dotnet build`
- [ ] `dotnet test`
- [ ] Testar endpoints manualmente

### 4. Deploy (5 minutos)
- [ ] `docker build -t identity-service:latest .`
- [ ] Push para registry
- [ ] Deploy em Azure

---

## 📚 Exemplos de Boilerplate Pronto para Copiar

### 1. Novo Command & Handler

```csharp
// Command
public class MyCommand : IRequest<int>
{
    public string Name { get; set; } = null!;
}

// Validator
public class MyCommandValidator : AbstractValidator<MyCommand>
{
    public MyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

// Handler
public class MyCommandHandler : IRequestHandler<MyCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    
    public MyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Guid> Handle(MyCommand request, CancellationToken cancellationToken)
    {
        var entity = new MyEntity { Name = request.Name };
        _context.MyEntities.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id; // ✅ Agora retorna Guid (herdado de BaseEntity)
    }
}
```

### 2. Novo Endpoint

```csharp
public class MyEndpoints
{
    public static void MapMyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/my")
            .WithName("My")
            .WithOpenApi()
            .RequireAuthorization();
        
        group.MapGet("/{id}", GetById)
            .WithName("GetById")
            .WithOpenApi();
        
        group.MapPost("/", Create)
            .WithName("Create")
            .WithOpenApi();
    }
    
    private static async Task<Ok<MyDto>> GetById(
        int id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetByIdQuery { Id = id },
            cancellationToken);
        
        return TypedResults.Ok(result);
    }
    
    private static async Task<Created<Guid>> Create(
        ISender sender,
        MyCommand command,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return TypedResults.Created($"/my/{id}", id); // ✅ Agora retorna Guid
    }
}
```

---

## 🔗 Relação: Boilerplate ↔ Suas Customizações

```
┌─────────────────────────────────────────────────────┐
│ Template Boilerplate (Clean.Architecture)           │
│ ├─ Estrutura em camadas                            │
│ ├─ MediatR Pipeline                                │
│ ├─ Entity Framework Core                           │
│ ├─ JWT Authentication                              │
│ ├─ Exception Handling                              │
│ ├─ Logging & Monitoring                            │
│ └─ Docker & Aspire                                 │
└─────────────────────────────────────────────────────┘
                        ↑
        ┌───────────────┼───────────────┐
        │               │               │
        ↓               ↓               ↓
   Your Domain    Your Business   Your Endpoints
   Entities       Logic (Use       (API Routes)
                  Cases)
```

---

## ✅ Conclusão

O **boilerplate** fornece:
1. ✅ Estrutura arquitetural pronta
2. ✅ Injeção de dependência configurada
3. ✅ Pipeline de validação/autorização
4. ✅ Auditoria automática
5. ✅ Tratamento de exceções
6. ✅ Autenticação JWT
7. ✅ Testes estruturados
8. ✅ Docker & Aspire prontos

Você adiciona:
- 📝 Suas entidades de negócio
- 💼 Suas regras de negócio (Use Cases)
- 🔌 Seus endpoints API
- 🧪 Seus testes específicos

Comece deletando `WeatherForecasts/` e implementando suas features!

