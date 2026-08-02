# Day 1 — Program Orientation & .NET Development Environment Setup

**8 hours**

## Learning Objectives

- Understand the program's four phases and how Weeks 1–10 build on
  each other
- Install and verify a working .NET SDK
- Navigate the `dotnet` CLI to create, build, and run a console
  application

## What I Did

- Walked through the full program structure: 4 phases, weekly
  cadence, mentor check-ins, and the Phase 3 capstone
- Installed .NET SDK and verified it with `dotnet --version` and
  `dotnet --list-sdks`
- Set up VS Code with the C# Dev Kit extension (IntelliSense and
  debugger configured)
- Created, built, and ran a first console app (`HelloBinX`) with
  `dotnet new console` and `dotnet run`
- Modified the program to print my name and the current date
- Created a GitHub account and the internship repository

## Code

```csharp
Console.WriteLine("Shahd Omari");
Console.WriteLine(DateTime.Now);
```

## What I Learned

If the environment isn't set up correctly, everything afterward
becomes harder — verifying the SDK and running a simple "Hello World"
before moving on saved me from debugging environment issues later in
the week. This also made the `dotnet` CLI commands (`new console`,
`run`, `build`) feel automatic early, since I'll be using them for
every project going forward.

## Project

[`HelloBinX/`](HelloBinX/) — first console app confirming the
environment works end to end.