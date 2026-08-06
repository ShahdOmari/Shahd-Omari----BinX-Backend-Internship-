# Day 5 — Testing & Documenting the API with Postman; Week 3 Synthesis

**8 hours**

## Learning Objectives

- Build a complete, organized Postman collection covering every
  endpoint
- Test both success and error paths systematically
- Use Postman environments and variables to keep a collection
  portable

## What I Did

- Copied the Day 4 CRUD API and pointed it at a separate database
  (`TaskTrackerDb_Day5`) to keep testing isolated from Day 4's data
- Built a Postman collection (`TaskTracker API - Week 3`) with a
  `Tasks` folder containing 10 requests: a happy-path and an
  error-path request for every endpoint (Create, Get All, Get By Id,
  Update, Delete)
- Created a `Local Development` environment with a `baseUrl` variable
  (`http://localhost:5264`), and updated every request to use
  `{{baseUrl}}` instead of a hardcoded URL
- Added `pm.test()` assertions to all 10 requests checking the
  expected status code (and additional checks like response shape on
  a few), turning manual verification into an automated pass/fail
  check
- Ran into the auto-increment ID assumption issue firsthand: requests
  hardcoded to `id=1` started failing with `404` once the database's
  identity counter had already moved past 1 from earlier test runs.
  Fixed it by having the Create request save the returned id as a
  collection variable (`taskId`) via `pm.collectionVariables.set()`,
  and updated the dependent requests (Get By Id, Update, Delete) to
  reference `{{taskId}}` instead of a fixed number
- Exported the finished collection to
  [`TaskTrackerApi-Postman-Collection.json`](TaskTrackerApi-Postman-Collection.json)

## Endpoint Documentation

| Method | Endpoint | Purpose | Required Fields | Possible Errors |
|---|---|---|---|---|
| GET | `/api/v1/tasks` | List all tasks | — | — |
| GET | `/api/v1/tasks/{id}` | Get a single task | — | `404` if id doesn't exist |
| POST | `/api/v1/tasks` | Create a task | `title`, `priorityLevel` (1–5), `projectId`, `assignedToUserId` | `400` if title empty or priority out of range |
| PUT | `/api/v1/tasks/{id}` | Update a task | `title`, `priorityLevel` (1–5), `isCompleted` | `404` if id doesn't exist, `400` for invalid input |
| DELETE | `/api/v1/tasks/{id}` | Delete a task | — | `404` if id doesn't exist |

## What I Learned

Hardcoding `id=1` in dependent requests seemed reasonable until the
identity counter moved on from earlier test runs — a `404` on
`GET /api/v1/tasks/1` was confusing at first since the request
"should" have worked, until checking the actual Create response
showed the real id was `2`, not `1`. Saving the created id as a
collection variable instead of assuming a fixed value fixed the
whole chain of dependent requests at once, and is a more realistic
pattern than fixed test data for anything backed by a real
auto-incrementing database. The lesson's warning about happy-path-only
collections giving false confidence also made concrete sense here —
if the collection had only tested Create and Get All, the fragile
hardcoded-id assumption in the other requests would never have
surfaced.

## Project

[`TaskTrackerApi/`](TaskTrackerApi/) — same CRUD API as Day 4, tested
end-to-end with a full Postman collection covering happy paths,
error paths, and automated status code assertions.

[`TaskTrackerApi-Postman-Collection.json`](TaskTrackerApi-Postman-Collection.json)
— exported Postman collection, importable and re-runnable.