# BinX Tech — .NET Backend Internship

![CI](https://github.com/ShahdOmari/Shahd-Omari----BinX-Backend-Internship-/actions/workflows/ci.yml/badge.svg)

**Intern:** Shahd Omari | **Track:** .NET Backend Development | **Duration:** 10 weeks · 400 hours

A full-stack backend internship program by BinX Tech (Nablus, Palestine), structured across four phases
and culminating in a production-grade capstone project — a Cardiac Patient Monitoring System built with
ASP.NET Core 10, deployed live, and backed by a CI/CD pipeline.

---

## Capstone Project — Cardiac Patient Monitoring System

A real-world REST API for monitoring cardiac patients in a hospital setting:
vital signs, medications, appointments, and role-based staff access.

**Live API:** deployed on Railway → `GET /health` returns `{ status: "healthy" }`

**Tech stack:**

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 10 |
| ORM | Entity Framework Core 10 |
| Database | SQL Server (dev) · PostgreSQL (production) |
| Caching | Redis · StackExchange.Redis · IDistributedCache |
| Auth | ASP.NET Core Identity · JWT Bearer |
| Validation | FluentValidation |
| Docs | Swashbuckle / Swagger UI (OAS 3.0) |
| Testing | xUnit · WebApplicationFactory · SQLite in-memory |
| CI/CD | GitHub Actions · Railway |

**Key achievements by sprint:**

| Sprint | Week | Highlight |
|---|---|---|
| Sprint 1 | Week 6 | Core CRUD API, EF Core, pagination, FluentValidation |
| Sprint 2 | Week 7 | ASP.NET Core Identity, JWT, RBAC across all endpoints, AuditLoggingMiddleware |
| Sprint 3 | Week 8 | Query optimization (300 rows → 7), Redis cache (2651ms → 3ms), composite indexes |
| Sprint 4 | Week 9 | 34/34 tests, Swagger docs, GitHub Actions CI, Railway deployment |

**Code:** [Cardiac-Monitoring-System/](Cardiac-Monitoring-System/)

---

## Program Structure

10 weeks · 4 phases · feature-branch workflow throughout
Phase 1 — Foundations (Weeks 1–2) C#, OOP, LINQ, ASP.NET Core basics
Phase 2 — Core Backend (Weeks 3–5) EF Core, REST design, validation, testing
Phase 3 — Capstone Project (Weeks 6–9) 4 sprints building the cardiac monitoring API
Phase 4 — Final Presentation (Week 10) Demo, retrospective, portfolio review

---

## Weeks

### Phase 1 — Foundations

#### Week 1 — C# Fundamentals & Git
**40 hours · Phase 1 · Foundations**

| Day | Topic |
|---|---|
| [Day 1](Week1-Onboarding-Csharp-Git/Day1-EnvironmentSetup/README.md) | Environment Setup & .NET CLI |
| [Day 2](Week1-Onboarding-Csharp-Git/Day2-TypesControlFlow/README.md) | Types, Variables & Control Flow |
| [Day 3](Week1-Onboarding-Csharp-Git/Day3-OOP/README.md) | Object-Oriented Programming |
| [Day 4](Week1-Onboarding-Csharp-Git/Day4-CollectionsLinq/README.md) | Collections & LINQ |
| [Day 5](Week1-Onboarding-Csharp-Git/Day5-GitGithub/README.md) | Git & GitHub Workflow |

[Week 1 Summary](Week1-Onboarding-Csharp-Git/README.md) · [PR #1](https://github.com/ShahdOmari/Shahd-Omari----BinX-Backend-Internship-/pull/1)

---

#### Week 2 — Advanced C# & ASP.NET Core
**40 hours · Phase 1→2 · ASP.NET Core Foundations**

| Day | Topic |
|---|---|
| [Day 1](Week2-AdvancedCsharp-AspNetCore/Day1-Generics/README.md) | Generics & Advanced Collections |
| [Day 2](Week2-AdvancedCsharp-AspNetCore/Day2-AdvancedLinq/README.md) | Advanced LINQ & Deferred Execution |
| [Day 3](Week2-AdvancedCsharp-AspNetCore/Day3-AsyncConcurrency/README.md) | Async/Await & Concurrency |
| [Day 4](Week2-AdvancedCsharp-AspNetCore/Day4-AspNetCoreSetup/README.md) | ASP.NET Core Setup & Routing |
| [Day 5](Week2-AdvancedCsharp-AspNetCore/Day5-MiddlewareDI/README.md) | Middleware & Dependency Injection |

[Week 2 Summary](Week2-AdvancedCsharp-AspNetCore/README.md)

---

### Phase 3 — Capstone Project

#### Week 6 — Sprint 1: Core API
**40 hours · Sprint 1 · CRUD, EF Core, Validation**

Built the API foundation: 5 controllers, EF Core with SQL Server, FluentValidation,
global exception handling, pagination, and the cardiac risk evaluation engine.

[Sprint 1 Documentation](Cardiac-Monitoring-System/Week6-Sprint1-CoreApi/) · [PR](https://github.com/ShahdOmari/Shahd-Omari----BinX-Backend-Internship-/pulls)

---

#### Week 7 — Sprint 2: Identity & RBAC
**40 hours · Sprint 2 · Authentication & Authorization**

Wired ASP.NET Core Identity, issued JWTs with domain-specific claims (staffProfileId),
enforced role-based access across every endpoint (Nurse/Doctor/Auditor/Admin),
added ownership checks on VitalSigns, and built AuditLoggingMiddleware.

[Sprint 2 Documentation](Week7-Sprint2-Identity-RoleBasedAuthorization/)

---

#### Week 8 — Sprint 3: Performance & Caching
**40 hours · Sprint 3 · Query Optimization · Redis · Indexes**

| Change | Before | After |
|---|---|---|
| GetCriticalPatients rows | 300 (full table load) | 7 (correlated subquery) |
| GET /Patients cache hit | ~45ms | 3ms |
| VitalSigns/critical warm | table scan | index seek · 50ms |

Three composite indexes added. Redis cache-aside with explicit invalidation.
EF Core query logging confirmed every fix with real SQL output.

[Sprint 3 Documentation](Week8-Sprint3-Performance-Caching/)

---

#### Week 9 — Sprint 4: Testing, Documentation & Deployment
**40 hours · Sprint 4 · Quality & Delivery**

- **34/34 tests passing** — 8 new tests closed coverage gaps (Auth errors, RBAC, ownership)
- **Swagger enriched** — XML doc comments, example requests, response codes on all endpoints
- **GitHub Actions CI** — build + test on every push; red-green demo confirmed
- **Railway deployment** — live API with health check, production secrets via Railway Variables

[Sprint 4 Documentation](Week9-Sprint4-Testing-Documentation/)

---

## CI/CD Pipeline
BinX-Backend-Internship/
├── .github/workflows/ci.yml ← GitHub Actions CI/CD
├── Dockerfile ← multi-stage build for Railway
├── railway.json ← Railway deployment config
├── README.md ← this file
│
├── Week1-Onboarding-Csharp-Git/
├── Week2-AdvancedCsharp-AspNetCore/
│
├── Week7-Sprint2-Identity-RoleBasedAuthorization/
├── Week8-Sprint3-Performance-Caching/
├── Week9-Sprint4-Testing-Documentation/
│
└── Cardiac-Monitoring-System/ ← capstone project (all runnable code)
├── src/CardiacMonitoring.Api/
└── tests/CardiacMonitoring.Tests/

---

## Git Workflow

```bash
git checkout -b feature/weekX-dayY-topic
git commit -m "descriptive message"
git push -u origin feature/weekX-dayY-topic
# open Pull Request → review → merge
```

Every sprint lives on its own branch and merges to main via PR.
The CI pipeline runs on every PR and every push to main.
