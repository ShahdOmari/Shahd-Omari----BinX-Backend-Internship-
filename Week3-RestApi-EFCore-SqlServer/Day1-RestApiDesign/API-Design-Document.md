# Task Tracker API — REST Design Document

**BinX Backend Internship — Week 3, Day 1: REST API Design Principles & Resource Modeling**
**Author:** Shahd Omari

---

## Overview

This document maps out the REST resource design for the Task Tracker
domain used throughout this internship (the `TaskItem` and `Project`
model from Week 1, Day 3). The goal of this exercise is to design the
API surface on paper first — endpoints, status codes, and naming
conventions — before writing any implementation code, which happens
starting Week 3 Day 3 with Entity Framework Core.

---

## 1. Core Resources

Following REST convention, resources are named as **plural nouns**,
not verbs:

- `tasks` — individual task items
- `projects` — groups of related tasks

---

## 2. Full Endpoint Map — Primary Resource: Tasks

The table below covers the complete set of operations for the primary
resource (`tasks`): list, get one, create, update, and delete.

| Method | Endpoint | Description | Success Code | Error Case & Code |
|---|---|---|---|---|
| GET | `/api/v1/tasks` | List all tasks | `200 OK` | — (empty list still returns `200 OK` with `[]`, not an error) |
| GET | `/api/v1/tasks/{id}` | Get a single task by ID | `200 OK` | `404 Not Found` if the ID doesn't exist |
| POST | `/api/v1/tasks` | Create a new task | `201 Created` (with a `Location` header pointing to the new resource) | `400 Bad Request` if required fields are missing or invalid (e.g. empty title, priority outside 1–5) |
| PUT | `/api/v1/tasks/{id}` | Replace an existing task entirely | `200 OK` | `404 Not Found` if the ID doesn't exist; `400 Bad Request` if the replacement data is invalid |
| DELETE | `/api/v1/tasks/{id}` | Delete a task | `204 No Content` | `404 Not Found` if the ID doesn't exist |

**Design note:** GET on an empty list is intentionally *not* treated
as an error case — an empty result set is still a successful request,
just with no data. Only a missing *specific* resource (by ID) counts
as `404`.

---

## 3. Nested Resource Endpoint  

`GET /api/v1/projects/{id}/tasks`

**Description:** Returns all tasks belonging to a specific project.

This reflects a real ownership relationship already established in
the Week 1 domain model — a `Project` holds a collection of `TaskItem`
objects, so nesting the URL under `/projects/{id}/tasks` mirrors that
relationship directly in the API surface, instead of using a flat
query parameter like `/api/v1/tasks?projectId={id}`.

| Success Code | Error Case & Code |
|---|---|
| `200 OK` | `404 Not Found` if the project ID itself doesn't exist |

---

## 4. Versioning Convention

**Decision:** URL segment versioning — `/api/v1/...`

**Reasoning:** URL-based versioning is immediately visible in the
request itself, without requiring a tool like Postman to inspect
request headers. It's simpler to document, easier to test manually,
and removes any ambiguity about which API version a given request is
targeting — an important property for an API still early in
development, where breaking changes are more likely.

The alternative (a custom header like `Api-Version: 1`) was
considered but rejected for this project, since it adds an extra step
to every manual test and isn't visible just from looking at the URL.

---

## 5. Status Code Reference (Summary)

| Code | Meaning | Used For |
|---|---|---|
| `200 OK` | Success | Successful `GET` or `PUT` |
| `201 Created` | Resource created | Successful `POST` |
| `204 No Content` | Success, no response body | Successful `DELETE` |
| `400 Bad Request` | Invalid input | Missing or invalid fields on `POST`/`PUT` |
| `404 Not Found` | Resource does not exist | Invalid ID on `GET`/`PUT`/`DELETE` |

---

## Design Notes — What I Learned

Designing the endpoint map before writing any code made a few things
concrete that felt abstract in the lesson:

- **Returning `200` for everything, including errors, is a real
  temptation** when you're focused on "does the data come back
  correctly" rather than "does the *status code itself* communicate
  the outcome." Writing out the error case for every single endpoint
  up front — not just the happy path — made it obvious where a 404 or
  400 actually belongs.
- **The nested endpoint decision** clarified when nesting makes sense
  versus a flat resource with a filter. Since `Project → Tasks` is a
  genuine ownership relationship (a task doesn't exist independently
  of belonging to a project's context in this domain), nesting the
  URL felt like the more honest representation, rather than treating
  it as just another query filter.
- **Deciding on versioning now**, even though this is a small
  internal project, matches the lesson's point directly: retrofitting
  versioning after real clients depend on an API is far more
  disruptive than deciding the convention on day one.