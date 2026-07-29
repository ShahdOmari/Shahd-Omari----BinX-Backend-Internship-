using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//  Create a list of at least 8 TaskItem objects---------------------------
List<TaskItem> tasks = new List<TaskItem>
{
    new TaskItem("Design database schema", 3),
    new TaskItem("Write unit tests", 2),
    new TaskItem("Setup CI/CD pipeline", 5),
    new TaskItem("Fix login bug", 4),
    new TaskItem("Update documentation", 1),
    new TaskItem("Review pull request", 2),
    new TaskItem("Deploy to staging", 5),
    new TaskItem("Refactor auth service", 3)
};


tasks[0].MarkComplete();
tasks[2].MarkComplete();
tasks[4].MarkComplete();
tasks[6].MarkComplete();

Console.WriteLine("---- All Tasks ----");
foreach (var t in tasks)
{
    Console.WriteLine($"- {t.Title} | Priority: {t.PriorityLevel} | Completed: {t.IsCompleted}");
}

//  LINQ Queries----------------------------------------------------------- 


var incompleteTasks = tasks
    .Where(t => !t.IsCompleted)
    .ToList();

Console.WriteLine("\n----- Incomplete Tasks (Filter) -----");
foreach (var t in incompleteTasks)
{
    Console.WriteLine($"- {t.Title}");
}


var highPriorityTitles = tasks
    .Where(t => t.PriorityLevel >= 3)
    .OrderByDescending(t => t.PriorityLevel)
    .Select(t => t.Title)
    .ToList();

Console.WriteLine("\n----- High Priority Task Titles (Projection) -----");
foreach (var title in highPriorityTitles)
{
    Console.WriteLine($"- {title}");
}


double averageCompletedPriority = tasks
    .Where(t => t.IsCompleted)
    .Average(t => t.PriorityLevel);

int completedCount = tasks.Count(t => t.IsCompleted);

Console.WriteLine("\n----- Aggregation -----");
Console.WriteLine($"Completed tasks count: {completedCount}");
Console.WriteLine($"Average priority of completed tasks: {averageCompletedPriority:F2}");

//  Async Method---------------------------------------------------------
Console.WriteLine("\n----- Async Operation -----");
string result = await FetchTaskSummaryAsync(tasks.Count);
Console.WriteLine(result);

//  Try/Catch for a risky operation ----------------------------------------
Console.WriteLine("\n----- Exception Handling -----");
Console.Write("Enter a priority level to search for (1-5): ");
string? userInput = Console.ReadLine();

try
{
    int priorityToFind = int.Parse(userInput ?? string.Empty);

    var matchingTasks = tasks
        .Where(t => t.PriorityLevel == priorityToFind)
        .ToList();

    Console.WriteLine($"Found {matchingTasks.Count} task(s) with priority {priorityToFind}.");
}
catch (FormatException)
{
    Console.WriteLine("Invalid input. Please enter a valid whole number.");
}
catch (OverflowException)
{
    Console.WriteLine("The number entered is too large.");
}
finally
{
    Console.WriteLine("Search attempt finished.");
}


// Local function: Async method simulating an I/O delay------------------- 
static async Task<string> FetchTaskSummaryAsync(int taskCount)
{
    Console.WriteLine("Fetching task summary from server...");
    await Task.Delay(2000); 
    return $"Summary ready: {taskCount} tasks loaded successfully.";
}


// Type Declarations-------------------------------------------------

public record CreateTaskRequest(string Title, string ProjectName, int PriorityLevel);

public interface ILoggable
{
    void LogActivity();
}

public class TaskItem : ILoggable
{
    private bool _isCompleted;

    public Guid Id { get; }
    public string Title { get; private set; }
    public int PriorityLevel { get; private set; }
    public bool IsCompleted => _isCompleted;

    public TaskItem(string title, int priorityLevel)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title cannot be empty");
        if (priorityLevel < 1 || priorityLevel > 5)
            throw new ArgumentException("Priority must be between 1 and 5");

        Id = Guid.NewGuid();
        Title = title;
        PriorityLevel = priorityLevel;
        _isCompleted = false;
    }

    public void MarkComplete()
    {
        _isCompleted = true;
    }

    public void LogActivity()
    {
        Console.WriteLine($"[TaskItem Log] '{Title}' - Priority: {PriorityLevel} - Completed: {_isCompleted}");
    }
}