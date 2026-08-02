# Day 3 — C# Fundamentals II: Object-Oriented Programming

**8 hours**

## Learning Objectives

- Choose between a class, a record, and a struct for a given modeling
  need
- Apply encapsulation using access modifiers and properties
- Use inheritance and interfaces to model shared behavior and
  polymorphism

## What I Did

- Compared class, record, and struct, and when each one makes sense
  (identity vs. immutable data vs. small value data)
- Built a small Task Tracker domain model:
  - `TaskItem` class — private backing fields, public properties,
    constructor validation
  - `Project` class — holds a list of `TaskItem` objects
  - `CreateTaskRequest` record — immutable DTO
  - `ILoggable` interface — implemented by both `TaskItem` and
    `Project`
- Wrote a single `PrintLog(ILoggable item)` method that worked
  correctly with both unrelated classes, demonstrating polymorphism

## Key Code Example

```csharp
public interface ILoggable
{
    void LogActivity();
}

public class TaskItem : ILoggable { /* ... */ }
public class Project : ILoggable { /* ... */ }

// Same method works with either type:
static void PrintLog(ILoggable item) => item.LogActivity();
```

## What I Learned

The "IS-A vs. CAN-DO" rule of thumb made the inheritance-vs-interface
decision much clearer. `TaskItem` and `Project` have no real
inheritance relationship, but both needed the same logging
capability — that's exactly the case an interface is for. Writing one
method that worked with both was the first time polymorphism felt
genuinely useful rather than theoretical, since it directly avoided
writing two near-identical methods.

## Project

[`Day3Practice/`](Day3Practice/) — Task Tracker domain model with
encapsulation, a record DTO, and an interface shared by two unrelated
classes.
