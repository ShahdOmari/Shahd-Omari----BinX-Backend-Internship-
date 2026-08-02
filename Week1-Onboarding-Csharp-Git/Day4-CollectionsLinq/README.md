# Day 4 — C# Fundamentals III: Collections & LINQ Basics

**8 hours**

## Learning Objectives

- Choose the right collection type for a given data-access pattern
- Query and transform collections using LINQ
- Write and reason about basic asynchronous code with async/await

## What I Did

- Compared `List<T>`, `Dictionary<TKey, TValue>`, and `HashSet<T>`,
  and when each collection fits the access pattern
- Created a list of 8 `TaskItem` objects with varied priority and
  completion status
- Wrote 3 LINQ queries: a filter (incomplete tasks), a projection
  (high-priority task titles), and an aggregation (count and average
  priority of completed tasks)
- Wrote an async method (`FetchTaskSummaryAsync`) simulating an
  I/O delay with `Task.Delay`, and awaited it from the main program
- Handled invalid user input safely using try/catch with specific
  exception types (`FormatException`, `OverflowException`) instead of
  a generic catch

## Key Code Example

```csharp
var incompleteTasks = tasks.Where(t => !t.IsCompleted).ToList();

var highPriorityTitles = tasks
    .Where(t => t.PriorityLevel >= 3)
    .OrderByDescending(t => t.PriorityLevel)
    .Select(t => t.Title)
    .ToList();

double avgCompletedPriority = tasks
    .Where(t => t.IsCompleted)
    .Average(t => t.PriorityLevel);
```

## What I Learned

Chaining `.Where()`, `.OrderBy()`, and `.Select()` made it obvious why
LINQ is preferred over manual loops — the intent of each query is
readable at a glance. Testing the try/catch with actual invalid input
(not just correct input) was what made exception handling click,
since I could see the specific exception type get caught instead of
the program crashing.

## Project

[`Day4Practice/`](Day4Practice/) — LINQ queries, an async method with
`Task.Delay`, and exception handling over a Task Tracker domain
model.