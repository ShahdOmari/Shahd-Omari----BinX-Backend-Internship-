# Day 3 — GitHub Actions CI Pipeline

## Sprint Context
Sprint 4, Week 9, Day 3.

## What Was Built

### CI Workflow: `.github/workflows/ci.yml`
Located at the repository root (not inside Cardiac-Monitoring-System/) —
GitHub Actions requires the workflow file to live at the repository root.

Triggers on:
- Every push to `main`
- Every pull request targeting `main`

**Pipeline steps:**
1. `actions/checkout@v4` — checks out code
2. `actions/setup-dotnet@v4` — installs .NET 10 SDK on ubuntu-latest
3. `dotnet restore` — restores NuGet packages
4. `dotnet build --configuration Release` — builds in Release mode
5. `dotnet test --configuration Release` — runs all 34 tests

**Why no external services needed in CI:**
Tests use SQLite in-memory (no SQL Server) and AddDistributedMemoryCache()
(no Redis). The factory setup from Day 1 made CI work out of the box.

### Status Badge
Added to README.md — shows live pipeline state on the repository page.

### Red-Green-Green Demo (lab requirement)
1. Pipeline ran green on first push ✅
2. Deliberately changed Assert.Equal(Unauthorized) to Assert.Equal(OK)
   → pipeline failed ❌ visible in Actions tab
3. Reverted → pipeline green again ✅

## Lesson Learned: Workflow File Location
First attempt placed ci.yml inside Cardiac-Monitoring-System/.github/workflows/
which GitHub ignores — the .github folder must be at the repository root,
not inside a subdirectory. Fixed by creating .github/workflows/ci.yml
at the root of the BinX-Backend-Internship repository.

## Test Suite: 34/34 passing
