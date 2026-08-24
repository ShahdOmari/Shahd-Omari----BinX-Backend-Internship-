# Day 1 — Sprint 1 Planning & Project Database Design

### Week 6 · Phase 3, Sprint 1

---

## Learning Objectives

- Run a Sprint Planning session and translate it into a sized, realistic backlog.
- Design the capstone project's complete database schema.
- Finalize the schema as a documented ERD.

## What I Did

- Wrote a one-sentence Sprint 1 goal (see `Sprint1-Backlog.md`).
- Listed every entity the capstone needs across its full professional baseline —
  not just what was in scope originally — confirming the schema built across
  Weeks 1-4 already covers it completely.
- Finalized the ERD as a Mermaid diagram (`ERD.md`), version-controlled and
  rendered automatically by GitHub, so it can't silently fall out of sync with the
  actual schema the way an exported-once image file would.
- Confirmed the schema against 1NF/2NF/3NF normalization explicitly.
- Sized the Sprint 1 backlog into half-day-to-a-day tasks with a clear Definition
  of Done for each.

## Files in This Folder

- `Sprint1-Backlog.md` — sprint goal, full entity list, sized backlog, Definition of Done
- `ERD.md` — the finalized entity relationship diagram (Mermaid) with normalization notes

## Key Takeaway

A schema diagram that isn't kept in sync with the real database becomes actively
misleading rather than just outdated — using Mermaid directly in a Markdown file
that lives in the same repository as the code keeps it naturally versioned alongside
every schema change, instead of a diagram exported once and left to rot.
