# 🏗️ __PROJECT_NAME__ - Clean Architecture .NET Template

A production-ready .NET 8 Web API template following Clean Architecture principles, SOLID design patterns, and industry best practices.

---

## 🎯 Features

### Core Architecture
- ✅ **Clean Architecture** with clear separation of concerns (Domain, Application, Infrastructure, API)
- ✅ **SOLID Principles** implementation throughout the codebase
- ✅ **Repository Pattern** with Unit of Work
- ✅ **Dependency Injection** with built-in .NET DI container
- ✅ **AutoMapper** for object-to-object mapping

### Security & Authentication
- 🔐 **JWT Bearer Authentication** with customizable token lifetime
- 🔐 **ASP.NET Core Identity** for user management
- 🔐 **Permission-based Authorization** with granular access control
- 🔐 **Rate Limiting** (AspNetCoreRateLimit)
- 🔐 **Password Policies** (complexity, lockout, etc.)

### Logging & Monitoring
- 📊 **Serilog** for structured logging
- 📊 **Health Checks** with custom application checks
- 📊 **Activity Logging Middleware** for audit trails
- 📊 **Telemetry** for user activity tracking
- 📊 **Automatic Log Cleanup** service

### API Documentation
- 📚 **Swagger/OpenAPI** with JWT authentication integration
- 📚 **XML Documentation** support for all endpoints

### Database & Persistence
- 💾 **Entity Framework Core 8** with multi-provider support (SQL Server, PostgreSQL)
- 💾 **Automatic Migrations** on startup
- 💾 **Database Seeding** for initial data
- 💾 **Audit Trail** with automatic tracking of entity changes
- 💾 **Soft Delete** pattern implementation
- 💾 **Database-agnostic design** for easy provider switching

### Additional Features
- ⚡ **CORS** configuration
- ⚡ **Background Services** (log cleanup, session management)
- ⚡ **Error Handling Middleware** with standardized responses
- ⚡ **Session Heartbeat** for active user tracking
- ⚡ **Environment-specific** configurations

---

## 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher
- Database server (choose one):
  - [SQL Server 2019+](https://www.microsoft.com/sql-server) or SQL Server Express (Default)
  - [PostgreSQL 14+](https://www.postgresql.org/download/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) / [VS Code](https://code.visualstudio.com/) / [Rider](https://www.jetbrains.com/rider/)
- [Git](https://git-scm.com/)

---

## 🚀 Quick Start

### 1️⃣ Clone or Use Template

```bash
# Clone this repository
git clone <your-repository-url>
cd __PROJECT_NAME__

# Or use as GitHub template
# Click "Use this template" button on GitHub
```

### 2️⃣ Customize Project Names

Run the setup script to replace placeholders with your project name:

```bash
# Windows (PowerShell)
.\setup-project.ps1 -ProjectName "YourProjectName"

# Linux/macOS
./setup-project.sh "YourProjectName"
```

This will replace all occurrences of `__PROJECT_NAME__` with your actual project name.

### 3️⃣ Configure Database

The template supports both SQL Server and PostgreSQL. Choose your preferred provider and update the configuration in `Api-YourProjectName/appsettings.Development.json`:

**For SQL Server (Default):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=YourDatabase;User Id=youruser;Password=yourpassword;TrustServerCertificate=True;"
  },
  "Database": {
    "Provider": "SqlServer"
  }
}
```

**For PostgreSQL:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=YourDatabase;Username=youruser;Password=yourpassword;Include Error Detail=true"
  },
  "Database": {
    "Provider": "PostgreSQL"
  }
}
```

### 4️⃣ Configure JWT Secret

Generate a secure JWT secret (minimum 32 characters):

```bash
# PowerShell
-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | ForEach-Object {[char]$_})

# Linux/macOS
openssl rand -base64 32
```

Update `appsettings.Development.json`:

```json
{
  "Authentication": {
    "JwtSecret": "your-generated-secret-here",
    "Issuer": "YourProjectAPI",
    "Audience": "YourProjectClients"
  }
}
```

### 5️⃣ Run the Application

```bash
cd Api-YourProjectName
dotnet restore
dotnet run
```

Or with hot reload:

```bash
dotnet watch run
```

The API will be available at:
- **HTTPS**: `https://localhost:7149`
- **HTTP**: `http://localhost:5149`
- **Swagger**: `https://localhost:7149/swagger`

### 6️⃣ Test the API

Default admin credentials (created automatically on first run):
- **Username**: `admin`
- **Password**: `Admin123!*` (change this in `SeedDB.cs`)

```bash
# Login
curl -X POST https://localhost:7149/api/Auth/admin/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!*"}'

# Access protected endpoint
curl -X GET https://localhost:7149/api/YourEndpoint \
  -H "Authorization: Bearer {your-jwt-token}"
```

---

## 💾 Database Configuration

This template supports **multiple database providers** out of the box, allowing you to choose between SQL Server and PostgreSQL without code changes.

### Supported Database Providers

- **SQL Server** (Default)
- **PostgreSQL**

### Configuration

Database configuration is managed through `appsettings.json` with two key settings:

1. **Connection String**: Located in `ConnectionStrings:DefaultConnection`
2. **Database Provider**: Located in `Database:Provider`

#### Example Configuration (SQL Server - Default)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=YourDatabase;User Id=youruser;Password=yourpassword;TrustServerCertificate=True;MultipleActiveResultSets=True;"
  },
  "Database": {
    "Provider": "SqlServer"
  }
}
```

#### Example Configuration (PostgreSQL)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=YourDatabase;Username=youruser;Password=yourpassword;Include Error Detail=true"
  },
  "Database": {
    "Provider": "PostgreSQL"
  }
}
```

### How It Works

The database persistence is configured using an extension method `AddPersistence(services, configuration)` located in `CC.Infrastructure.Extensions.PersistenceExtensions`.

This extension method:
1. Reads the `Database:Provider` setting from configuration
2. Reads the connection string from `ConnectionStrings:DefaultConnection`
3. Configures Entity Framework Core with the appropriate database provider:
   - For **SqlServer**: Uses `UseSqlServer()` with retry on failure (5 attempts, 10 seconds max delay)
   - For **PostgreSQL**: Uses `UseNpgsql()` with retry on failure (5 attempts, 10 seconds max delay)
4. Registers the `AuditingSaveChangesInterceptor` if auditing is enabled

The configuration is called in `Program.cs`:
```csharp
builder.Services.AddPersistence(builder.Configuration);
```

### Environment-Specific Configuration

You can configure different database providers per environment:

- `appsettings.json` - Base configuration (SqlServer by default)
- `appsettings.Development.json` - Development environment (can use local SQL Server or PostgreSQL)
- `appsettings.Production.json` - Production environment
- `appsettings.qa.json` - QA environment

### Database-Agnostic Design

The `DBContext` has been designed to be database-agnostic:
- No SQL Server-specific functions (like `NEWID()`, `GETUTCDATE()`) in entity configurations
- `Id` (Guid) and `DateCreated` (DateTime) are generated in application code via the `EntityBase<T>` constructor
- Migrations can be generated for both SQL Server and PostgreSQL

### Generating Migrations

When generating migrations, ensure you have the correct database provider configured:

```bash
# For SQL Server (default)
cd CC.Infrastructure
dotnet ef migrations add YourMigrationName --startup-project ../Api-__PROJECT_NAME__

# For PostgreSQL (change Provider in appsettings.json first)
cd CC.Infrastructure
dotnet ef migrations add YourMigrationName --startup-project ../Api-__PROJECT_NAME__
```

### Friday Tool Integration

The "Friday" tool can generate projects with either database provider by:
1. Setting the appropriate `Database:Provider` value in the generated `appsettings.json`
2. Providing the correct connection string format for the chosen provider
3. No code changes are required - the template automatically adapts to the configured provider

---

## 🏛️ Project Structure

```
📦 __PROJECT_NAME__
├── 📂 Api-__PROJECT_NAME__/              # 🌐 Presentation Layer (REST API)
│   ├── 📂 Controllers/                   # API endpoints
│   ├── 📂 Handlers/                      # Middleware & DI configuration
│   ├── 📂 HealthChecks/                  # Custom health checks
│   ├── 📂 Services/                      # Background services
│   ├── 📂 Configuration/                 # Authorization policies
│   └── 📄 Program.cs                     # Application entry point
│
├── 📂 __PROJECT_NAME__.Domain/           # 🎯 Domain Layer (Business Logic)
│   ├── 📂 Entities/                      # Domain entities
│   ├── 📂 Dtos/                          # Data Transfer Objects
│   ├── 📂 Interfaces/                    # Service & Repository contracts
│   ├── 📂 Constants/                     # System constants
│   ├── 📂 Enums/                         # Enumerations
│   └── 📄 AutoMapperProfile.cs           # Object mapping configuration
│
├── 📂 __PROJECT_NAME__.Application/      # 💼 Application Layer (Use Cases)
│   ├── 📂 Services/                      # Business logic implementation
│   ├── 📂 Helpers/                       # Utility classes
│   └── 📂 Utils/                         # JWT, encryption, etc.
│
└── 📂 __PROJECT_NAME__.Infrastructure/   # 🔧 Infrastructure Layer (Data & External Services)
    ├── 📂 Configurations/                # EF Core context & seeding
    ├── 📂 Repositories/                  # Data access implementation
    ├── 📂 Authorization/                 # Permission handlers
    ├── 📂 External/                      # External API clients
    └── 📂 Migrations/                    # Database migrations
```

---

## 📚 Architecture Overview

This template follows **Clean Architecture** (also known as Onion Architecture or Hexagonal Architecture):

```
┌─────────────────────────────────────────────┐
│          🌐 API Layer (Presentation)        │
│   Controllers │ Middleware │ Configuration  │
└────────────────────┬────────────────────────┘
                     │
┌────────────────────▼────────────────────────┐
│      💼 Application Layer (Use Cases)       │
│      Services │ Validators │ Mappers        │
└────────────────────┬────────────────────────┘
                     │
┌────────────────────▼────────────────────────┐
│         🎯 Domain Layer (Core Logic)        │
│   Entities │ DTOs │ Interfaces │ Constants  │
└────────────────────┬────────────────────────┘
                     │
┌────────────────────▼────────────────────────┐
│    🔧 Infrastructure Layer (Data & I/O)     │
│   Repositories │ DbContext │ External APIs  │
└─────────────────────────────────────────────┘
```

### Dependency Flow
- **API** → **Application** → **Domain** ← **Infrastructure**
- Domain has **no dependencies** on other layers
- Infrastructure depends on **Domain** only
- Application depends on **Domain** only
- API depends on all layers (composition root)

---

## 🔐 Security Best Practices

### JWT Authentication
- Tokens are signed with HS256 algorithm
- Configurable token lifetime (default: 60 minutes)
- Claims include: UserId, Email, Roles, Permissions
- Automatic token validation on each request

### Password Policies
- Minimum 8 characters
- Requires uppercase, lowercase, digit, and special character
- Account lockout after 5 failed attempts (5 minutes)
- Password hashing with ASP.NET Core Identity

### Rate Limiting
- 100 requests per minute per IP (default)
- Customizable per endpoint
- Authentication endpoints have stricter limits

### Authorization
- Permission-based access control
- Policy-based authorization
- Custom authorization handlers
- Granular permissions (CRUD operations per module)

---

## 🛠️ Configuration

### Environment-Specific Settings

The template supports multiple environments:
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production overrides
- `appsettings.Staging.json` - Staging overrides (optional)

**⚠️ IMPORTANT**: Never commit `appsettings.Development.json` with real credentials!

---

## 📦 Adding New Features

See [DEVELOPMENT_GUIDE.md](./docs/DEVELOPMENT_GUIDE.md) for detailed instructions on:
- Creating new entities
- Adding controllers
- Implementing services
- Creating repositories
- Adding permissions
- Database migrations

---

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 📞 Support

For questions, issues, or suggestions:
- Create an issue on GitHub
- Contact: dev@yourcompany.com

---

## 🙏 Acknowledgments

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) by Robert C. Martin
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core) documentation
- [Entity Framework Core](https://docs.microsoft.com/ef/core/) documentation

---

**Built with ❤️ using Clean Architecture principles**
