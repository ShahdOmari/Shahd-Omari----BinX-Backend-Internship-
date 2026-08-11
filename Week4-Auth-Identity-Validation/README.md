# Week 4 — Authentication, Identity & Input Validation

**Phase 2 · 40 hours · 5 training days · Authentication & Security**

## Goal of the Week

Lock down the API built in Week 3 behind real authentication, and
make sure it rejects bad input before it ever reaches the database.
Integrate ASP.NET Core Identity for user management and password
hashing, implement JWT-based login and route protection, apply
role-based access control, validate every request rigorously with
FluentValidation, and close the week hardening the API with rate
limiting, CORS, and security headers.

## What I Aimed to Learn

- What ASP.NET Core Identity provides out of the box, and how to set
  it up with EF Core
- JWT structure, issuing a token on login, and configuring JWT bearer
  authentication middleware
- Protecting routes with `[Authorize]` and role-based access control
- Writing FluentValidation validators expressing real business rules
- Configuring rate limiting, CORS, and security headers to harden an
  API for production

## Day-by-Day

| Day | Topic | Status |
|---|---|---|
| [Day 1](Day1-Identity-Registration/README.md) | ASP.NET Core Identity & User Registration | ✅ Complete |
| [Day 2](Day2-JWT-Auth/README.md) | JWT Authentication & Token Issuance | ⏳ Pending |
| [Day 3](Day3-Authorization-Roles/README.md) | Protecting Routes & Role-Based Access Control | ⏳ Pending |
| [Day 4](Day4-FluentValidation/README.md) | Input Validation with FluentValidation | ⏳ Pending |
| [Day 5](Day5-Hardening/README.md) | Rate Limiting, CORS & Security Headers | ⏳ Pending |

Each day's folder has its own README with the specific exercises,
code decisions, and what I learned that day.

## Postman Documentation Standard

Starting this week, Postman collections follow a higher documentation
bar in response to Week 3 mentor feedback ("the Postman testing
should be clearer"): collection-level and per-request documentation,
descriptive numbered request names with expected outcomes, and saved
response examples for both success and error cases — not just
functional requests with no explanation.

## Week 4 Deliverables

- A working registration and login flow backed by ASP.NET Core
  Identity
- JWT-based authentication with configured bearer middleware and
  token expiry
- Protected routes with role-based access control across at least
  two roles
- FluentValidation validators covering all Create and Update
  endpoints, returning structured errors
- Rate limiting, a named CORS policy, and security headers configured
  on the API

## Week 4 Outcome

*To be completed once Day 5 is finished.*