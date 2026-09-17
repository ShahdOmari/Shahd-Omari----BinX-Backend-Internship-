# Day 4 — Deployment & Full CI/CD Pipeline

## Sprint Context
Sprint 4, Week 9, Day 4.

## What Was Built

### Railway Deployment
Platform: Railway (railway.app)
Project: loyal-transformation
Status: Deployment successful ✅

### Dockerfile (multi-stage)
- Stage 1 (build): `mcr.microsoft.com/dotnet/sdk:10.0` — restore + publish Release
- Stage 2 (runtime): `mcr.microsoft.com/dotnet/aspnet:10.0` — minimal runtime image
- Reads `PORT` environment variable injected automatically by Railway
- Sets `ASPNETCORE_ENVIRONMENT=Production`

### railway.json
Tells Railway to use the Dockerfile at the repo root and check `/health`
after deployment. Encoding issue (invalid char) fixed by writing ASCII explicitly.

### Production Secrets (via Railway Variables UI)
Never in source code — all injected at runtime:

| Variable | Purpose |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` — disables Swagger, enables production logging |
| `Jwt__Key` | JWT signing secret (min 32 chars) |
| `Jwt__Issuer` | `CardiacMonitoringApi` |
| `Jwt__Audience` | `CardiacMonitoringApiUsers` |
| `DATABASE_URL` | PostgreSQL — injected automatically by Railway add-on |

### Full CI/CD Pipeline
Two-job workflow in `.github/workflows/ci.yml`:
Push to main
↓
[build-and-test] — restore → build → 34/34 tests
↓ only if tests pass + only on main
[deploy] — railway up → live public URL

The deploy job has two guards:
- `needs: build-and-test` — deploy never runs if any test fails
- `if: github.ref == refs/heads/main` — PRs never trigger deploy

### Health Check Endpoint
`GET /health` — returns `{ status: "healthy", timestamp: "..." }`
Used by Railway to confirm the container started successfully.

## Key Takeaway
Broken code never reaches production — the CI gate enforces it automatically.
Every push to main goes through: build → test (34/34) → deploy.
No manual steps, no exceptions.

## Test Suite: 34/34 passing
