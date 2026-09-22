# Day 3 — Full Rehearsal & Mock Q&A

## Week 10 Context
Week 10, Day 3 — final rehearsal before presentation day.

## What Was Produced

### Demo-Script.md
6 scenes, timed at 8-10 minutes:
1. Live API on Railway + CI pipeline green
2. Authentication & JWT claims (with jwt.io decode)
3. RBAC in action (Nurse → 403, Admin → 200)
4. Ownership check (Nurse B → 403 on Nurse A reading)
5. Redis cache hit/miss with timing
6. Query optimization before/after

Includes backup plan if Railway is unavailable.

### QA-Preparation.md
7 technical questions with full honest answers:
- JWT vs Session auth
- 401 vs 403 distinction
- Cache-Aside vs Write-Through
- Composite vs separate indexes
- How security coverage was verified
- Redis failure handling
- Middleware vs Action Filter

### Rehearsal-Checklist.md
Timing targets, pre-demo setup, things to say out loud,
and common mistakes to avoid.

## Key Principle
Explain decisions, not features.
"I added caching" → weak.
"Cache-aside on the catalog endpoint reduced response time from 2651ms
to 3ms — confirmed by reading the terminal logs" → strong.
