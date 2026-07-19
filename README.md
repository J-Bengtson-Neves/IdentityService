# IdentityService

Serviço de identidade baseado em **Clean Architecture** com autenticação JWT, autorização e gerenciamento de usuários.

O projeto foi gerado usando o [Clean.Architecture.Solution.Template](https://github.com/jasontaylordev/CleanArchitecture) versão 10.8.0.

---

## 📚 Documentação

### Para Entender a Arquitetura

📖 **[ARCHITECTURE.md](./ARCHITECTURE.md)** - Documentação Arquitetural Completa
- Padrão Clean Architecture
- Estrutura de camadas
- Stack tecnológico
- Componentes principais
- Fluxo de dados
- Autenticação e autorização
- Integração de dependências

### Para Começar a Desenvolver

💻 **[DEVELOPMENT.md](./DEVELOPMENT.md)** - Guia Prático de Desenvolvimento
- Quick Start
- Como implementar uma feature passo-a-passo
- Padrões de codificação
- Boas práticas (SOLID, DDD)
- Testabilidade
- Troubleshooting

### Para Referência Rápida

⚡ **[QUICK_REFERENCE.md](./QUICK_REFERENCE.md)** - Referência Rápida
- Comandos essenciais
- Configuração de ambiente
- Stack de versões
- Endpoints padrão
- Estrutura de resposta
- Exemplos práticos

### Para Entender o Boilerplate

🎁 **[BOILERPLATE.md](./BOILERPLATE.md)** - Guia do Template
- O que vem pronto
- O que você precisa customizar
- Exemplos de boilerplate
- Checklist de implementação

---

## ⚡ Quick Start

### 1. Configurar Ambiente

```bash
# Clone e navegue
git clone <repo>
cd IdentityService

# Restaurar pacotes
dotnet restore

# Inicie o banco de dados
docker-compose up -d postgres
```

### 2. Executar a Aplicação

```bash
# Via .NET Aspire (recomendado)
dotnet run --project src/AppHost

# Dashboard: http://localhost:15000
# API: http://localhost:5000/scalar
```

### 3. Testar

```bash
dotnet test
```

---

## 🏗️ Arquitetura

```
┌─────────────────────────────────────────┐
│         Presentation (Web/API)          │  ← REST Endpoints
├─────────────────────────────────────────┤
│     Application (Use Cases/Business)    │  ← MediatR + Validators
├─────────────────────────────────────────┤
│         Domain (Entities/Rules)         │  ← Lógica Pura
├─────────────────────────────────────────┤
│     Infrastructure (External Services)  │  ← EF Core, Identity, JWT
└─────────────────────────────────────────┘
```

**Veja mais em [ARCHITECTURE.md](./ARCHITECTURE.md)**

---

## 🔧 Stack Tecnológico

| Tecnologia | Versão | Propósito |
|-----------|--------|----------|
| **.NET** | 9.0 | Framework |
| **PostgreSQL** | 16 | Banco de dados |
| **MediatR** | 12.x | CQRS |
| **Entity Framework Core** | 9.0 | ORM |
| **FluentValidation** | 11.x | Validação |
| **JWT** | - | Autenticação |
| **Docker** | - | Containerização |
| **.NET Aspire** | - | Orquestração |

---

## 📁 Estrutura de Pastas

```
IdentityService/
├── src/
│   ├── Domain/              # Entidades, Regras de Negócio
│   ├── Application/         # Use Cases (Commands/Queries)
│   ├── Infrastructure/      # EF Core, Identity, Auth
│   ├── Web/                 # API REST
│   ├── Shared/              # Código compartilhado
│   └── AppHost/             # Aspire
├── tests/                   # Testes (Unit, Integration, E2E)
├── ARCHITECTURE.md          # 📖 Documentação arquitetural
├── DEVELOPMENT.md           # 💻 Guia de desenvolvimento
├── QUICK_REFERENCE.md       # ⚡ Referência rápida
├── BOILERPLATE.md           # 🎁 Guia do template
├── Dockerfile               # Build/Deploy
├── docker-compose.yml       # Dev environment
└── README.md                # Este arquivo
```

---

## Build

Executar build da solução:

```bash
dotnet build
```

Build Release:

```bash
dotnet build -c Release
```

---

## Run

Executar a aplicação:

```bash
# Via Aspire (recomendado - abre dashboard automaticamente)
dotnet run --project src/AppHost

# Direto
dotnet run --project src/Web
```

**Dashboard Aspire**: http://localhost:15000  
**API**: http://localhost:5000  
**Documentação**: http://localhost:5000/scalar

---

## Testes

A solução contém testes unitários, de integração e funcionais.

Executar todos os testes:

```bash
dotnet test
```

Testes de um projeto específico:

```bash
dotnet test --filter "FullyQualifiedName~Domain.UnitTests"
```

Com cobertura:

```bash
dotnet test /p:CollectCoverage=true
```

---

## Migrações de Banco de Dados

### Criar migration

```bash
dotnet ef migrations add "MigrationName" \
  --project src/Infrastructure \
  --startup-project src/Web
```

### Aplicar migrations

```bash
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/Web
```

---

## Docker

### Build

```bash
docker build -t identity-service:latest .
```

### Docker Compose (Desenvolvimento)

```bash
# Iniciar
docker-compose up -d

# Parar
docker-compose down

# Logs
docker-compose logs -f identity-api
```

---

## Estilos de Código

O template inclui suporte [EditorConfig](https://editorconfig.org/) para manter estilos consistentes. O arquivo **.editorconfig** define os estilos aplicáveis.

---

## Code Scaffolding

O template inclui suporte para scaffold de novos commands e queries.

Navegue até `src/Application/`:

Criar novo command:

```bash
dotnet new ca-usecase \
  --name CreateTodoList \
  --feature-name TodoLists \
  --usecase-type command \
  --return-type Guid
```

Criar nova query:

```bash
dotnet new ca-usecase \
  --name GetTodos \
  --feature-name TodoLists \
  --usecase-type query \
  --return-type TodosVm
```

Se encontrar erro *"No templates or subcommands found"*, instale o template:

```bash
dotnet new install Clean.Architecture.Solution.Template::10.8.0
```

---

## 🚀 Como Implementar uma Feature

1. **Ler** [DEVELOPMENT.md](./DEVELOPMENT.md) - Guia passo-a-passo
2. **Copiar** boilerplate de [BOILERPLATE.md](./BOILERPLATE.md)
3. **Referenciar** padrões em [ARCHITECTURE.md](./ARCHITECTURE.md)
4. **Consultar** comandos em [QUICK_REFERENCE.md](./QUICK_REFERENCE.md)

---

## 📞 Ajuda

Para mais informações:

- 📖 [Documentação Arquitetural](./ARCHITECTURE.md)
- 💻 [Guia de Desenvolvimento](./DEVELOPMENT.md)
- ⚡ [Referência Rápida](./QUICK_REFERENCE.md)
- 🎁 [Boilerplate & Template](./BOILERPLATE.md)
- 🔗 [Clean Architecture Website](https://cleanarchitecture.jasontaylor.dev)
- 📚 [MediatR Documentation](https://github.com/jbogard/MediatR)
- 🗄️ [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)

---

## 📝 Notas

- O projeto usa **PostgreSQL 16** para desenvolvimento
- Autenticação via **JWT Bearer Tokens**
- Autorização baseada em **Roles (RBAC)**
- Observabilidade via **.NET Aspire Dashboard**
- Deploy em **Azure Container Apps**

Comece deletando a pasta `WeatherForecasts/` e implementando suas features!