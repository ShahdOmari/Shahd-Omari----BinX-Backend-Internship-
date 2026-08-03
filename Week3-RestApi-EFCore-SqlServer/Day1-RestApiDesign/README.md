# Day 1 — REST API Design Principles & Resource Modeling

**8 hours**

## Learning Objectives

- Explain what makes an API genuinely RESTful, not just "JSON over
  HTTP"
- Apply consistent resource naming conventions across an API
- Use HTTP status codes correctly and consistently

## What I Did

- Designed the REST resource map for the Task Tracker domain (the
  `TaskItem` and `Project` model from Week 1) before writing any
  implementation code
- Mapped the full CRUD endpoint set for the primary resource
  (`tasks`): list, get one, create, update, delete — with the correct
  HTTP verb and status code for each success and error case
- Added a nested resource endpoint (`/api/v1/projects/{id}/tasks`)
  reflecting the real ownership relationship between `Project` and
  `TaskItem`
- Decided and documented a versioning convention (`/api/v1/...`) for
  the project

## What I Learned

Writing out the error case for every single endpoint up front — not
just the happy path — made it obvious where a `404` or `400` actually
belongs, instead of defaulting to `200` for everything. Deciding on
the nested endpoint also clarified when nesting a resource under
another makes sense versus using a flat resource with a query filter:
since a task's context here is tied to its project, nesting felt like
the more honest representation of that relationship in the URL
itself.

## Deliverable

[`API-Design-Document.md`](API-Design-Document.md) — full REST
resource map, status code reference, and versioning decision for the
Task Tracker API.