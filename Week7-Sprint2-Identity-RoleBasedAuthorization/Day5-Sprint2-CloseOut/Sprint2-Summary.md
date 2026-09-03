# Sprint 2 Summary — Ready for Mentor Check-In

## Sprint Goal (achieved ✅)
Wire ASP.NET Core Identity into the Cardiac Monitoring System, issue JWTs with domain-specific claims, enforce real role-based access control across every endpoint, and add a healthcare-grade audit trail.

---

## Registration & Login Flow
Client
│
├─ POST /api/v1/Auth/register { email, password, department }
│ │
│ ├─ BEGIN TRANSACTION
│ ├─ UserManager.CreateAsync → AspNetUsers row
│ ├─ StaffProfiles row (linked by ApplicationUserId)
│ ├─ RoleManager → assign "Nurse" role
│ └─ COMMIT → { userId, staffProfileId, role: "Nurse" }
│
└─ POST /api/v1/Auth/login { email, password }
│
├─ CheckPasswordSignInAsync
├─ Build JWT:
│ ├─ sub = user.Id
│ ├─ email = user.Email
│ ├─ staffProfileId = StaffProfiles.Id ← domain claim
│ └─ role = "Nurse" (or Doctor/Admin etc.)
└─ { token: "eyJ..." } expires in 60 minutes

---

## RBAC Matrix

| Endpoint | Public | Nurse | Doctor | Auditor | Admin |
|---|---|---|---|---|---|
| POST /auth/register | ✅ | ✅ | ✅ | ✅ | ✅ |
| POST /auth/login | ✅ | ✅ | ✅ | ✅ | ✅ |
| POST /auth/assign-role | ❌ | ❌ | ❌ | ❌ | ✅ |
| GET /patients | ❌ | ✅ | ✅ | ✅ | ✅ |
| POST/PUT /patients | ❌ | ✅ | ✅ | ❌ | ✅ |
| DELETE /patients | ❌ | ❌ | ✅ | ❌ | ✅ |
| GET /vitalsigns (all) | ❌ | ❌ | ✅ | ✅ | ✅ |
| GET /vitalsigns/{id} | ❌ | own only | ✅ | ✅ | ✅ |
| POST /vitalsigns | ❌ | ✅ | ✅ | ❌ | ✅ |
| GET /vitalsigns/critical | ❌ | ❌ | ✅ | ❌ | ✅ |
| GET /medications | ❌ | ✅ | ✅ | ✅ | ✅ |
| POST/PUT/DELETE /medications | ❌ | ❌ | ✅ | ❌ | ✅ |
| GET/POST/PUT /appointments | ❌ | ✅ | ✅ | ✅ | ✅ |
| DELETE /appointments | ❌ | ❌ | ✅ | ❌ | ✅ |

**Legend:** ✅ = allowed | ❌ = 403 Forbidden | "own only" = 200 if recorded by this staff member, 403 otherwise

---

## Ownership Model
`VitalSign.RecordedByStaffProfileId` (FK, nullable) is set from the `staffProfileId` JWT claim at creation time. `GET /vitalsigns/{id}` checks this field against the current caller's claim — no extra DB query needed.

---

## AuditLoggingMiddleware
Every request logged:
AUDIT | {Method} {Path} | status={Status} | user={UserId} | staff={StaffProfileId} | role={Role} | {ElapsedMs}ms
Placed after `UseAuthentication()` / `UseAuthorization()` so claims and status codes are fully resolved before logging.

---

## Test Suite
| Suite | Count | Status |
|---|---|---|
| Unit (CardiacRiskEvaluator) | 10 | ✅ |
| Unit (VitalSignService) | 4 | ✅ |
| Integration (Patients) | 3 | ✅ |
| Integration (VitalSigns) | 4 | ✅ |
| Integration (RoleBasedAccess) | 3 | ✅ |
| Integration (BusinessRuleValidation) | 2 | ✅ |
| **Total** | **26** | **26/26 ✅** |

---

## Pull Request
Branch: `week7/day2-auth`
PR: [link to your PR on GitHub — update this after merging]

---

## Sprint 3 Preview
5 tagged edge cases moved to Sprint 3 backlog (see `Sprint2-DoD-Audit.md`). Top priority: role parameter validation on `assign-role` endpoint.
