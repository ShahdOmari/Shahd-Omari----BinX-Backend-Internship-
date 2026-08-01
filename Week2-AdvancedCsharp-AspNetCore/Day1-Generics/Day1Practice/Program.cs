using System;
using System.Collections.Generic;
using System.Linq;

// Testing the generic Repository with two different types from my Week 1 domain model.
// Same Repository<T> class, no duplicated code for TaskItem vs Project.

var taskRepo = new Repository<TaskItem>();
taskRepo.Add(new TaskItem("Design database schema", 3));
taskRepo.Add(new TaskItem("Write unit tests", 2));
taskRepo.Add(new TaskItem("Setup CI/CD pipeline", 5));

var projectRepo = new Repository<Project>();
projectRepo.Add(new Project("BinX API"));
projectRepo.Add(new Project("Internal Dashboard"));

Console.WriteLine("----- All Tasks -----");
foreach (var task in taskRepo.GetAll())
{
    Console.WriteLine($"- {task.Title} (Priority: {task.PriorityLevel})");
}

Console.WriteLine("\n----- All Projects -----");
foreach (var project in projectRepo.GetAll())
{
    Console.WriteLine($"- {project.Name}");
}

// Find() takes a predicate (a Func<T, bool>), so I can search by any condition
// without writing a separate method for every possible search.
Console.WriteLine("\n----- Find: High Priority Task -----");
var highPriorityTask = taskRepo.Find(t => t.PriorityLevel >= 5);
if (highPriorityTask != null)
{
    Console.WriteLine($"Found: {highPriorityTask.Title}");
}

// Proving GetAll() actually returns something read-only, not just in name.
Console.WriteLine("\n----- Testing IReadOnlyList Protection -----");
var tasksResult = taskRepo.GetAll();

// tasksResult.Add(new TaskItem("Hack attempt", 1));
// Uncommenting this line gives a compile error, not a runtime one:
// "IReadOnlyList<TaskItem> does not contain a definition for 'Add'"
// That's the point — the caller physically can't mutate the repo's internal list
// from outside, since IReadOnlyList doesn't expose any method that would allow it.

Console.WriteLine($"Tasks count (read-only view): {tasksResult.Count}");


// ===== Type Declarations =====

// Generic repository that can hold any single type of item (all TaskItems,
// or all Projects, but not mixed in the same instance).
//
// "where T : class" restricts T to reference types only. Two reasons this
// matters here: repositories are meant for entities that have identity
// (TaskItem, Project), not raw value types like int or a plain struct.
// It also means Find() can safely return null when nothing matches the
// predicate, since null is only a valid value for reference types — if T
// could be a value type like int, returning null wouldn't compile.
public class Repository<T> where T : class
{
    private readonly List<T> _items = new();

    public void Add(T item) => _items.Add(item);

    // Returning IReadOnlyList<T> instead of List<T> on purpose. If this
    // returned List<T>, anyone calling GetAll() could clear or modify the
    // repository's internal data directly, bypassing Add() entirely.
    // IReadOnlyList<T> still gives full read access (Count, indexing,
    // foreach) but removes any mutation methods from the interface itself.
    public IReadOnlyList<T> GetAll() => _items.AsReadOnly();

    // FirstOrDefault returns null (for reference types) instead of throwing
    // when nothing matches, which is exactly the behavior I want for a
    // "search and maybe find nothing" method.
    public T? Find(Func<T, bool> predicate) => _items.FirstOrDefault(predicate);
}

public class TaskItem
{
    public Guid Id { get; }
    public string Title { get; private set; }
    public int PriorityLevel { get; private set; }
    public bool IsCompleted { get; private set; }

    public TaskItem(string title, int priorityLevel)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title cannot be empty");
        if (priorityLevel < 1 || priorityLevel > 5)
            throw new ArgumentException("Priority must be between 1 and 5");

        Id = Guid.NewGuid();
        Title = title;
        PriorityLevel = priorityLevel;
        IsCompleted = false;
    }

    public void MarkComplete() => IsCompleted = true;
}

public class Project
{
    public Guid Id { get; }
    public string Name { get; private set; }

    public Project(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be empty");

        Id = Guid.NewGuid();
        Name = name;
    }
}