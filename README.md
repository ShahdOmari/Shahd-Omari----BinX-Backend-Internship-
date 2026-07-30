# BinX Tech Backend Internship

## About This Repository

This repository documents my progress through the **BinX Tech Backend Development Internship Program (.NET)** — a 10-week, 400-hour program split into four phases, culminating in a Phase 3 capstone project. It's organized week by week, with each week broken down into daily folders that contain the code, exercises, and notes from that day's learning.

The goal of this repo is not just to store code, but to track a clear learning path: what I set out to learn, what I actually built, and what I understood by the end of each day. Every folder is meant to be self-explanatory — someone browsing the repo (a mentor, a reviewer, or future me) should be able to understand the journey just by reading the READMEs.

**Tech stack covered so far:**
- C# and the .NET SDK
- Object-Oriented Programming (OOP) principles
- Collections & LINQ (including grouping, joining, and flattening)
- async/await
- Generics and constraints
- Git & GitHub workflow (commits, feature branches, pull requests)
- (Future weeks will expand this list as the internship progresses)

**Tools used:**
- Visual Studio Code (C# Dev Kit) or Visual Studio
- .NET SDK
- Git / GitHub
- Notion (weekly summaries and program tracker)

**📓 Notion Workspace:**  https://app.notion.com/p/BinX-Tech-Backend-Internship-3acf35c1957c80109d04e98eb418ed12?source=copy_link
 full program tracker with weekly and daily breakdowns.
---

## Repository Structure

```
BinX-Backend-Internship/
│
├── README.md                        ← this file: overview of the whole internship
│
├── Week1-Onboarding-Csharp-Git/
│   ├── README.md                    ← summary of Week 1 goals + what was accomplished
│   ├── Day1-EnvironmentSetup/
│   ├── Day2-TypesControlFlow/
│   ├── Day3-OOP/
│   ├── Day4-CollectionsLinq/
│   └── Day5-GitGithub/
│
├── Week2-AdvancedCsharp-AspNetCore/
│   ├── README.md                    ← summary of Week 2 goals + what was accomplished
│   ├── Day1-Generics/
│   ├── Day2-AdvancedLinq/
│   ├── Day3-AsyncConcurrency/
│   ├── Day4-AspNetCoreSetup/
│   └── Day5-MiddlewareDI/
│
├── Week3-.../                       ← added as the internship progresses
└── ...
```

Each week's folder contains its own `README.md` summarizing that week's objectives and outcomes, and each day's folder contains the relevant project code and notes.

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

To keep the history clean and reviewable, each task/day is developed on its own feature branch and merged into `main` via a Pull Request:

```bash
git checkout -b feature/weekX-dayY-topic
# ... work, commit ...
git add .
git commit -m "Clear description of what was done"
git push -u origin feature/weekX-dayY-topic
```

Then a Pull Request is opened on GitHub from the feature branch into `main`, describing exactly what was added.

> **Note:** Days 1–4 of Week 1 were committed directly to `main` while I was still getting comfortable with the basic Git commands. Starting with Day 5, I'm applying the full feature-branch → pull request workflow described above, and following it for every day/task since.

---

## Week 1 — Onboarding, C# Fundamentals & Git

**Phase 1 · 40 hours · 5 training days · Foundations**

### Goal of the Week
Set up a professional .NET development environment and build the core C# fundamentals every backend service in this program will be built on — types, OOP, collections, and LINQ — before touching ASP.NET Core or any web framework. The week closes with the same Git/GitHub feature-branch workflow used for every sprint later in the program.

### What I Aimed to Learn
- How to properly install, configure, and verify a .NET SDK development environment, and navigate the `dotnet` CLI
- The difference between **value types** and **reference types**, and why it matters
- Modern C# control-flow features (switch expressions, nullable reference types)
- When to use **class**, **record**, or **struct**, and the difference between **inheritance** and **interfaces**
- Choosing the right **collection** for a given access pattern, querying with **LINQ**, and writing basic **async/await** code
- A practical Git workflow: feature branches, clear commit messages, and opening a pull request

### Day-by-Day Breakdown

**Day 1 — Program Orientation & .NET Development Environment Setup**
- Walked through the full program structure: 4 phases, weekly cadence, mentor check-ins, and the Phase 3 capstone
- Installed .NET SDK and verified it with `dotnet --version` and `dotnet --list-sdks`
- Set up VS Code with the C# Dev Kit extension (IntelliSense + debugger configured)
- Created, built, and ran a first console app (`HelloBinX`) with `dotnet new console` and `dotnet run`
- Created a GitHub account and the internship repository
- *What I learned:* If the environment isn't set up correctly, everything afterward becomes harder — verifying the SDK and running a simple "Hello World" before moving on saved me from debugging environment issues later in the week.

**Day 2 — C# Fundamentals I: Types, Variables & Control Flow**
- Explored the difference between value types (e.g. `int`, `struct`) and reference types (e.g. `class`, arrays, strings), including copy-by-value vs. copy-by-reference behavior
- Practiced variables, type inference (`var`), and idiomatic naming
- Practiced modern control flow: `switch` expressions and loops (`for`, `foreach`, `while`)
- Learned about nullable reference types (`string?` vs `string`) and why they catch null-reference bugs at compile time instead of at runtime
- *What I learned:* The value vs. reference type distinction is the one thing I'll need to keep in mind constantly going forward. Testing it directly made it concrete: mutating a reference type (like an array) through one variable affected every other variable pointing to it, while a value type copy stayed completely independent. Strings behave like reference types but *act* immutable, which was the trickiest part to internalize.

**Day 3 — C# Fundamentals II: Object-Oriented Programming**
- Compared `class`, `record`, and `struct`, and when each one makes sense (identity vs. immutable data vs. small value data)
- Applied encapsulation: private backing fields, public properties, and constructors that enforce valid state
- Studied inheritance vs. interfaces, and why favoring interfaces over deep inheritance chains is usually more flexible and testable
- Modeled a small domain (a task tracker with `TaskItem` and `Project`) with polymorphism via an `ILoggable` interface, setting up the foundation for Dependency Injection (covered in Week 2)
- *What I learned:* The "IS-A vs. CAN-DO" rule of thumb made the inheritance-vs-interface decision much clearer. `TaskItem` and `Project` have no real inheritance relationship, but both needed the same logging capability — that's exactly the case an interface is for, and writing one method (`PrintLog`) that worked with both was the first time polymorphism felt genuinely useful rather than theoretical.

**Day 4 — C# Fundamentals III: Collections & LINQ Basics**
- Compared `List<T>`, `Dictionary<TKey, TValue>`, and `HashSet<T>`, and when each collection fits the access pattern
- Practiced LINQ using method syntax (filter, projection, aggregation) — the style most commonly used in real projects
- Wrote an `async`/`await` method simulating an I/O delay, since this becomes the foundation for every database call going forward
- Practiced exception handling: catching specific exception types (`FormatException`, `OverflowException`) meaningfully instead of a blanket `catch (Exception)`
- *What I learned:* Chaining `.Where()`, `.OrderBy()`, and `.Select()` made it obvious why LINQ is preferred over manual loops — the intent of each query is readable at a glance. Testing the `try/catch` with actual invalid input (not just correct input) was what made exception handling click, since I could see the specific exception type get caught instead of the program crashing.

**Day 5 — Git & GitHub Workflow; Week 1 Synthesis**
- Set up `.gitignore`, connected the local repo to a GitHub remote, and created a feature branch (`feature/week1-day5-git-workflow`)
- Committed the Day 5 practice project and README updates with clear, descriptive commit messages
- Opened a Pull Request from the feature branch into `main`, describing the week's work, and added the mentor as a collaborator to request their review
- Put together a Week 1 summary in Notion (environment setup notes, the daily exercises, and a link to the pull request) for the mentor check-in
- *What I learned:* Working on a feature branch instead of pushing straight to `main` made the purpose of the workflow concrete rather than theoretical — `main` stays stable and reviewable at all times, and the Pull Request is what actually documents "what changed and why" in one place instead of scattering that across individual commits.

### Week 1 Deliverables
- A verified, working .NET SDK development environment with a configured IDE
- A console program demonstrating C# types, control flow, and nullable reference type handling
- A small object-oriented domain model using classes, records, and at least one interface
- LINQ queries and an async method demonstrated over a collection
- A GitHub repository with a feature branch, clear commit history, and an opened pull request
- A Week 1 summary document in Notion, ready for the mentor check-in

### Week 1 Outcome
By the end of this week, the development environment was fully functional (.NET SDK, VS Code, C# Dev Kit, Git, GitHub), and I had hands-on practice with core C# concepts — value vs. reference types, OOP with encapsulation and interfaces, collections, LINQ, and async/await — along with a real Git feature-branch and Pull Request workflow. Both the C# fundamentals and the Git workflow carry forward directly into Week 2.

---

## Week 2 — Advanced C# & ASP.NET Core Foundations

**Phase 1 → 2 · 40 hours · 5 training days · ASP.NET Core Foundations**

### Goal of the Week
Extend last week's C# fundamentals with generics, deeper LINQ, and a more rigorous async/await model, then set up a first ASP.NET Core Web API project — routing, the middleware pipeline, and dependency injection — the exact request/response path every endpoint built for the rest of the program will run through.

### What I Aimed to Learn
- Why generics exist, how to write generic classes/methods, and how to apply constraints (`where T : class`)
- The difference between deferred and immediate LINQ execution, and how to use `GroupBy`, `Join`, and `SelectMany`
- The Task-based asynchronous pattern, common async pitfalls (like blocking with `.Result`), and running operations concurrently with `Task.WhenAll`
- How to scaffold an ASP.NET Core Web API and define routes with both Controllers and Minimal APIs
- How the middleware pipeline works and how execution order affects behavior
- How to register and inject services using ASP.NET Core's built-in dependency injection container

### Day-by-Day Breakdown

**Day 1 — Generics & Advanced Collections**
- Learned why generics exist: reusable, type-safe code without casting or losing compile-time safety (the alternative being untyped `object`-based collections)
- Built a generic `Repository<T>` class with `Add`, `GetAll`, and `Find(predicate)` methods
- Applied a `where T : class` constraint, with a comment explaining why: it restricts `T` to reference types, matching entities with identity, and allows `Find()` to safely return `null` when nothing matches
- Instantiated the repository with two different types from the Week 1 domain model (`TaskItem` and `Project`), proving the same class works generically across unrelated types
- Changed `GetAll()`'s return type from `List<T>` to `IReadOnlyList<T>` and confirmed at compile time that callers can no longer modify the result directly
- *What I learned:* The constraint isn't just a syntax requirement — it's what lets the generic code safely assume certain behavior (like being able to return `null`) about any type that gets plugged in. Seeing the compile error when trying to `.Add()` to an `IReadOnlyList<T>` made the "return the least permissive interface" rule concrete instead of just a guideline.

**Day 2 — Advanced LINQ & Deferred Execution**
- Created two related collections (`Customers` and `Orders`, 6 items each) sharing a `CustomerId` foreign key
- Wrote a `GroupBy` query summarizing the total order amount per customer
- Wrote a `Join` query combining customer names with their order amounts
- Wrote a `SelectMany` query flattening every item across every order into a single sequence
- Demonstrated deferred execution directly: defined a query with `.Where()`, added a new order to the source list *after* defining the query but *before* enumerating it, then showed the new order appeared in the results anyway
- *What I learned:* Deferred execution stopped being an abstract warning and became something I could actually see happen — the new order (#107) showed up in the query results even though it didn't exist yet when the query was written, because `Where()` re-evaluates against the current state of the collection at the moment it's enumerated, not the moment it's defined. That's exactly the kind of subtle bug the lesson warned about if you don't expect it.

**Day 3 — Async/Await Deep Dive & Concurrency Basics**
- *Status: pending*
- *What I learned:* [fill in after completing the day]

**Day 4 — ASP.NET Core Project Setup & Routing**
- *Status: pending*
- *What I learned:* [fill in after completing the day]

**Day 5 — Middleware Pipeline & Dependency Injection; Week 2 Synthesis**
- *Status: pending*
- *What I learned:* [fill in after completing the day]

### Week 2 Deliverables
- A generic repository class with appropriate constraints, committed to GitHub
- LINQ exercises demonstrating grouping, joining, flattening, and deferred execution
- An async/concurrency demo using `Task.WhenAll` and a cancellation token
- A scaffolded ASP.NET Core Web API with at least 4 endpoints, built with both Controllers and Minimal APIs
- Custom middleware and at least one DI-registered service injected into a controller
- A Week 2 summary document in Notion, ready for the mentor check-in

### Week 2 Outcome
*To be completed once Day 5 is finished.*

---

## Upcoming Weeks
This section will be updated as new weeks are added, each with its own goals and summary linked from here.