# Week 3 — REST APIs, Entity Framework Core & SQL Server

**Phase 2 · 40 hours · 5 training days · REST APIs & Entity Framework Core**

## Goal of the Week

Move from hardcoded in-memory data to a real relational database.
Design REST endpoints following proper conventions, model a
normalized SQL Server schema, wire it up through Entity Framework
Core with code-first migrations, and implement full CRUD operations —
tested end-to-end in Postman, including error paths, not just the
happy path.

## What I Aimed to Learn

- REST conventions: resource naming, HTTP verbs, and correct status
  codes
- How to design a normalized relational schema through third normal
  form
- Setting up Entity Framework Core with SQL Server and running
  code-first migrations
- Implementing full CRUD operations backed by EF Core, using async
  queries throughout
- Handling not-found and validation error cases with correct HTTP
  responses
- Building a complete Postman collection testing both success and
  error paths

## Day-by-Day

| Day | Topic | Status |
|---|---|---|:
| [Day 1](Day1-RestApiDesign/README.md) | REST API Design & Resource Modeling | ✅ Complete |
| [Day 2](Day2-SchemaDesign/README.md) | SQL Server Schema Design & Normalization | ✅ Complete |
| [Day 3](Day3-EfCoreMigrations/README.md) | EF Core Setup & Code-First Migrations | ✅ Complete 
| [Day 4](Day4-CrudOperations/README.md) | Implementing CRUD Operations with EF Core | ✅ Complete |
| [Day 5](Day5-PostmanTesting/README.md) | Testing & Documenting the API with Postman | ⏳ Pending |

## Week 3 Deliverables

- A REST resource design document with endpoint list, status codes,
  and versioning convention
- A normalized (3NF) database schema diagrammed as an ERD
- An EF Core project with entity classes, a DbContext, and applied
  code-first migrations
- Full CRUD operations for the primary resource, backed by a real SQL
  Server database
- A complete Postman collection with happy-path and error-path tests,
  using environments and variables

## Week 3 Outcome

*To be completed once Day 5 is finished.*