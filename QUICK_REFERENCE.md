# Referência Rápida - IdentityService

## 🗂️ Comandos Essenciais

### Build & Run

```bash
# Restaurar pacotes
dotnet restore

# Build
dotnet build

# Build Release
dotnet build -c Release

# Executar via Aspire
dotnet run --project src/AppHost

# Executar direto Web
dotnet run --project src/Web
```

### Testes

```bash
# Todos os testes
dotnet test

# Teste específico
dotnet test --filter "FullyQualifiedName~CreateInvoiceCommandHandlerTests"

# Com cobertura
dotnet test /p:CollectCoverage=true

# Sem cache
dotnet test --no-build
```

### Banco de Dados

```bash
# Criar migration
dotnet ef migrations add "MigrationName" \
  --project src/Infrastructure \
  --startup-project src/Web

# Aplicar migrations
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/Web

# Remover última migration
dotnet ef migrations remove \
  --project src/Infrastructure \
  --startup-project src/Web

# Ver SQL da migration
dotnet ef migrations script "OldMigration" "NewMigration" \
  --project src/Infrastructure \
  --startup-project src/Web
```

### Docker

```bash
# Build imagem
docker build -t identity-service:latest .

# Executar container
docker run -p 5001:8080 identity-service:latest

# Docker Compose - iniciar
docker-compose up -d

# Docker Compose - parar
docker-compose down

# Docker Compose - remover volumes
docker-compose down -v

# Logs
docker-compose logs -f identity-api
```

### Scaffolding

```bash
# Novo Command
dotnet new ca-usecase \
  --name CreateInvoice \
  --feature-name Invoices \
  --usecase-type command \
  --return-type Guid

# Nova Query
dotnet new ca-usecase \
  --name GetInvoices \
  --feature-name Invoices \
  --usecase-type query \
  --return-type InvoicesVm
```

---

## 🔧 Configuração de Ambiente

### Desenvolvimento (docker-compose.yml)

```env
ConnectionStrings__IdentityServiceDb=Host=postgres;Port=5432;Database=identityservice;Username=identity;Password=supersecret
ASPNETCORE_ENVIRONMENT=Development
Jwt__Secret=my_super_secret_jwt_key_that_is_long_enough_32bytes!
Jwt__Issuer=IdentityService
Jwt__Audience=IdentityService.Clients
Jwt__AccessTokenExpirationMinutes=15
Jwt__RefreshTokenExpirationDays=7
```

### Produção (Azure)

```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__IdentityServiceDb=Server=<db-server>;Database=identityservice;Port=5432;User Id=<user>;Password=<password>;
Jwt__Secret=<value-from-keyvault>
Jwt__Issuer=IdentityService
Jwt__Audience=IdentityService.Clients
Jwt__AccessTokenExpirationMinutes=15
Jwt__RefreshTokenExpirationDays=7
```

---

## 🏗️ Stack Versões

| Componente | Versão |
|-----------|--------|
| .NET SDK | 10.0.302 |
| .NET Runtime | 9.0 |
| PostgreSQL | 16-alpine |
| Node.js | 20.x |
| MediatR | 12.x |
| Entity Framework Core | 9.0 |
| FluentValidation | 11.x |
| AutoMapper | 13.x |

---

## 📡 API Endpoints Padrão

### Autenticação (Auto-mapeado pelo Identity)

```
POST   /register            # Registrar novo usuário
POST   /login               # Login
POST   /refresh             # Refresh token
POST   /logout              # Logout
GET    /me                  # Obter usuário atual
```

### Documentação

```
GET    /openapi/v1.json     # OpenAPI Schema
GET    /scalar              # Swagger UI alternativo
GET    /swagger/ui          # Swagger UI padrão
```

### Health Check

```
GET    /health              # Health check
GET    /health/ready        # Readiness probe
GET    /health/live         # Liveness probe
```

---

## 📊 Estrutura de Resposta

### Success (2xx)

```json
{
  "status": 200,
  "message": "Operação realizada com sucesso"
}
```

### Error (4xx/5xx)

```json
{
  "type": "about:blank",
  "title": "Unprocessable Entity",
  "status": 422,
  "detail": "Um ou mais erros de validação ocorreram.",
  "errors": {
    "email": ["Email é obrigatório"]
  }
}
```

---

## 🔐 JWT Token Exemplo

### Header
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

### Payload
```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "name": "User Name",
  "aud": "IdentityService.Clients",
  "iss": "IdentityService",
  "iat": 1234567890,
  "exp": 1234568790
}
```

---

## 🗄️ Estrutura de Banco de Dados

### Tabelas Principais

```sql
-- AspNetUsers (Identity)
CREATE TABLE AspNetUsers (
  Id SERIAL PRIMARY KEY,
  UserName VARCHAR(256),
  NormalizedUserName VARCHAR(256),
  Email VARCHAR(256),
  NormalizedEmail VARCHAR(256),
  EmailConfirmed BOOLEAN,
  PasswordHash TEXT,
  ...
);

-- AspNetRoles
CREATE TABLE AspNetRoles (
  Id SERIAL PRIMARY KEY,
  Name VARCHAR(256),
  NormalizedName VARCHAR(256),
  ...
);

-- AspNetUserRoles (Relacionamento)
CREATE TABLE AspNetUserRoles (
  UserId INT FOREIGN KEY,
  RoleId INT FOREIGN KEY,
  PRIMARY KEY (UserId, RoleId)
);

-- Suas Entidades
CREATE TABLE Invoices (
  Id SERIAL PRIMARY KEY,
  InvoiceNumber VARCHAR(50) UNIQUE NOT NULL,
  ClientName VARCHAR(200) NOT NULL,
  Amount DECIMAL(18,2) NOT NULL,
  Status INT NOT NULL,
  DueDate TIMESTAMP NOT NULL,
  UserId INT FOREIGN KEY,
  Created TIMESTAMP NOT NULL,
  CreatedBy VARCHAR(256),
  LastModified TIMESTAMP,
  LastModifiedBy VARCHAR(256),
  ...
);
```

---

## 🧪 Exemplo de Teste Unitário

```csharp
[TestFixture]
public class CreateInvoiceCommandHandlerTests
{
    private Mock<IApplicationDbContext> _mockContext;
    private Mock<IUser> _mockUser;
    private CreateInvoiceCommandHandler _handler;
    
    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockUser = new Mock<IUser>();
        _handler = new CreateInvoiceCommandHandler(_mockContext.Object, _mockUser.Object);
    }
    
    [Test]
    public async Task Handle_Should_Create_Invoice_Successfully()
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            InvoiceNumber = "INV-001",
            ClientName = "ACME Corp",
            Amount = 1000,
            DueDate = DateTime.Today.AddDays(30)
        };
        
        _mockUser.Setup(x => x.Id).Returns("1");
        
        var invoices = new List<Invoice>();
        var mockSet = new Mock<DbSet<Invoice>>();
        
        mockSet.Setup(m => m.Add(It.IsAny<Invoice>()))
            .Callback<Invoice>(invoices.Add);
        
        _mockContext.Setup(x => x.Invoices).Returns(mockSet.Object);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.That(result, Is.GreaterThan(0));
        Assert.That(invoices.Count, Is.EqualTo(1));
        Assert.That(invoices[0].InvoiceNumber, Is.EqualTo("INV-001"));
    }
}
```

---

## 🚨 Erros Comuns

| Erro | Causa | Solução |
|------|-------|--------|
| `DbContext is not initialized` | Não foi injetado via DI | Injetar via construtor |
| `Multiple DbContext instances` | Criando múltiplas instâncias | Usar DI singleton/scoped |
| `Connection refused` | PostgreSQL não está rodando | `docker-compose up -d postgres` |
| `Migration pending` | Migrations não foram aplicadas | `dotnet ef database update` |
| `Entity not tracked` | Entidade não está no contexto | Reattach com `.Update()` |
| `Authentication fails` | JWT inválido/expirado | Renovar token via refresh |
| `Unauthorized 401` | Sem token no header | Adicionar `Authorization: Bearer <token>` |
| `Forbidden 403` | Sem permissão (role) | Adicionar role ao usuário |

---

## 📝 Variáveis de Ambiente (appsettings.json)

```json
{
  "ConnectionStrings": {
    "IdentityServiceDb": "Host=localhost;Port=5432;Database=identityservice;Username=identity;Password=supersecret"
  },
  "Jwt": {
    "Secret": "my_super_secret_jwt_key_that_is_long_enough_32bytes!",
    "Issuer": "IdentityService",
    "Audience": "IdentityService.Clients",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

---

## 🔗 Links Úteis

- [Dashboard Aspire](http://localhost:15000)
- [API Documentation](http://localhost:5000/scalar)
- [OpenAPI Schema](http://localhost:5000/openapi/v1.json)
- [PostgreSQL Docs](https://www.postgresql.org/docs/)
- [EF Core Docs](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Docs](https://learn.microsoft.com/en-us/aspnet/core/)

---

## � Chave Primária - Use GUID

### Por Que GUID?

✅ **Segurança**: Não expõe padrão previsível  
✅ **Distribuição**: Sem conflitos em múltiplos bancos  
✅ **Replicação**: Suporta merge sem conflitos  
✅ **Escalabilidade**: Funciona bem com sharding  

### Como Implementar

```csharp
// ✅ Correto
public class Invoice : BaseEntity
{
    // Id: Guid (herdado de BaseEntity)
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; }
}

// ❌ Evitar
public class Invoice : BaseEntity
{
    public int Id { get; set; } // NUNCA use int
}
```

### Configuração EF Core (PostgreSQL)

```csharp
builder.Property(x => x.Id)
    .HasDefaultValueSql("gen_random_uuid()");
```

### Foreign Keys

```csharp
// ✅ Correto
public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; } // FK também é Guid
    public Order Order { get; set; }
}

// ❌ Evitar
public class OrderItem
{
    public Guid Id { get; set; }
    public int OrderId { get; set; } // Tipo diferente - ERRADO
}
```

---

## �💡 Dicas de Performance

### DbContext
```csharp
// ❌ Evitar
var items = context.Items.ToList();

// ✅ Usar
var items = await context.Items.ToListAsync();

// ✅ Eager Loading
var items = await context.Items
    .Include(x => x.Children)
    .ToListAsync();

// ✅ Projection
var dtos = await context.Items
    .Select(x => new ItemDto { Id = x.Id, Name = x.Name })
    .ToListAsync();

// ✅ Paginação
var items = await context.Items
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

### Índices
```csharp
// Adicionar índices em colunas frequentemente buscadas
builder.HasIndex(x => x.Email).IsUnique();
builder.HasIndex(x => x.Status);
builder.HasIndex(x => new { x.UserId, x.Status }); // Composite
```

### Conexão
```csharp
// Connection pooling (automático)
// Ajustar se necessário
services.AddDbContext<DbContext>(options =>
    options.UseNpgsql(connectionString, o =>
        o.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetialDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null)));
```

---

## 🐳 Docker Compose Referência

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
    networks:
      - identity-network

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
    networks:
      - identity-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U identity"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  pgdata:

networks:
  identity-network:
    driver: bridge
```

---

## 🎯 Checklist de Deployment

### Pré-Deployment
- [ ] Todos os testes passando
- [ ] Build Release realizado
- [ ] Migrations atualizadas
- [ ] Segredos em Key Vault
- [ ] Variáveis de ambiente corretas
- [ ] HTTPS habilitado
- [ ] CORS configurado

### Deployment
- [ ] Build imagem Docker
- [ ] Push para registry
- [ ] Aplicar migrations
- [ ] Atualizar config
- [ ] Fazer health check
- [ ] Monitorar logs

### Pós-Deployment
- [ ] Verificar status da app
- [ ] Teste de smoke
- [ ] Monitorar métricas
- [ ] Rollback plan preparado

---

## 📚 Estrutura de Pastas em Detalhes

```
IdentityService/
├── src/
│   ├── Domain/                    # Lógica pura de negócio
│   ├── Application/               # Use cases
│   ├── Infrastructure/            # Detalhes técnicos
│   ├── Web/                       # API REST
│   ├── Shared/                    # Constantes compartilhadas
│   └── AppHost/                   # Orchestração Aspire
├── tests/
│   ├── Domain.UnitTests/
│   ├── Application.UnitTests/
│   ├── Infrastructure.IntegrationTests/
│   ├── Application.FunctionalTests/
│   └── TestAppHost/
├── artifacts/                     # Outputs de build
├── .github/
│   └── workflows/                 # CI/CD (se usar)
├── Directory.Build.props          # Props globais
├── Directory.Packages.props       # Versions centralizadas
├── global.json                    # .NET SDK version
├── Dockerfile                     # Build/Deploy
├── docker-compose.yml             # Dev environment
├── ARCHITECTURE.md                # Este arquivo
├── DEVELOPMENT.md                 # Guia de desenvolvimento
└── README.md                      # Quick start
```

---

## 🔄 Git Workflow

```bash
# Criar branch
git checkout -b feature/user-management

# Fazer commits
git add .
git commit -m "feat: implementar CRUD de usuários"

# Push
git push origin feature/user-management

# Criar PR
# Após aprovação

# Merge
git merge feature/user-management
git push origin main
```

---

## 📞 Suporte

Para mais informações:
1. Consulte [ARCHITECTURE.md](./ARCHITECTURE.md)
2. Consulte [DEVELOPMENT.md](./DEVELOPMENT.md)
3. Acesse a [documentação oficial](https://cleanarchitecture.jasontaylor.dev)
4. Verifique [MediatR docs](https://github.com/jbogard/MediatR)

