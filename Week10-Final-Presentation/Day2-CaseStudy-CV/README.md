# Day 2 — Case Study & CV/LinkedIn Update

## Week 10 Context
Sprint 4, Week 10, Day 2.

## What Was Produced

### 1. Technical Case Study (`Case-Study.md`)
Covers the full project arc for a technical hiring audience:
- The Problem — why this is not a generic CRUD application
- Architecture — layering order and reasoning
- Biggest Challenge — the full table load, first fix that failed (EF Core translation
  limit), and the correlated subquery that worked
- Performance Results — real numbers from terminal logs
- Test Suite — 34/34 and why SQLite in-memory matters for CI
- CI/CD — the pipeline and the red-green demo
- Outcome — deployed state
- What I Would Build Differently — honest retrospective

### 2. CV Bullets (`CV-LinkedIn.md`)
Two versions: detailed (4 bullets with specific metrics) and
one-line (for space-constrained CVs). Every number is real —
300 rows, 7 rows, 2651ms, 3ms, 34/34.

### 3. LinkedIn Post
Draft announcing the completed capstone — names the stack,
cites one concrete outcome per technical area, links to GitHub.

### 4. LinkedIn About Section
Short update positioning as a .NET Backend Developer with
specific project evidence.

## Key Principle Applied
"Improved performance" is vague. "2651ms → 3ms via Redis cache-aside"
is a claim a technical interviewer can ask follow-up questions about.
Every bullet and case study section was written to meet that standard.
