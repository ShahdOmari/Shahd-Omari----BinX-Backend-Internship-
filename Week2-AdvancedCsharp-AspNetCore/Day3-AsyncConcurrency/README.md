# Day 3 — Async/Await Deep Dive & Concurrency Basics

**8 hours**

## Learning Objectives

- Explain the Task-based asynchronous pattern and how it differs from
  synchronous code
- Avoid the most common async pitfalls, especially blocking on async
  code
- Run independent operations concurrently using `Task.WhenAll`

## What I Did

- Wrote 3 async methods simulating different data sources (database,
  external API, cache), each with a different `Task.Delay`
- Called all 3 sequentially with individual awaits and measured the
  total elapsed time (~4500ms — roughly the sum of all three delays)
- Rewrote the same calls using `Task.WhenAll` and compared the
  elapsed time (~2000ms — roughly the slowest single delay)
- Added a `CancellationToken` parameter to a longer operation and
  demonstrated cancelling it mid-way through, catching the resulting
  `OperationCanceledException`

## Key Code Example

```csharp
// Sequential: ~4500ms total
var r1 = await FetchFromDatabaseAsync();
var r2 = await FetchFromExternalApiAsync();
var r3 = await FetchFromCacheAsync();

// Concurrent: ~2000ms total
Task<string> t1 = FetchFromDatabaseAsync();
Task<string> t2 = FetchFromExternalApiAsync();
Task<string> t3 = FetchFromCacheAsync();
await Task.WhenAll(t1, t2, t3);
```

## What I Learned

Seeing the actual millisecond difference between sequential
(~4500ms) and concurrent (~2000ms) execution made the value of
`Task.WhenAll` concrete rather than theoretical — running independent
operations one after another wastes time for no real reason. The
cancellation demo also made it clear that a `CancellationToken` only
works usefully if it's checked *between* steps of a longer
operation, not just once at the start.

## Project

[`Day3Practice/`](Day3Practice/) — sequential vs. concurrent async
execution comparison, plus a cancellation demo.