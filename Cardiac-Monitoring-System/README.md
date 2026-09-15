![CI](https://github.com/ShahdOmari/Shahd-Omari----BinX-Backend-Internship-/actions/workflows/ci.yml/badge.svg)

# Cardiac Patient Monitoring System

A production-grade REST API for monitoring cardiac patients in a hospital setting —
recording vital signs, managing medications and appointments, and enforcing
role-based access control across clinical staff.

Built as a capstone project for the **BinX Tech .NET Backend Internship Program**,
Weeks 6–10.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 10 |
| ORM | Entity Framework Core 10 |
| Database | SQL Server (LocalDB for development) |
| Caching | Redis via StackExchange.Redis + IDistributedCache |
| Authentication | ASP.NET Core Identity + JWT Bearer |
| Validation | FluentValidation |
| API Docs | Swashbuckle / Swagger UI (OAS 3.0) |
| Testing | xUnit, Moq, WebApplicationFactory, SQLite in-memory |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (included with Visual Studio) or SQL Server Express
- [Redis](https://redis.io/) — Memurai (Windows) or Docker (`docker run -d -p 6379:6379 redis:alpine`)

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/ShahdOmari/Shahd-Omari----BinX-Backend-Internship-.git
cd Shahd-Omari----BinX-Backend-Internship-\Cardiac-Monitoring-System
```

### 2. Configure the connection string

Edit `src/CardiacMonitoring.Api/appsettings.json` — the default targets SQL Server LocalDB:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CardiacMonitoringDb;Trusted_Connection=True;",
    "Redis": "localhost:6379"
  }
}
```

For a full SQL Server instance, replace `(localdb)\\mssqllocaldb` with your server address.

### 3. Configure JWT settings

In `appsettings.json` (or via user secrets for production):

```json
{
  "Jwt": {
    "Key": "your-secret-key-minimum-32-characters-long",
    "Issuer": "CardiacMonitoringApi",
    "Audience": "CardiacMonitoringApiUsers"
  }
}
```

### 4. Apply database migrations

```bash
cd src/CardiacMonitoring.Api
dotnet ef database update
```

This applies all migrations and seeds:
- 20 patients, 300 vital sign readings, 40 medications, 40 appointments
- One Admin account: `admin@cardiac.com` / `AdminPass123!`

### 5. Start Redis

```bash
# Windows — start Memurai service, or:
docker run -d --name cardiac-redis -p 6379:6379 redis:alpine
```

### 6. Run the API

```bash
dotnet run
```

API available at: `http://localhost:5286`
Swagger UI: `http://localhost:5286/swagger`

---

## Environment Variables

| Variable | Description | Default |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | LocalDB |
| `ConnectionStrings__Redis` | Redis connection string | `localhost:6379` |
| `Jwt__Key` | JWT signing key (min 32 chars) | *(required)* |
| `Jwt__Issuer` | JWT issuer claim | `CardiacMonitoringApi` |
| `Jwt__Audience` | JWT audience claim | `CardiacMonitoringApiUsers` |

---

## Running Tests

```bash
dotnet test
```

The test suite uses SQLite in-memory (no SQL Server required) and replaces Redis
with `AddDistributedMemoryCache()` — no external dependencies needed to run tests.
Test summary: total: 34, failed: 0, succeeded: 34

---

## API Overview

### Authentication

All endpoints except `POST /Auth/register` and `POST /Auth/login` require a JWT
Bearer token. Obtain one by registering and logging in:

```bash
# Register (returns Nurse role by default)
POST /api/v1/Auth/register
{ "email": "nurse@hospital.ps", "password": "Pass@123!", "department": "Cardiology" }

# Login
POST /api/v1/Auth/login
{ "email": "nurse@hospital.ps", "password": "Pass@123!" }
# → returns { "token": "eyJ..." }

# Use token in all subsequent requests:
Authorization: Bearer eyJ...
```

### Roles

| Role | Default? | Can do |
|---|---|---|
| **Nurse** | ✅ At registration | Record vitals, view patients, view own readings |
| **Doctor** | ❌ Admin grants | Full clinical access, prescribe/delete medications |
| **Auditor** | ❌ Admin grants | Read-only access across all data |
| **Admin** | Seeded once | Manage roles, system administration |

```bash
# Admin promotes a Nurse to Doctor:
POST /api/v1/Auth/assign-role?email=nurse@hospital.ps&role=Doctor
Authorization: Bearer <admin-token>
```

### Key Endpoints

| Method | Endpoint | Access | Description |
|---|---|---|---|
| GET | `/api/v1/Patients` | All staff | Paginated patient list (Redis cached) |
| POST | `/api/v1/Patients` | All staff | Register new patient |
| DELETE | `/api/v1/Patients/{id}` | Doctor, Admin | Permanently remove patient |
| GET | `/api/v1/VitalSigns` | Doctor, Auditor, Admin | All readings with patient names |
| POST | `/api/v1/VitalSigns` | All staff | Record new reading |
| GET | `/api/v1/VitalSigns/{id}` | Owner or Doctor+ | Single reading (ownership enforced) |
| GET | `/api/v1/VitalSigns/critical` | Doctor, Admin | Latest critical reading per patient |
| POST | `/api/v1/Medications` | Doctor, Admin | Prescribe medication |
| DELETE | `/api/v1/Medications/{id}` | Doctor, Admin | Remove medication |

Full interactive documentation: **http://localhost:5286/swagger**

---

## Architecture Highlights

**Layered security model:**
Request → Rate Limiter → JWT Auth → Role Check → Ownership Check → Controller
↓
AuditLoggingMiddleware

**Performance (Sprint 3):**
- `GET /VitalSigns/critical`: correlated SQL subquery returns 7 rows instead of full table scan on 300+
- `GET /Patients`: Redis cache-aside — miss ~30ms, hit ~3ms (98% faster)
- Composite indexes on `VitalSigns(RiskLevel, RecordedAtUtc)` and `VitalSigns(PatientId, RecordedAtUtc)`

---

## Project Structure
Cardiac-Monitoring-System/
├── src/
│ └── CardiacMonitoring.Api/
│ ├── Controllers/ # API endpoints
│ ├── Data/ # EF Core DbContext + migrations
│ ├── DTOs/ # Request/response shapes
│ ├── Entities/ # Domain models
│ ├── Identity/ # ApplicationUser (extends IdentityUser)
│ ├── Middleware/ # AuditLoggingMiddleware, GlobalExceptionHandler
│ ├── Repositories/ # Generic repository pattern
│ ├── Services/ # VitalSignService, CacheService, RiskEvaluator
│ └── Program.cs
└── tests/
└── CardiacMonitoring.Tests/
├── Integration/ # WebApplicationFactory-based API tests
└── Unit/ # CardiacRiskEvaluator, VitalSignService tests

---

## Sprint History

| Sprint | Week | Focus |
|---|---|---|
| Sprint 1 | Week 6 | Core CRUD API, EF Core, validation, pagination |
| Sprint 2 | Week 7 | ASP.NET Core Identity, JWT, RBAC, audit middleware |
| Sprint 3 | Week 8 | Query optimization, Redis caching, database indexes |
| Sprint 4 | Week 9 | Test coverage, API documentation |

