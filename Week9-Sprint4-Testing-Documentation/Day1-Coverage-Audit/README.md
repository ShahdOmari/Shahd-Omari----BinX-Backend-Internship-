# Day 1 — Sprint 4 Planning & Test Coverage Audit

## Sprint 4 Goal
Deliver a fully tested, documented capstone API — every critical route
covered, Swagger enriched with meaningful comments, and the N+1 regression
test from Sprint 3's retrospective committed.

## Sprint 4 Backlog

| # | Task | Priority | Source |
|---|---|---|---|
| 1 | Auth error paths (wrong password, duplicate email) | High | Coverage audit |
| 2 | VitalSigns/critical RBAC (Nurse=403, Admin=200) | High | Sprint 3 retrospective |
| 3 | DELETE /Patients RBAC (Nurse=403) | High | Coverage audit |
| 4 | POST /VitalSigns happy path | High | Coverage audit |
| 5 | Ownership check — Nurse A cannot read Nurse B reading | High | Coverage audit |
| 6 | Swagger/OpenAPI documentation | Medium | Sprint 4 curriculum |
| 7 | PUT /Patients happy path | Medium | Coverage audit |
| 8 | GET /Medications + GET /Appointments | Low | Coverage audit |

## Coverage Audit Before Day 1

| Endpoint | Happy | Error | RBAC | Priority |
|---|---|---|---|---|
| POST /Auth/login (wrong password) | — | ❌ | — | HIGH |
| POST /Auth/register (duplicate) | — | ❌ | — | HIGH |
| GET /VitalSigns/critical | ❌ | ❌ | ❌ | HIGH |
| POST /VitalSigns | ❌ | ❌ | ❌ | HIGH |
| GET /VitalSigns/{id} ownership | ❌ | ❌ | ❌ | HIGH |
| DELETE /Patients | ❌ | ❌ | partial | HIGH |

## Tests Added (8 new tests → total 34/34)

| Test | What it covers |
|---|---|
| Login_ReturnsUnauthorized_WhenPasswordIsWrong | Auth error path |
| Register_ReturnsBadRequest_WhenEmailAlreadyExists | Duplicate registration |
| GetCriticalPatients_ReturnsForbidden_ForNurseRole | RBAC gap |
| GetCriticalPatients_ReturnsSuccess_ForAdminRole | Happy path |
| DeletePatient_ReturnsForbidden_ForNurseRole | RBAC gap |
| CreateVitalSign_ReturnsCreated_WithValidData | Core workflow |
| GetVitalSignById_ReturnsForbidden_ForDifferentNurse | Ownership check |
| GetVitalSignById_ReturnsSuccess_ForRecordingNurse | Ownership happy path |

## Technical Note
Tests deserializing VitalSignResponse failed because the API serializes
RiskLevel as a string ("Normal"/"Watch"/"Critical") via JsonStringEnumConverter,
but test project uses default deserializer options. Fix: read the ID via
JsonDocument.Parse() instead of full deserialization.

## Test Suite: 34/34 passing
