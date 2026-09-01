# Sprint 1 Demo Script — Postman

### Demoed live via Postman against the running API, per Sprint Review requirements.

---

## Part 1 — Catalog Browsing (Paginated Patients Endpoint)

| Step | Request | Expected |
|---|---|---|
| 1 | `GET /api/v1/Patients?page=1&pageSize=2` | Returns `PagedResult` shape: `items`, `page`, `pageSize`, `totalCount` |
| 2 | `GET /api/v1/Patients?gender=Female` | Only Female patients returned |
| 3 | `GET /api/v1/Patients?sortBy=name&sortDirection=desc` | Results sorted Z→A by full name |

## Part 2 — "Order Creation" Equivalent: Critical Vital Sign → Auto-Scheduled Follow-Up

This capstone has no e-commerce order flow — the equivalent real business
transaction is: **recording a Critical vital sign automatically schedules an
urgent follow-up appointment**, both writes wrapped in one database transaction.

| Step | Request | Expected |
|---|---|---|
| 1 | `POST /api/v1/VitalSigns` with Critical values (HR 145, SBP 190, SpO2 85) | `201`, `riskLevel: "Critical"` |
| 2 | `GET /api/v1/Appointments?upcomingOnly=true` | A new appointment appears: `"On-Call Cardiologist"`, reason mentions "automatically scheduled" |
| 3 | Repeat step 1 for the same patient immediately | No duplicate appointment created (24-hour de-duplication rule) |

## Part 3 — Security & Error Handling Spot-Check

| Step | Request | Expected |
|---|---|---|
| 1 | `POST /api/v1/Medications` as Nurse | `403 Forbidden` |
| 2 | `GET /api/v1/Diagnostics/trigger-error` | Safe `ProblemDetails` response, no stack trace |
| 3 | `dotnet test` run live | 24/24 passing |
