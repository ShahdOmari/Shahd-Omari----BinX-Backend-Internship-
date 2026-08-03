# Day 2 — SQL Server Schema Design & Normalization

**8 hours**

## Learning Objectives

- Explain why normalization matters and what problems it prevents
- Apply first, second, and third normal form to a schema design
- Model primary keys, foreign keys, and relationships correctly

## What I Did

- Extended the Day 1 API resources (`tasks`, `projects`) with two
  additional entities needed for a realistic schema: `Users` and
  `Tags`
- Started from a deliberately unnormalized single-table draft to
  identify concrete 1NF and 3NF violations, rather than applying the
  rules abstractly
- Normalized the schema to 3NF: separated `Users`, `Projects`,
  `Tasks`, and `Tags` into their own tables, and introduced a
  `TaskTags` join table for the many-to-many relationship between
  tasks and tags
- Defined primary keys for every table and foreign keys for every
  relationship, including a composite primary key on `TaskTags`
- Diagrammed the final schema as an ERD using dbdiagram.io
- Chose appropriate column types for every attribute, with particular
  attention to using `DECIMAL` instead of `FLOAT` for the `Budget`
  column

## What I Learned

Starting from an unnormalized table made the reasoning behind 3NF
concrete instead of abstract — seeing `ProjectBudget` sitting on a
`Tasks` row made the update anomaly obvious: if two tasks shared a
project, updating the budget would mean finding and updating every
task row referencing it. Adding `Users` and `Tags` specifically to
have a real many-to-many case to work through was also useful, since
the Day 1 resources alone didn't require a join table. The
`DECIMAL` vs `FLOAT` decision for `Budget` was the one column type
choice I made deliberately rather than by default, since floating-point
rounding errors are a real, well-documented problem for financial data.

## Deliverables

- [`Schema-Design-Document.md`](Schema-Design-Document.md) — full
  normalization walkthrough (1NF → 3NF), entity list, primary/foreign
  keys, and column type decisions
- [`schema-erd.png`](schema-erd.png) — entity-relationship diagram
  exported from dbdiagram.io