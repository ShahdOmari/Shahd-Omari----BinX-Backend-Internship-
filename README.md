# BinX Tech Backend Internship

## About This Repository

This repository documents my progress through the **BinX Tech Backend Development Internship Program (.NET)** — a 10-week, 400-hour program split into four phases, culminating in a Phase 3 capstone project. It's organized week by week, with each week broken down into daily folders that contain the code, exercises, and a README for that day's work.

Every folder has its own README: a summary at the week level, and a detailed one at the day level covering what was done, key code, and what I learned. This file is just the entry point — start here, then follow the links below into whichever week or day you want to see.

**Tech stack covered so far:**
- C# and the .NET SDK
- Object-Oriented Programming (OOP) principles
- Collections & LINQ (including grouping, joining, flattening, deferred execution)
- async/await and concurrency (Task.WhenAll, CancellationToken)
- Generics and constraints
- ASP.NET Core (Controllers, Minimal APIs, middleware, dependency injection)
- Git & GitHub workflow (commits, feature branches, pull requests)
- (Future weeks will expand this list as the internship progresses)

**Tools used:**
- Visual Studio Code (C# Dev Kit) or Visual Studio
- .NET SDK
- Git / GitHub
- Postman

---

## Repository Structure

```
BinX-Backend-Internship/
│
├── README.md                              ← this file
│
├── Week1-Onboarding-Csharp-Git/
│   ├── README.md                          ← Week 1 summary
│   ├── Day1-EnvironmentSetup/README.md
│   ├── Day2-TypesControlFlow/README.md
│   ├── Day3-OOP/README.md
│   ├── Day4-CollectionsLinq/README.md
│   └── Day5-GitGithub/README.md
│
├── Week2-AdvancedCsharp-AspNetCore/
│   ├── README.md                          ← Week 2 summary
│   ├── Day1-Generics/README.md
│   ├── Day2-AdvancedLinq/README.md
│   ├── Day3-AsyncConcurrency/README.md
│   ├── Day4-AspNetCoreSetup/README.md
│   └── Day5-MiddlewareDI/README.md
│
├── Week3-.../                             ← added as the internship progresses
└── ...
```

---

## How to Run a Project

Most days include one or more .NET console/web projects. To run any of them:

```bash
cd path/to/DayX-Topic/ProjectName
dotnet run
```

Make sure the .NET SDK is installed and available (`dotnet --version` to check).

---

## Git Workflow

Each task/day is developed on its own feature branch and merged into `main` via a Pull Request:

```bash
git checkout -b feature/weekX-dayY-topic
# ... work, commit ...
git add .
git commit -m "Clear description of what was done"
git push -u origin feature/weekX-dayY-topic
```

A Pull Request is then opened on GitHub from the feature branch into `main`, describing what was added.

> **Note:** Days 1–4 of Week 1 were committed directly to `main` while I was still getting comfortable with the basic Git commands. Starting with Day 5, I've applied the full feature-branch → pull request workflow for every day/task since.

---

## Week 1 — Onboarding, C# Fundamentals & Git

**Phase 1 · 40 hours · 5 training days · Foundations**

Set up a professional .NET development environment and built the core C# fundamentals every backend service in this program is built on — types, OOP, collections, and LINQ — then closed with a real Git/GitHub feature-branch workflow.

| Day | Topic |
|---|---|
| [Day 1](Week1-Onboarding-Csharp-Git/Day1-EnvironmentSetup/README.md) | Environment Setup & .NET CLI |
| [Day 2](Week1-Onboarding-Csharp-Git/Day2-TypesControlFlow/README.md) | Types, Variables & Control Flow |
| [Day 3](Week1-Onboarding-Csharp-Git/Day3-OOP/README.md) | Object-Oriented Programming |
| [Day 4](Week1-Onboarding-Csharp-Git/Day4-CollectionsLinq/README.md) | Collections & LINQ Basics |
| [Day 5](Week1-Onboarding-Csharp-Git/Day5-GitGithub/README.md) | Git & GitHub Workflow |

**Full summary:** [Week1-Onboarding-Csharp-Git/README.md](Week1-Onboarding-Csharp-Git/README.md)
**Pull Request:** https://github.com/ShahdOmari/Shahd-Omari----BinX-Backend-Internship-/pull/1

---

## Week 2 — Advanced C# & ASP.NET Core Foundations

**Phase 1 → 2 · 40 hours · 5 training days · ASP.NET Core Foundations**

Extended C# with generics, deeper LINQ, and a more rigorous async/await model, then built a first ASP.NET Core Web API — routing, the middleware pipeline, and dependency injection.

| Day | Topic |
|---|---|
| [Day 1](Week2-AdvancedCsharp-AspNetCore/Day1-Generics/README.md) | Generics & Advanced Collections |
| [Day 2](Week2-AdvancedCsharp-AspNetCore/Day2-AdvancedLinq/README.md) | Advanced LINQ & Deferred Execution |
| [Day 3](Week2-AdvancedCsharp-AspNetCore/Day3-AsyncConcurrency/README.md) | Async/Await & Concurrency |
| [Day 4](Week2-AdvancedCsharp-AspNetCore/Day4-AspNetCoreSetup/README.md) | ASP.NET Core Setup & Routing |
| [Day 5](Week2-AdvancedCsharp-AspNetCore/Day5-MiddlewareDI/README.md) | Middleware & Dependency Injection |

**Full summary:** [Week2-AdvancedCsharp-AspNetCore/README.md](Week2-AdvancedCsharp-AspNetCore/README.md)

---

## Upcoming Weeks

This section will be updated as new weeks are added, each linked from here the same way.