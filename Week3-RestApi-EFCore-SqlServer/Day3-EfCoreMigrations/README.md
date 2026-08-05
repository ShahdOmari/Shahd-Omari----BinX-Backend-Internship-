# Day 3 — Entity Framework Core Setup & Code-First Migrations

**8 hours**

## Learning Objectives

- Install and configure Entity Framework Core with SQL Server
- Define entity classes and a DbContext matching the Day 2 schema
- Generate and apply code-first migrations

## What I Did

- Installed `Microsoft.EntityFrameworkCore.SqlServer` and
  `Microsoft.EntityFrameworkCore.Tools`, plus the global `dotnet-ef`
  tool
- Defined entity classes (`User`, `Project`, `TaskItem`, `Tag`)
  matching the Day 2 ERD, including navigation properties for every
  relationship
- Created an `AppDbContext` exposing a `DbSet<T>` for each entity, and
  registered it in `Program.cs` using a LocalDB connection string
- Explicitly configured the many-to-many relationship between
  `TaskItem` and `Tag` (producing a `TaskTags` join table), and
  restricted the delete behavior on `AssignedToUserId` to avoid a
  multiple-cascade-paths conflict with the `Project → Owner` and
  `Task → Project` cascade chains
- Explicitly set `HasPrecision(18, 2)` on `Project.Budget` to remove
  EF Core's default-precision warning, consistent with the `DECIMAL`
  reasoning from the Day 2 schema document
- Ran `dotnet ef migrations add InitialCreate` and inspected the
  generated migration file
- Ran `dotnet ef database update`, creating `TaskTrackerDb` on
  LocalDB with all 5 tables (`Users`, `Projects`, `Tasks`, `Tags`,
  `TaskTags`) and their foreign keys and indexes
- Verified the connection with `dotnet ef dbcontext info`

## Key Code Example

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<TaskItem>()
        .HasMany(t => t.Tags)
        .WithMany(tag => tag.Tasks)
        .UsingEntity(j => j.ToTable("TaskTags"));

    modelBuilder.Entity<TaskItem>()
        .HasOne(t => t.AssignedToUser)
        .WithMany()
        .HasForeignKey(t => t.AssignedToUserId)
        .OnDelete(DeleteBehavior.NoAction);

    modelBuilder.Entity<Project>()
        .Property(p => p.Budget)
        .HasPrecision(18, 2);
}
```

## What I Learned

The "multiple cascade paths" error was the most valuable part of
today — SQL Server refused to create the schema because a `User`
could be deleted through two different cascade routes at once
(directly via `Tasks.AssignedToUserId`, and indirectly through
`Projects.OwnerId → Tasks.ProjectId`). This made a real, concrete case
for `DeleteBehavior.NoAction`: not every foreign key should cascade
delete, especially when a single deletion could trigger conflicting
paths through the schema. Reading the actual generated SQL in the
migration file (not just trusting that "it probably works") is what
made this error make sense instead of just being a wall of red text.

I also ran into a project mix-up early on: the project was copied
from Week 2's Day 5 folder to save setup time, but a leftover
`TaskItem` record definition in the old `Program.cs` conflicted with
the new `TaskItem` entity class, causing a confusing compiler error
that had nothing to do with EF Core itself. Cleaning `Program.cs`
down to just what Day 3 actually needed resolved it immediately —
a reminder to check for leftover code from a copied project before
assuming a new error is related to the new code being written.

## Project

[`TaskTrackerApi/`](TaskTrackerApi/) — entity classes, DbContext, and
the first code-first migration, verified against a real LocalDB
database.