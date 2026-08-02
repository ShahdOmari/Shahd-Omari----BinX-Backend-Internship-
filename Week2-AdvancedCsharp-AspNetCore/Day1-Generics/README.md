# Day 1 — Generics & Advanced Collections

**8 hours**

## Learning Objectives

- Explain why generics exist and what problem they solve over
  object-typed collections
- Write generic methods and classes with type parameters
- Apply generic constraints to restrict what a type parameter can be

## What I Did

- Learned why generics exist: reusable, type-safe code without
  casting or losing compile-time safety (the alternative being
  untyped `object`-based collections)
- Built a generic `Repository<T>` class with `Add`, `GetAll`, and
  `Find(predicate)` methods
- Applied a `where T : class` constraint, with a comment explaining
  why: it restricts `T` to reference types, matching entities with
  identity, and allows `Find()` to safely return `null` when nothing
  matches
- Instantiated the repository with two different types from the
  Week 1 domain model (`TaskItem` and `Project`), proving the same
  class works generically across unrelated types
- Changed `GetAll()`'s return type from `List<T>` to
  `IReadOnlyList<T>` and confirmed at compile time that callers can
  no longer modify the result directly

## Key Code Example

```csharp
public class Repository<T> where T : class
{
    private readonly List<T> _items = new();
    public void Add(T item) => _items.Add(item);
    public IReadOnlyList<T> GetAll() => _items.AsReadOnly();
    public T? Find(Func<T, bool> predicate) => _items.FirstOrDefault(predicate);
}
```

## What I Learned

The constraint isn't just a syntax requirement — it's what lets the
generic code safely assume certain behavior (like being able to
return `null`) about any type that gets plugged in. Seeing the
compile error when trying to `.Add()` to an `IReadOnlyList<T>` made
the "return the least permissive interface" rule concrete instead of
just a guideline.

## Project

[`Day1Practice/`](Day1Practice/) — generic repository tested with two
unrelated types from the Week 1 domain model.