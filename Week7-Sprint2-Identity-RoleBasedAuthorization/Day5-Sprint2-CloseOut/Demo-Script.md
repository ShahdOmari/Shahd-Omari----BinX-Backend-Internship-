# Sprint 2 Demo Script — Authentication, RBAC & Audit Trail

> Estimated time: 10–12 minutes  
> Required: Postman open, `dotnet run` running, terminal visible for audit logs

---

## Setup (before the demo)
Ensure the server is running:
```powershell
cd Cardiac-Monitoring-System\src\CardiacMonitoring.Api
dotnet run
```
Have Postman open with the Cardiac Monitoring System collection loaded and the `Cardiac-Local` environment active.

---

## Scene 1 — Registration creates two linked records (2 min)

**Narrate:** "When a new staff member joins the system, a single POST creates both their login account and their staff profile in one atomic database transaction — if either fails, neither is saved."

**Action:** POST `/api/v1/Auth/register`
```json
{
  "email": "demo.nurse@cardiac.com",
  "password": "DemoPass123!",
  "department": "Cardiology"
}
```
**Point out in the response:**
- `userId` — the Identity account
- `staffProfileId` — the linked domain record
- `role: "Nurse"` — automatically assigned, never self-selected

---

## Scene 2 — Login issues a JWT with domain claims (2 min)

**Narrate:** "Login returns a signed JWT. Let's decode it live."

**Action:** POST `/api/v1/Auth/login` with same credentials → copy token → open jwt.io

**Point out in the decoded payload:**
- `sub` — the user's Identity ID
- `staffProfileId: "X"` — embedded at login time so every subsequent request can identify the staff member without an extra database query
- `http://schemas.microsoft.com/ws/2008/06/identity/claims/role: "Nurse"` — this is what `[Authorize(Roles = "Nurse")]` checks at runtime

---

## Scene 3 — Deliberate Rejection Case 1: Role boundary (2 min)

**Narrate:** "A Nurse is fully authenticated — her token is valid and signed — but she's deliberately blocked from clinical decision endpoints. Watch the difference between 401 and 403."

**Action 1:** GET `/api/v1/VitalSigns` with **no token**
- **Expected:** `401 Unauthorized` — identity unknown

**Action 2:** GET `/api/v1/VitalSigns` with **Nurse token**
- **Expected:** `403 Forbidden` — identity known, role insufficient

**Key point:** "401 means 'who are you?', 403 means 'I know who you are, and the answer is no.' Confusing these two is one of the most common subtle authorization bugs in real APIs."

---

## Scene 4 — Deliberate Rejection Case 2: Ownership check (2 min)

**Narrate:** "RBAC alone isn't enough. Two nurses have equal roles — but they shouldn't be able to read each other's recorded data."

**Setup:** Show a VitalSign recorded by Nurse A (ID known from the POST response earlier).

**Action:** GET `/api/v1/VitalSigns/{id}` with **Nurse B's token**
- **Expected:** `403 Forbidden`

**Action:** Same request with **Nurse A's token**
- **Expected:** `200 OK`

**Action:** Same request with **Doctor token**
- **Expected:** `200 OK` — Doctors have cross-patient visibility by design

**Key point:** "Ownership is enforced via the `staffProfileId` claim in the token — no extra database lookup needed per request."

---

## Scene 5 — Audit trail in the terminal (1 min)

**Narrate:** "Every request is logged with who made it, what they asked for, what we returned, and how long it took — including the rejection cases you just saw."

**Action:** Show the terminal. Point to lines like:
AUDIT | GET /api/v1/VitalSigns | status=403 | user=<id> | staff=2 | role=Nurse | 12ms
AUDIT | GET /api/v1/VitalSigns/3 | status=403 | user=<id> | staff=2 | role=Nurse | 8ms
AUDIT | GET /api/v1/VitalSigns/3 | status=200 | user=<id> | staff=1 | role=Nurse | 6ms

**Key point:** "In a real healthcare system, this audit trail is a compliance requirement. Our middleware catches every request — including the 401s and 429s that never reach a controller — because it sits before the routing layer."

---

## Scene 6 — Admin-only role assignment (1 min)

**Narrate:** "Promoting a staff member to Doctor requires an Admin token. Any other token gets rejected — closing what was previously an open endpoint anyone could call."

**Action:** POST `/api/v1/Auth/assign-role?email=demo.nurse@cardiac.com&role=Doctor` with **Nurse token**
- **Expected:** `403 Forbidden`

**Action:** Same request with **Admin token**
- **Expected:** `200 OK`

---

## Closing (30 sec)
"What we've built is a full, layered security model:
1. **Authentication** — who are you? (JWT)
2. **Authorization** — what are you allowed to do? (RBAC)
3. **Ownership** — are you allowed to access *this specific resource*? (claim-based)
4. **Audit** — what did you actually do? (middleware)"
