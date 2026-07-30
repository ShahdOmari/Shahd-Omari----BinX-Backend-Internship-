using System;
using System.Collections.Generic;
using System.Linq;

// 3. Instantiate the repository with two different types-----------

var taskRepo = new Repository<TaskItem>();
taskRepo.Add(new TaskItem("Design database schema", 3));
taskRepo.Add(new TaskItem("Write unit tests", 2));
taskRepo.Add(new TaskItem("Setup CI/CD pipeline", 5));

var projectRepo = new Repository<Project>();
projectRepo.Add(new Project("BinX API"));
projectRepo.Add(new Project("Internal Dashboard"));

Console.WriteLine("-----All Tasks-----");
foreach (var task in taskRepo.GetAll())
{
    Console.WriteLine($"- {task.Title} (Priority: {task.PriorityLevel})");
}

Console.WriteLine("\n----- All Projects -----");
foreach (var project in projectRepo.GetAll())
{
    Console.WriteLine($"- {project.Name}");
}

// Using Find with a predicate-------------------------------
Console.WriteLine("\n----- Find: High Priority Task -----");
var highPriorityTask = taskRepo.Find(t => t.PriorityLevel >= 5);
if (highPriorityTask != null)
{
    Console.WriteLine($"Found: {highPriorityTask.Title}");
}

// 4. Confirm GetAll's result cannot be modified directly -----------------------
Console.WriteLine("\n----- Testing IReadOnlyList Protection -----");
var tasksResult = taskRepo.GetAll();


Console.WriteLine($"Tasks count (read-only view): {tasksResult.Count}");




// 2. Generic Repository with a "where T : class" constraint-------------------------------
//
// WHY where T : class is needed here:
// This constraint restricts T to reference types only (like TaskItem, Project),
// excluding value types (int, bool, struct). This makes sense for a repository
// that stores entities with identity, value types don't have that concept.
// It also allows Find() to safely return null when no match is found,
// since null is only a valid value for reference types.
public class Repository<T> where T : class
{
    private readonly List<T> _items = new();

    public void Add(T item) => _items.Add(item);

    // 4. Returns IReadOnlyList instead of List, so the caller can view
    // the data but cannot Add, Remove, or modify it directly.------------------------
    public IReadOnlyList<T> GetAll() => _items.AsReadOnly();

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