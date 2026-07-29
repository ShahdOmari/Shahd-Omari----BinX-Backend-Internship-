# BinX Tech Backend Internship

## About This Repository

This repository documents my progress through the **BinX Tech Backend Development Internship Program (.NET)** — a 10-week, 400-hour program split into four phases, culminating in a Phase 3 capstone project. It's organized week by week, with each week broken down into daily folders that contain the code, exercises, and notes from that day's learning.

The goal of this repo is not just to store code, but to track a clear learning path: what I set out to learn, what I actually built, and what I understood by the end of each day. Every folder is meant to be self-explanatory — someone browsing the repo (a mentor, a reviewer, or future me) should be able to understand the journey just by reading the READMEs.

**Tech stack covered so far:**
- C# and the .NET SDK
- Object-Oriented Programming (OOP) principles
- Collections & LINQ
- async/await
- Git & GitHub workflow (commits, feature branches, pull requests)
- (Future weeks will expand this list as the internship progresses)

**Tools used:**
- Visual Studio Code (C# Dev Kit) or Visual Studio
- .NET SDK
- Git / GitHub
- Notion (weekly summaries and program tracker)

**📓 Notion Workspace:**  
https://app.notion.com/p/BinX-Tech-Backend-Internship-3acf35c1957c80109d04e98eb418ed12?source=copy_link
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
├── Week2-.../                       ← added as the internship progresses
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

> **Note:** Days 1–4 of Week 1 were committed directly to `main` while I was still getting comfortable with the basic Git commands. Starting with Day 5, I'm applying the full feature-branch → pull request workflow described above, and will follow it for every day/task going forward.

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
- *Status: in progress*
- Practicing the full workflow: `.gitignore`, remote setup, feature branches, and pull requests
- Learning to write clear, descriptive commit messages in imperative mood instead of vague ones like "update"
- Opening a pull request from a feature branch into `main` with a clear description, and requesting the mentor as reviewer
- Putting together a Week 1 summary in Notion (environment setup notes, exercises, and a link to the pull request) for the mentor check-in
- *What I learned:* [fill in after completing the day]

### Week 1 Deliverables
- A verified, working .NET SDK development environment with a configured IDE
- A console program demonstrating C# types, control flow, and nullable reference type handling
- A small object-oriented domain model using classes, records, and at least one interface
- LINQ queries and an async method demonstrated over a collection
- A GitHub repository with a feature branch, clear commit history, and an opened pull request
- A Week 1 summary document in Notion, ready for the mentor check-in

**🔗 Pull Request: https://app.notion.com/p/BinX-Tech-Backend-Internship-3acf35c1957c80109d04e98eb418ed12?source=copy_link
### Week 1 Outcome
*To be completed once Day 5 is finished.*

---

## Upcoming Weeks
This section will be updated as new weeks are added, each with its own goals and summary linked from here.