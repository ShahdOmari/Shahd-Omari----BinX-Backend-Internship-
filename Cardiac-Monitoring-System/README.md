# Cardiac Patient Monitoring System

### Capstone Project — BinX Backend Development Internship (.NET)
### Applies concepts from Week 1 through Week 5

---

## 1. Project Overview

A standalone ASP.NET Core Web API for monitoring cardiac patients: patient profiles,
vital-sign measurements, medications, and appointments — secured behind real
authentication, validated against real business rules, and covered by a full
automated test suite.

**What makes this more than plain CRUD:** every vital-sign reading is automatically
scored for cardiac risk (`Normal` / `Watch` / `Critical`) the moment it's recorded,
and a dedicated endpoint surfaces which patients need attention right now — a small,
realistic feature that puts generics, LINQ, dependency injection, and async EF Core
to work together, not just CRUD boilerplate demonstrated in isolation.

## 2. Why This Idea

A monitoring system is a natural fit for a backend-only API: it has clear entities
and relationships, a genuine reason for role-based access (a Nurse and a Doctor
don't have the same permissions), and a natural extension point — risk scoring —
that showcases real business logic instead of a token example.

## 3. Tech Stack

| Category | Technology |
|---|---|
| Language & Runtime | C# / .NET 10 |
| Web Framework | ASP.NET Core Web API |
| ORM & Database | Entity Framework Core, SQL Server (LocalDB for development) |
| Identity & Auth | ASP.NET Core Identity, JWT Bearer Authentication |
| Validation | FluentValidation (including async business rules) |
| API Documentation | Swashbuckle (Swagger/OpenAPI) |
| Hardening | Rate limiting, CORS, HSTS/HTTPS redirection |
| Error Handling | `IExceptionHandler` + RFC 7807 `ProblemDetails` |
| Testing | xUnit, Moq, `Microsoft.AspNetCore.Mvc.Testing` (WebApplicationFactory), SQLite in-memory |

## 4. Domain Model

- **Patient** — core profile (name, date of birth, gender)
- **VitalSign** — heart rate, blood pressure, oxygen saturation, timestamp, and a
  computed `RiskLevel`
- **Medication** — name, dosage, frequency, linked to a patient
- **Appointment** — scheduled date/time, doctor, reason, linked to a patient

All four have a one-to-many relationship from `Patient`, enforced with foreign keys
and cascade delete.

## 5. Roles

| Role | Can Do |
|---|---|
| **Nurse** | Register/login, view patients, record vital signs, view medications/appointments |
| **Doctor** | Everything a Nurse can do, plus create/update/delete medications, and delete patient records |

Verified end-to-end with real issued JWTs in `RoleBasedAccessTests.cs` — a Nurse
attempting a Doctor-only action correctly receives `403 Forbidden` (authenticated,
not permitted), never `401` (not authenticated).

## 6. Architecture

```
Cardiac-Monitoring-System/
├── CardiacMonitoring.slnx
├── src/
│   └── CardiacMonitoring.Api/
│       ├── Controllers/         # Patients, VitalSigns, Medications, Appointments, Auth, Diagnostics
│       ├── Entities/             # EF Core domain models (Patient, VitalSign, Medication, Appointment, RiskLevel)
│       ├── DTOs/                  # Request/response records, grouped by resource
│       ├── Data/                  # AppDbContext (extends IdentityDbContext)
│       ├── Migrations/           # EF Core code-first migrations
│       ├── Repositories/         # Generic IRepository<T> / Repository<T>
│       ├── Services/              # IRiskEvaluator/CardiacRiskEvaluator, IVitalSignService/VitalSignService
│       ├── Validators/            # FluentValidation rules (including async PatientId-existence checks)
│       ├── Middleware/            # GlobalExceptionHandler (IExceptionHandler)
│       └── Program.cs
└── tests/
    └── CardiacMonitoring.Tests/
        ├── Services/               # Unit tests (xUnit) + Moq-based service tests
        └── Integration/            # WebApplicationFactory integration tests (SQLite in-memory)
```

**Layering rationale:** business logic (risk scoring, vital-sign recording) lives in
`Services/`, not in controllers — this is what makes the Moq-based unit tests in
`VitalSignServiceTests.cs` possible without touching a real database, and keeps
`VitalSignsController` a thin HTTP-concerns layer.

## 7. The Generic Repository Pattern

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task<bool> SaveChangesAsync();
}
```

One generic implementation (`Repository<T>`), registered once in `Program.cs` via
`AddScoped(typeof(IRepository<>), typeof(Repository<>))`, is reused for `Patient`,
`VitalSign`, `Medication`, and `Appointment` — avoiding four nearly-identical
repository classes.

## 8. The Risk-Scoring Feature

`CardiacRiskEvaluator` applies threshold rules to every new `VitalSign` reading
*before* it's saved, so a stored reading always carries the risk level that was true
the moment it was actually recorded:

```csharp
bool isCritical = v.HeartRateBpm > 130 || v.HeartRateBpm < 40
    || v.SystolicBp > 180 || v.SystolicBp < 80
    || v.OxygenSaturationPercent < 90;

bool isWatch = v.HeartRateBpm is > 100 or < 50
    || v.SystolicBp is > 140 or < 90
    || v.OxygenSaturationPercent < 95;
```

`GET /api/v1/VitalSigns/critical` uses LINQ (`GroupBy` → `OrderByDescending` →
`First` → `Where`) to return only the most recent reading per patient, filtered to
those currently `Critical` — a genuinely useful "who needs attention right now" view.

## 9. Authentication & Authorization

- **Identity** handles user storage and password hashing (PBKDF2, salted per user) —
  no custom hashing code anywhere in the project.
- **JWT** issued on login, with the user's roles embedded as claims at token-issue
  time (`ClaimTypes.Role`) — this is what `[Authorize(Roles = "Doctor")]` actually
  checks against; assigning a role in the database alone is not enough.
- Protected endpoints: everything except `Auth/register` and `Auth/login`.
  `Medications`' create/update/delete actions are additionally restricted to
  `Doctor` only.

## 10. Validation

FluentValidation validators cover every `Create` request, including **async business
rules** — e.g. does the referenced `PatientId` actually exist? Because ASP.NET
Core's automatic model-validation pipeline only supports synchronous rules,
validators are invoked explicitly inside each controller action:

```csharp
var validationResult = await _validator.ValidateAsync(request);
if (!validationResult.IsValid)
    return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
```

## 11. Error Handling

A single `GlobalExceptionHandler` (implementing ASP.NET Core's built-in
`IExceptionHandler`) catches any unhandled exception anywhere in the pipeline,
logs full details server-side with structured fields (`RequestMethod`,
`RequestPath`, `ExceptionType`), and returns a generic, safe `ProblemDetails`
response to the client — the real exception message and stack trace are never sent
externally. A permanent `GET /api/v1/Diagnostics/trigger-error` endpoint exists
purely to verify this end-to-end after any future change.

## 12. Hardening

- **Rate limiting** — a stricter 5-requests/minute limit on `Login` specifically
  (repeated rapid attempts are the clearest brute-force signal), 100/minute general
  limit elsewhere.
- **CORS** — a named policy allowing only a specific known frontend origin, not a
  permissive "allow any origin" policy.
- **HTTPS/HSTS** — HTTPS redirection always on; HSTS enabled outside `Development`.
- **SQL injection** — every query goes through EF Core's LINQ methods, which
  parameterize automatically; the codebase contains zero raw, string-interpolated
  SQL (`FromSqlRaw`/`ExecuteSqlRaw`) — verified by direct search.

## 13. Testing

**24 tests, 24 passing**, spanning three layers:

| Layer | File(s) | What It Covers |
|---|---|---|
| Unit (xUnit) | `CardiacRiskEvaluatorTests.cs` | 10 tests — every risk-classification boundary, via `[Fact]` and a `[Theory]` with 6 boundary cases |
| Unit + Moq | `VitalSignServiceTests.cs` | 3 tests — service logic isolated from the database via mocked `IRepository<VitalSign>` and `IRiskEvaluator`; verifies `AddAsync`/`SaveChangesAsync` are each called exactly once |
| Integration (WebApplicationFactory) | `PatientsApiTests.cs`, `VitalSignsApiTests.cs`, `RoleBasedAccessTests.cs`, `BusinessRuleValidationTests.cs` | 11 tests — real HTTP requests against an in-memory-hosted API with an isolated SQLite in-memory database: happy/error paths, JWT-protected endpoints, RBAC boundaries (`403` vs `401`), async validation rules |

**Test database choice:** SQLite in-memory, not LocalDB — LocalDB is Windows-only,
and this project is intended to run in a future CI pipeline (GitHub Actions, which
defaults to Linux runners). SQLite in-memory is cross-platform, fast, and
self-cleaning (each `WebApplicationFactory` instance gets a fresh, isolated database).

**Testing priority followed risk, not ease:** the two most valuable, previously
untested areas — RBAC enforcement and async validation rules — were identified
through explicit risk analysis and closed last, rather than adding shallow tests to
already-covered code.

### Two real bugs found by these tests

1. **Accidental class-level `[Authorize]` on `AuthController`** — would have blocked
   every unauthenticated user from registering or logging in at all. Went unnoticed
   in manual Swagger testing only because a leftover valid token was always present
   from a previous session. Caught immediately by an integration test using a clean,
   unauthenticated client.
2. **Silent JSON enum-deserialization defaulting in the test client** — a bare
   `new JsonSerializerOptions()` doesn't enable case-insensitive property matching,
   so `System.Text.Json` silently left `RiskLevel` at its default (`Normal`) instead
   of throwing, even though the API had returned `"Critical"` correctly. Diagnosed by
   logging the raw response body directly rather than assuming the API was wrong.
   Fixed with `new JsonSerializerOptions(JsonSerializerDefaults.Web)`.

## 14. How to Run

### Prerequisites
- .NET SDK 10
- SQL Server LocalDB (development) — SQLite is used automatically for tests, no setup needed

### Setup
```bash
cd Cardiac-Monitoring-System/src/CardiacMonitoring.Api
dotnet restore
dotnet ef database update
dotnet run
```

Open the Swagger URL printed in the terminal (e.g. `http://localhost:5286/swagger`).

### Configuration
Connection string and JWT settings live in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CardiacMonitoringDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Issuer": "CardiacMonitoringApi",
    "Audience": "CardiacMonitoringApiUsers",
    "Key": "..."
  }
}
```

### Authentication flow (via Swagger)
1. `POST /api/v1/Auth/register` — `{ "email": "...", "password": "..." }`
2. `POST /api/v1/Auth/assign-role?email=...&role=Nurse` (or `Doctor`)
3. `POST /api/v1/Auth/login` — copy the returned `token`
4. Click **Authorize** (top right) → `Bearer <token>`

### Running tests
```bash
cd Cardiac-Monitoring-System
dotnet test
```

## 15. Demo Checklist (5–10 minutes)

1. Show Swagger — walk through the resource list and the `/critical` endpoint.
2. Register → assign role → login → show the JWT and its role claim.
3. Create a Patient, then a Critical vital sign for them → show `RiskLevel` in the
   response.
4. `GET /VitalSigns/critical` → show it returns only that patient.
5. Attempt a Doctor-only action (create Medication) as a Nurse → `403`.
6. Same action as a Doctor → succeeds.
7. Trigger a validation failure (invalid `PatientId`) → `400` with a clear message.
8. Hit `GET /Diagnostics/trigger-error` → show the safe `ProblemDetails` response.
9. Run `dotnet test` live → 24/24 passing.

## 16. What's Deliberately Out of Scope

- Deployment / CI-CD pipeline (Week 9 material)
- Refresh tokens (explicitly an optional stretch task even in the training material)
- Advanced caching or performance work (later Phase 3 sprint)

---

*This project applies, in order: Week 1 (C#/OOP/collections/LINQ/async), Week 2
(generics, routing, middleware, DI), Week 3 (REST design, EF Core, SQL Server,
CRUD), Week 4 (Identity, JWT, RBAC, FluentValidation, hardening), and Week 5
(xUnit, Moq, WebApplicationFactory integration testing, centralized error handling).*
