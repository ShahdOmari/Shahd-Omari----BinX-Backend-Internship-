# CV Bullet & LinkedIn — Cardiac Patient Monitoring System

---

## CV Bullet (Technical Role)

**Cardiac Patient Monitoring System** | ASP.NET Core 10 · EF Core · SQL Server · Redis · JWT
- Designed and built a production REST API with role-based access control
  (Nurse / Doctor / Auditor / Admin), JWT authentication, and per-resource
  ownership enforcement, deployed live via Railway with a GitHub Actions CI/CD pipeline
- Reduced the critical-patients endpoint from a full table scan (300 rows)
  to a correlated SQL subquery (7 rows) — 97% fewer rows transferred per request
- Implemented Redis cache-aside on the patient catalog endpoint:
  response time from 2651ms (cold) to 3ms (cached hit), 98% improvement
- Achieved 34/34 passing tests (unit + integration via WebApplicationFactory)
  with SQLite in-memory, enabling CI to run with zero external dependencies

---

## CV Bullet (Shorter Version — one line)

Built and deployed a .NET 10 cardiac monitoring REST API with JWT/RBAC,
Redis caching (2651ms → 3ms), query optimization (300 rows → 7),
and a GitHub Actions CI/CD pipeline — 34/34 tests passing.

---

## LinkedIn Post

---

Just wrapped up a 10-week .NET Backend Internship at BinX Tech 🇵🇸

The capstone project: a **Cardiac Patient Monitoring System** — a production
REST API for managing vital signs, medications, and appointments across
a hospital's clinical staff.

Some things I built and learned:

🔐 **Security in layers** — rate limiting → JWT auth → role-based access
(Nurse / Doctor / Auditor / Admin) → per-resource ownership checks.
Every layer has a different job; mixing them up is how bugs happen.

⚡ **Performance with evidence** — enabled EF Core query logging and
discovered the critical-patients endpoint was doing `SELECT *` on 300 rows
and filtering in C# memory. Fixed with a correlated SQL subquery:
300 rows → 7, confirmed by reading the actual generated SQL.

Redis cache-aside on the patient catalog:
**2651ms → 3ms** on cache hits (98% improvement).

✅ **34 tests, 34 passing** — integration tests via WebApplicationFactory
with SQLite in-memory, so CI runs with zero external infrastructure.

🚀 **Full CI/CD** — GitHub Actions runs build + test on every push;
Railway deploys automatically when tests pass on main.

The repository and live API are linked below.
Happy to answer questions about any of the technical decisions.

#dotnet #aspnetcore #backend #csharp #Palestine #BinXTech

---

## LinkedIn Profile — About Section Update

.NET Backend Developer with hands-on experience building production REST APIs.

Recent project: a Cardiac Patient Monitoring System built with ASP.NET Core 10,
Entity Framework Core, Redis caching, and JWT authentication — deployed live
with a GitHub Actions CI/CD pipeline and 34 automated tests.

Focused on: API design, performance optimization, test coverage, and clean
engineering practices.

Based in Nablus, Palestine 🇵🇸
