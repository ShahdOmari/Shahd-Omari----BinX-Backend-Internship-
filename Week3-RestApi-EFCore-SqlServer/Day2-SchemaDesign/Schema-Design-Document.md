# Task Tracker — Database Schema Design Document

**BinX Backend Internship — Week 3, Day 2: SQL Server Schema Design & Normalization**
**Author:** Shahd Omari

---

## Overview

This document designs a normalized (3NF) relational schema for the
Task Tracker domain, extending the Day 1 API resources (`tasks`,
`projects`) with two additional entities needed for a realistic
schema: `Users` (who owns a project and who a task is assigned to)
and `Tags` (a many-to-many relationship with tasks).

---

## 1. Entities and Attributes

Based on the Day 1 API resources, the following entities are needed:

**Users**
- Id
- Name
- Email

**Projects**
- Id
- Name
- Budget
- OwnerId (references a User)

**Tasks**
- Id
- Title
- PriorityLevel
- IsCompleted
- ProjectId (references a Project)
- AssignedToUserId (references a User)

**Tags**
- Id
- Name

---

## 2. Applying Normalization (1NF → 3NF)

### Starting point (unnormalized, for illustration only)

A naive first draft might combine everything into a single `Tasks`
table like this:

| TaskId | Title | Tags | ProjectName | ProjectBudget | OwnerName | OwnerEmail |
|---|---|---|---|---|---|---|
| 1 | Design schema | urgent, backend | BinX API | 5000 | Sajed | sajed@binx.com |

This violates normalization at every level, and each violation maps
directly to a real problem:

**1NF violation:** the `Tags` column holds multiple comma-separated
values in a single field. Searching for "which tasks have the
'urgent' tag" would require inefficient text matching instead of a
direct query.

**3NF violation:** `ProjectName`, `ProjectBudget`, `OwnerName`, and
`OwnerEmail` all depend on the *project*, not on the *task* itself
(the primary key). If two tasks belong to the same project, this
data would be duplicated — and if the project's budget changes, every
task row referencing it would need to be updated.

### Applying 1NF — atomic values only

Splitting the comma-separated tags into individual rows requires a
separate structure entirely (handled below as a many-to-many
relationship), rather than multiple values in one column.

### Applying 3NF — separating entities by what they actually describe

Each piece of data is moved to the table it actually depends on:

- `ProjectName` and `ProjectBudget` → belong to `Projects`, not `Tasks`
- `OwnerName` and `OwnerEmail` → belong to `Users`, not `Projects`
- Tags → belong to a separate `Tags` table, linked through a join
  table (since a task can have many tags, and a tag can apply to many
  tasks — a classic many-to-many case, same reasoning as the
  Students/Courses example from the lesson)

### Final normalized structure

```
Users(Id PK, Name, Email)
Projects(Id PK, Name, Budget, OwnerId FK -> Users.Id)
Tasks(Id PK, Title, PriorityLevel, IsCompleted,
      ProjectId FK -> Projects.Id, AssignedToUserId FK -> Users.Id)
Tags(Id PK, Name)
TaskTags(TaskId FK -> Tasks.Id, TagId FK -> Tags.Id)   -- join table
```

Every non-key column now depends only on the primary key of the table
it lives in, and nothing is duplicated across rows.

---

## 3. Primary Keys and Foreign Keys

| Table | Primary Key | Foreign Keys |
|---|---|---|
| `Users` | `Id` | — |
| `Projects` | `Id` | `OwnerId` → `Users.Id` |
| `Tasks` | `Id` | `ProjectId` → `Projects.Id`, `AssignedToUserId` → `Users.Id` |
| `Tags` | `Id` | — |
| `TaskTags` | `TaskId` + `TagId` (composite) | `TaskId` → `Tasks.Id`, `TagId` → `Tags.Id` |

**Design note:** `TaskTags` uses a **composite primary key**
(`TaskId` + `TagId` together) rather than its own separate `Id`
column, since the combination of a specific task and a specific tag
is naturally unique — a task can't be linked to the same tag twice.

---

## 4. Entity-Relationship Diagram

Diagrammed using [dbdiagram.io](https://dbdiagram.io) with the
following DBML source: 

Table Users {
Id int [pk, increment]
Name varchar(100)
Email varchar(150)
}

Table Projects {
Id int [pk, increment]
Name varchar(100)
Budget decimal(10,2)
OwnerId int [ref: > Users.Id]
}

Table Tasks {
Id int [pk, increment]
Title varchar(200)
PriorityLevel int
IsCompleted bit
ProjectId int [ref: > Projects.Id]
AssignedToUserId int [ref: > Users.Id]
}

Table Tags {
Id int [pk, increment]
Name varchar(50)
}

Table TaskTags {
TaskId int [ref: > Tasks.Id]
TagId int [ref: > Tags.Id]
indexes {
(TaskId, TagId) [pk]
} 
}
*(ERD screenshot exported from dbdiagram.io: `schema-erd.png`, in
this folder)*

---

## 5. Column Types

| Table | Column | Type | Reasoning |
|---|---|---|---|
| Users | Id | `INT IDENTITY` | Auto-incrementing, simple internal reference |
| Users | Name | `VARCHAR(100)` | Sized for a realistic full name, not `NVARCHAR(MAX)` |
| Users | Email | `VARCHAR(150)` | Fits standard email length conventions |
| Projects | Budget | `DECIMAL(10,2)` | **Monetary value — never `FLOAT`.** `FLOAT` uses approximate binary representation and can introduce rounding errors (e.g. `0.1 + 0.2 ≠ 0.3` exactly) that are unacceptable in financial data. `DECIMAL` stores the exact value. |
| Tasks | PriorityLevel | `INT` | Small whole number (1–5), no need for `BIGINT` |
| Tasks | IsCompleted | `BIT` | True/false flag — SQL Server's boolean equivalent |
| Tasks | Title | `VARCHAR(200)` | Bounded length, avoids unnecessary storage overhead from `NVARCHAR(MAX)` |
| Tags | Name | `VARCHAR(50)` | Tags are short by nature; a generous but bounded limit |

---

## Design Notes — What I Learned

Working through the unnormalized starting table made the "why" behind
3NF click in a way the definition alone didn't. Writing out
`ProjectBudget` sitting on a `Tasks` row made it obvious that if two
tasks shared a project, updating the budget would mean finding and
updating every task row referencing it — exactly the update anomaly
the lesson described, just seen directly in my own domain instead of
an abstract example.

Deciding to add `Users` and `Tags` (beyond just the Day 1 resources)
was necessary to actually have a many-to-many relationship to
practice — without it, there was no real case for a join table, and
that's one of the more important normalization patterns to get
comfortable with early.

The `DECIMAL` vs `FLOAT` choice for `Budget` was the one column type
decision I made deliberately rather than by default — everything else
follows fairly naturally once the entities are correct, but money
specifically needed a conscious, correct choice from the start. 

