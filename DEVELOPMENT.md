# Guia Prático de Desenvolvimento - IdentityService

## 📋 Índice
1. [Quick Start](#quick-start)
2. [Estrutura de Pastas](#estrutura-de-pastas)
3. [Como Implementar uma Feature](#como-implementar-uma-feature)
4. [Padrões de Codificação](#padrões-de-codificação)
5. [Boas Práticas](#boas-práticas)
6. [Troubleshooting](#troubleshooting)

---

## ⚡ Quick Start

### 1. Clonar e Configurar

```bash
# Clone
git clone <repo>
cd IdentityService

# Restaure pacotes
dotnet restore

# Inicie os serviços (PostgreSQL)
docker-compose up -d
```

### 2. Executar a Aplicação

```bash
# Via Aspire
dotnet run --project src/AppHost

# Dashboard: http://localhost:15000
# API: http://localhost:5000
```

### 3. Testar

```bash
# Todos os testes
dotnet test

# Testes específicos
dotnet test --filter "FullyQualifiedName~TodoItems"
```

### 4. Build

```bash
dotnet build

# Release
dotnet build -c Release
```

---

## 📂 Estrutura de Pastas

### Padrão para uma Nova Feature

Se você está criando uma feature "Invoices", siga este padrão:

```
src/Application/
├── Invoices/                          # ✨ Sua Feature
│   ├── Commands/                      # Escrita
│   │   ├── CreateInvoice/
│   │   │   ├── CreateInvoiceCommand.cs
│   │   │   ├── CreateInvoiceCommandHandler.cs
│   │   │   └── CreateInvoiceCommandValidator.cs
│   │   ├── UpdateInvoice/
│   │   └── DeleteInvoice/
│   ├── Queries/                       # Leitura
│   │   ├── GetInvoices/
│   │   │   ├── GetInvoicesQuery.cs
│   │   │   └── GetInvoicesQueryHandler.cs
│   │   └── GetInvoiceById/
│   ├── EventHandlers/                 # Manipuladores de eventos
│   │   └── InvoiceCreatedEventHandler.cs
│   ├── Validators/                    # Validadores (FluentValidation)
│   │   └── InvoiceValidator.cs
│   └── DTOs/                          # Data Transfer Objects
│       ├── InvoiceDto.cs
│       └── CreateInvoiceDto.cs

src/Domain/
├── Entities/
│   └── Invoice.cs                     # Entidade de domínio
├── ValueObjects/
│   └── InvoiceNumber.cs               # Objeto de valor
├── Events/
│   └── InvoiceCreatedEvent.cs         # Evento de domínio
└── Exceptions/
    └── InvoiceNotFoundException.cs     # Exceção de negócio

src/Infrastructure/
├── Data/
│   ├── Configurations/
│   │   └── InvoiceConfiguration.cs    # EF Core configuration
│   └── Migrations/
│       └── AddInvoiceEntity.cs

src/Web/
├── Endpoints/
│   └── Invoices/
│       └── InvoiceEndpoints.cs        # API Endpoints
```

---

## 🚀 Como Implementar uma Feature

### Passo 1: Criar a Entidade de Domínio

**Arquivo**: `src/Domain/Entities/Invoice.cs`

```csharp
using IdentityService.Domain.Common;

namespace IdentityService.Domain.Entities;

public class Invoice : BaseEntity
{
    // ✅ IMPORTANTE: BaseEntity já fornece Id como Guid
    // public Guid Id { get; set; } // Herdado - NUNCA mude para int!
    
    public required string InvoiceNumber { get; set; }
    public required string ClientName { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public string? Notes { get; set; }
    
    // Relacionamentos - Use Guid para ForeignKeys também
    public Guid? UserId { get; set; } // ✅ Guid, não int
    
    // Métodos de domínio
    public void MarkAsPaid()
    {
        if (Status != InvoiceStatus.Pending)
            throw new InvalidOperationException("Apenas invoices pendentes podem ser marcadas como pagas");
        
        Status = InvoiceStatus.Paid;
        AddDomainEvent(new InvoicePaidEvent(Id));
    }
    
    public void Cancel()
    {
        if (Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Não é possível cancelar invoices já pagas");
        
        Status = InvoiceStatus.Cancelled;
        AddDomainEvent(new InvoiceCancelledEvent(Id));
    }
}

public enum InvoiceStatus
{
    Pending = 0,
    Paid = 1,
    Cancelled = 2,
    Overdue = 3
}
```

### Passo 2: Criar DTOs

**Arquivo**: `src/Application/Invoices/DTOs/CreateInvoiceDto.cs`

```csharp
namespace IdentityService.Application.Invoices.DTOs;

public class CreateInvoiceDto
{
    public required string InvoiceNumber { get; set; }
    public required string ClientName { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }
}

public class InvoiceDto : CreateInvoiceDto
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public string Status { get; set; } = null!;
}
```

### Passo 3: Criar Command & Handler

**Arquivo**: `src/Application/Invoices/Commands/CreateInvoice/CreateInvoiceCommand.cs`

```csharp
using MediatR;

namespace IdentityService.Application.Invoices.Commands.CreateInvoice;

public class CreateInvoiceCommand : IRequest<Guid>
{
    public required string InvoiceNumber { get; set; }
    public required string ClientName { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }
}
```

**Arquivo**: `src/Application/Invoices/Commands/CreateInvoice/CreateInvoiceCommandHandler.cs`

```csharp
using IdentityService.Application.Common.Interfaces;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Application.Invoices.Commands.CreateInvoice;

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public CreateInvoiceCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    
    public async Task<Guid> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = new Invoice
        {
            InvoiceNumber = request.InvoiceNumber,
            ClientName = request.ClientName,
            Amount = request.Amount,
            DueDate = request.DueDate,
            Notes = request.Notes,
            Status = InvoiceStatus.Pending,
            UserId = Guid.Parse(_user.Id!) // ✅ Guid.Parse, não int.Parse
        };
        
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);
        
        return invoice.Id;
    }
}
```

### Passo 4: Criar Validator

**Arquivo**: `src/Application/Invoices/Commands/CreateInvoice/CreateInvoiceCommandValidator.cs`

```csharp
using FluentValidation;

namespace IdentityService.Application.Invoices.Commands.CreateInvoice;

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceNumber)
            .NotEmpty().WithMessage("Número da invoice é obrigatório")
            .MaximumLength(50).WithMessage("Número não pode exceder 50 caracteres");
        
        RuleFor(x => x.ClientName)
            .NotEmpty().WithMessage("Nome do cliente é obrigatório")
            .MaximumLength(200).WithMessage("Nome não pode exceder 200 caracteres");
        
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Valor deve ser maior que 0");
        
        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("Data de vencimento deve ser no futuro");
    }
}
```

### Passo 5: Criar Query & Handler

**Arquivo**: `src/Application/Invoices/Queries/GetInvoices/GetInvoicesQuery.cs`

```csharp
using IdentityService.Application.Invoices.DTOs;
using MediatR;

namespace IdentityService.Application.Invoices.Queries.GetInvoices;

public class GetInvoicesQuery : IRequest<List<InvoiceDto>>
{
    public InvoiceStatus? Status { get; set; }
}
```

**Arquivo**: `src/Application/Invoices/Queries/GetInvoices/GetInvoicesQueryHandler.cs`

```csharp
using AutoMapper;
using AutoMapper.QueryableExtensions;
using IdentityService.Application.Common.Interfaces;
using IdentityService.Application.Invoices.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Application.Invoices.Queries.GetInvoices;

public class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, List<InvoiceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    
    public GetInvoicesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<List<InvoiceDto>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Invoices.AsQueryable();
        
        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status);
        
        return await query
            .ProjectTo<InvoiceDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
```

### Passo 6: Criar AutoMapper Profile

**Arquivo**: `src/Application/Invoices/Mappings/InvoiceMappingProfile.cs`

```csharp
using AutoMapper;
using IdentityService.Application.Invoices.Commands.CreateInvoice;
using IdentityService.Application.Invoices.DTOs;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Invoices.Mappings;

public class InvoiceMappingProfile : Profile
{
    public InvoiceMappingProfile()
    {
        CreateMap<Invoice, InvoiceDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        
        CreateMap<CreateInvoiceCommand, Invoice>();
    }
}
```

### Passo 7: Configurar EF Core

**Arquivo**: `src/Infrastructure/Data/Configurations/InvoiceConfiguration.cs`

```csharp
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        // ✅ BaseEntity já configura HasKey(x => x.Id) como Guid
        builder.HasKey(x => x.Id);
        
        // ✅ Configurar a geração automática de Guid no banco
        builder.Property(x => x.Id)
            .HasDefaultValueSql("gen_random_uuid()"); // PostgreSQL
        
        builder.Property(x => x.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(x => x.ClientName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);
        
        builder.Property(x => x.Status)
            .HasConversion<int>();
        
        // ✅ Chaves estrangeiras também devem ser Guid
        builder.Property(x => x.UserId)
            .HasColumnType("uuid");
        
        builder.HasIndex(x => x.InvoiceNumber)
            .IsUnique();
    }
}
```

**Nota sobre PostgreSQL:**
- Use `gen_random_uuid()` para gerar Guids automaticamente no banco
- Use `uuid` como tipo de coluna
- Para SQL Server: use `NEWID()` ou `NEWSEQUENTIALID()`
```

### Passo 8: Adicionar DbSet ao DbContext

**Arquivo**: `src/Infrastructure/Data/ApplicationDbContext.cs`

```csharp
public DbSet<Invoice> Invoices => Set<Invoice>();
```

### Passo 9: Criar Migration

```bash
cd src

dotnet ef migrations add "AddInvoiceEntity" \
  --project Infrastructure \
  --startup-project ../src/Web
```

### Passo 10: Criar Endpoints

**Arquivo**: `src/Web/Endpoints/Invoices/InvoiceEndpoints.cs`

```csharp
using IdentityService.Application.Invoices.Commands.CreateInvoice;
using IdentityService.Application.Invoices.Queries.GetInvoices;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IdentityService.Web.Endpoints.Invoices;

public class InvoiceEndpoints
{
    public const string Tag = "Invoices";
    
    public static void MapInvoiceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/invoices")
            .WithName("Invoices")
            .WithOpenApi()
            .WithTag(Tag)
            .RequireAuthorization();
        
        group.MapGet("/", GetInvoices)
            .WithName("ListInvoices")
            .WithDescription("Listar todas as invoices");
        
        group.MapPost("/", CreateInvoice)
            .WithName("CreateInvoice")
            .WithDescription("Criar nova invoice");
    }
    
    private static async Task<Ok<List<InvoiceDto>>> GetInvoices(
        ISender sender,
        InvoiceStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetInvoicesQuery { Status = status }, 
            cancellationToken);
        
        return TypedResults.Ok(result);
    }
    
    private static async Task<Created<Guid>> CreateInvoice(
        ISender sender,
        CreateInvoiceCommand command,
        CancellationToken cancellationToken = default)
    {
        var id = await sender.Send(command, cancellationToken);
        
        return TypedResults.Created($"/invoices/{id}", id); // ✅ Agora Guid
    }
}
```

### Passo 11: Registrar Endpoints no Program.cs

**Arquivo**: `src/Web/Program.cs`

```csharp
// Adicionar esta linha junto com os outros MapEndpoints
app.MapInvoiceEndpoints();
```

---

## 💡 Padrões de Codificação

### 1. Nomenclatura

| Conceito | Padrão | Exemplo |
|----------|--------|---------|
| Classes | PascalCase | `CreateInvoiceCommand` |
| Métodos | PascalCase | `Handle`, `GetInvoices` |
| Propriedades | PascalCase | `InvoiceNumber` |
| Campos privados | _camelCase | `_context` |
| Variáveis locais | camelCase | `invoice` |
| Constantes | PascalCase | `MaxInvoiceAmount` |
| Namespaces | Hierarquia com pontos | `IdentityService.Application.Invoices.Commands.CreateInvoice` |

### 2. Ordem de Membros em Classes

```csharp
public class MyEntity
{
    // 1. Constantes
    public const string MyConstant = "value";
    
    // 2. Propriedades estáticas
    public static string StaticProperty { get; set; } = "";
    
    // 3. Propriedades de instância
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    
    // 4. Relacionamentos (nav properties)
    public ICollection<Child> Children { get; set; } = [];
    
    // 5. Construtor
    public MyEntity() { }
    
    public MyEntity(string name)
    {
        Name = name;
    }
    
    // 6. Métodos públicos
    public void PublicMethod()
    {
    }
    
    // 7. Métodos privados
    private void PrivateMethod()
    {
    }
}
```

### 3. Tratamento de Erros

```csharp
// ❌ Evitar
catch (Exception ex)
{
    // Swallow exception
}

// ✅ Correto
public class InvoiceNotFoundException : Exception
{
    public InvoiceNotFoundException(int invoiceId)
        : base($"Invoice com ID {invoiceId} não encontrada")
    {
    }
}

// No handler
var invoice = await _context.Invoices.FirstOrDefaultAsync(x => x.Id == id);
if (invoice is null)
    throw new InvoiceNotFoundException(id);
```

### 4. Async/Await

```csharp
// ❌ Evitar
var result = GetDataAsync().Result;

// ✅ Correto
var result = await GetDataAsync();

// ❌ Evitar
Task.Run(() => DoSomething());

// ✅ Correto
await DoSomethingAsync();
```

### 5. Validação de Entrada

```csharp
// ❌ Evitar validações manuais
if (string.IsNullOrEmpty(name))
    throw new ArgumentNullException(nameof(name));

// ✅ Usar Guard clauses (se implementado no projeto)
Guard.Against.NullOrEmpty(name, nameof(name));

// ✅ Usar FluentValidation
public class CommandValidator : AbstractValidator<MyCommand>
{
    public CommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
```

---

## ✅ Boas Práticas

### 1. SOLID Principles

#### Single Responsibility Principle
```csharp
// ❌ Múltiplas responsabilidades
public class UserService
{
    public void CreateUser(string email)
    {
        // Validar email
        // Salvar no banco
        // Enviar email
        // Registrar log
    }
}

// ✅ Uma responsabilidade por classe
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
{
    // Validação: FluentValidation
    // Persistência: DbContext
    // Email: Injected IEmailService
    // Logs: ILogger injected
}
```

#### Open/Closed Principle
```csharp
// Usar interfaces para extensibilidade
public interface IPaymentProcessor
{
    Task<bool> ProcessAsync(decimal amount);
}

public class CreditCardProcessor : IPaymentProcessor { }
public class PayPalProcessor : IPaymentProcessor { }
```

#### Dependency Inversion Principle
```csharp
// ❌ Dependência concreta
public class InvoiceService
{
    private SqlServer _db = new(); // Acoplamento
}

// ✅ Dependência abstraída
public class InvoiceService
{
    private readonly IApplicationDbContext _context; // Interface
    
    public InvoiceService(IApplicationDbContext context)
    {
        _context = context;
    }
}
```

### 2. DDD - Domain Driven Design

```csharp
// ✅ Value Object (imutável, sem identidade)
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    public Money(decimal amount, string currency)
    {
        if (amount < 0) throw new ArgumentException("Deve ser positivo");
        Amount = amount;
        Currency = currency;
    }
}

// ✅ Aggregate Root (tem identidade)
public class Order : BaseEntity
{
    public Money Total { get; private set; }
    public List<OrderItem> Items { get; private set; } = [];
    
    public void AddItem(OrderItem item)
    {
        Items.Add(item);
        RecalculateTotal();
        AddDomainEvent(new ItemAddedEvent(Id, item));
    }
}

// ✅ Domain Event
public class ItemAddedEvent : BaseDomainEvent
{
    public int OrderId { get; set; }
    public OrderItem Item { get; set; }
}
```

### 3. Testabilidade

```csharp
// ✅ Injetar dependências
public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Guid>
{
    public CreateInvoiceCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        ILogger<CreateInvoiceCommandHandler> logger)
    {
        _context = context;
        _identityService = identityService;
        _logger = logger;
    }
}

// ✅ Testes com Mocks
[TestFixture]
public class CreateInvoiceCommandHandlerTests
{
    [Test]
    public async Task Should_Create_Invoice()
    {
        var mockContext = new Mock<IApplicationDbContext>();
        var mockIdentityService = new Mock<IIdentityService>();
        var mockLogger = new Mock<ILogger<CreateInvoiceCommandHandler>>();
        
        var handler = new CreateInvoiceCommandHandler(mockContext.Object, mockIdentityService.Object, mockLogger.Object);
        var result = await handler.Handle(new CreateInvoiceCommand(), CancellationToken.None);
        
        Assert.That(result, Is.Not.EqualTo(Guid.Empty)); // ✅ Verifica se Guid é válido
    }
}
```

### 4. Performance

```csharp
// ❌ N+1 Queries
var invoices = await _context.Invoices.ToListAsync();
foreach (var invoice in invoices)
{
    var user = await _context.Users.FirstAsync(u => u.Id == invoice.UserId);
}

// ✅ Eager Loading
var invoices = await _context.Invoices
    .Include(i => i.User)
    .ToListAsync();

// ✅ Projection (Select apenas campos necessários)
var invoiceDtos = await _context.Invoices
    .Select(i => new { i.Id, i.InvoiceNumber, i.Amount })
    .ToListAsync();

// ✅ Usar índices
builder.HasIndex(x => x.InvoiceNumber);
builder.HasIndex(x => new { x.UserId, x.Status });
```

### 5. Segurança

```csharp
// ✅ Autorização em Endpoints
group.RequireAuthorization();
group.RequireAuthorization("admin");

// ✅ Validação de entrada
RuleFor(x => x.Email)
    .NotEmpty()
    .EmailAddress()
    .MaximumLength(256);

// ✅ Proteção contra SQL Injection (EF Core)
var invoice = await _context.Invoices
    .FirstOrDefaultAsync(x => x.InvoiceNumber == invoiceNumber); // Parametrizado automaticamente

// ❌ Nunca
var query = $"SELECT * FROM Invoices WHERE InvoiceNumber = '{invoiceNumber}'"; // Vulnerable!
```

---

## 🐛 Troubleshooting

### Problema: "DbContext is not initialized"

**Solução:**
```csharp
// Certifique-se que está injetado via DI
public MyService(IApplicationDbContext context)
{
    _context = context; // ✅
}

// Não tente criar manualmente
private ApplicationDbContext _context = new(); // ❌
```

### Problema: "Entity of type 'X' cannot be tracked"

**Solução:**
```csharp
// Use .AsNoTracking() para queries read-only
var invoice = await _context.Invoices
    .AsNoTracking()
    .FirstOrDefaultAsync(x => x.Id == id);
```

### Problema: "The instance of entity type cannot be tracked"

**Solução:**
```csharp
// Reattach entidade ao context antes de salvar
_context.Invoices.Update(invoice);
await _context.SaveChangesAsync();
```

### Problema: Migrations não aplicadas

**Solução:**
```bash
# Aplicar todas as pending migrations
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/Web

# Verificar migration pendente
dotnet ef migrations list \
  --project src/Infrastructure \
  --startup-project src/Web
```

### Problema: "Multiple DbContext instances"

**Solução:**
```csharp
// ❌ Evitar
public class MyService
{
    private readonly ApplicationDbContext _context1 = new();
    private readonly ApplicationDbContext _context2 = new();
}

// ✅ Usar uma única instância injetada
public class MyService
{
    private readonly IApplicationDbContext _context;
}
```

### Problema: Testes falhando com "Connection refused"

**Solução:**
```bash
# Inicie o PostgreSQL
docker-compose up -d postgres

# Ou use InMemory para testes
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("TestDb"));
```

---

## 📞 Recursos Úteis

- **Documentação Clean Architecture**: https://cleanarchitecture.jasontaylor.dev
- **MediatR Documentation**: https://github.com/jbogard/MediatR
- **FluentValidation**: https://docs.fluentvalidation.net/
- **Entity Framework Core**: https://learn.microsoft.com/en-us/ef/core/
- **ASP.NET Core**: https://learn.microsoft.com/en-us/aspnet/core/

---

## 🎯 Checklist para Nova Feature

- [ ] Criar entidade de domínio (`src/Domain/Entities/`)
- [ ] Criar DTOs (`src/Application/YourFeature/DTOs/`)
- [ ] Criar Commands/Queries (`src/Application/YourFeature/Commands/` ou `Queries/`)
- [ ] Criar Handlers (implementar `IRequestHandler<>`)
- [ ] Criar Validators (herdar de `AbstractValidator<>`)
- [ ] Criar AutoMapper Profile (herdar de `Profile`)
- [ ] Criar EF Core Configuration (`src/Infrastructure/Data/Configurations/`)
- [ ] Adicionar DbSet ao `ApplicationDbContext`
- [ ] Criar Migration (`dotnet ef migrations add`)
- [ ] Criar Endpoints (`src/Web/Endpoints/`)
- [ ] Registrar Endpoints no `Program.cs`
- [ ] Adicionar Testes (Unit, Integration, Functional)
- [ ] Documentar API com OpenAPI/Swagger

