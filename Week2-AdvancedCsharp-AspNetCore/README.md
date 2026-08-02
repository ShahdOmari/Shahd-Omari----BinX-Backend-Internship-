# Week 2 — Advanced C# & ASP.NET Core Foundations

**Phase 1 → 2 · 40 hours · 5 training days · ASP.NET Core Foundations**

## Goal of the Week

Extend last week's C# fundamentals with generics, deeper LINQ, and a
more rigorous async/await model, then set up a first ASP.NET Core Web
API project — routing, the middleware pipeline, and dependency
injection — the exact request/response path every endpoint built for
the rest of the program will run through.

## What I Aimed to Learn

- Why generics exist, how to write generic classes/methods, and how
  to apply constraints (`where T : class`)
- The difference between deferred and immediate LINQ execution, and
  how to use `GroupBy`, `Join`, and `SelectMany`
- The Task-based asynchronous pattern, common async pitfalls (like
  blocking with `.Result`), and running operations concurrently with
  `Task.WhenAll`
- How to scaffold an ASP.NET Core Web API and define routes with both
  Controllers and Minimal APIs
- How the middleware pipeline works and how execution order affects
  behavior
- How to register and inject services using ASP.NET Core's built-in
  dependency injection container

## Day-by-Day

| Day | Topic | Status |
|---|---|---|
| [Day 1](Day1-Generics/README.md) | Generics & Advanced Collections | ✅ Complete |
| [Day 2](Day2-AdvancedLinq/README.md) | Advanced LINQ & Deferred Execution | ✅ Complete |
| [Day 3](Day3-AsyncConcurrency/README.md) | Async/Await & Concurrency | ✅ Complete |
| [Day 4](Day4-AspNetCoreSetup/README.md) | ASP.NET Core Setup & Routing | ✅ Complete |
| [Day 5](Day5-MiddlewareDI/README.md) | Middleware & Dependency Injection | ✅ Complete |

Each day's folder has its own README with the specific exercises,
code decisions, and what I learned that day.

## Week 2 Deliverables

- A generic repository class with appropriate constraints, committed
  to GitHub
- LINQ exercises demonstrating grouping, joining, flattening, and
  deferred execution
- An async/concurrency demo using `Task.WhenAll` and a
  `CancellationToken`
- A scaffolded ASP.NET Core Web API with at least 4 endpoints, built
  with both Controllers and Minimal APIs
- Custom middleware and at least one DI-registered service injected
  into a controller

## Week 2 Outcome

By the end of this week, I had a working ASP.NET Core Web API for the
first time — built with both Controllers and Minimal APIs, tested
through Swagger and Postman — along with a much deeper grip on C#
itself: generics with constraints, advanced LINQ (grouping, joining,
flattening, deferred execution), and async/await done correctly
(concurrent execution with `Task.WhenAll`, cancellation tokens, and
avoiding the `.Result` blocking trap). The middleware pipeline and
dependency injection lessons on Day 5 tie directly into how every
ASP.NET Core project in this program will be structured going
forward.