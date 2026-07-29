// Testing everything ---------------------------------------

var request = new CreateTaskRequest("Design database schema", "BinX API", 3);
Console.WriteLine($"Request received: {request.Title} for project {request.ProjectName}");


var project = new Project(request.ProjectName);
var task1 = new TaskItem(request.Title, request.PriorityLevel);
var task2 = new TaskItem("Write unit tests", 2);

project.AddTask(task1);
project.AddTask(task2);

task1.MarkComplete();


PrintLog(task1);   // TaskItem
PrintLog(task2);   // TaskItem
PrintLog(project); // Project 

//  Polymorphism: method 
static void PrintLog(ILoggable item)
{
    item.LogActivity();
} 



//  Record: DTO ------------------------------------------
public record CreateTaskRequest(string Title, string ProjectName, int PriorityLevel);

// Interface---------------------------------------------
public interface ILoggable
{
    void LogActivity();
}  

//  Class TaskItem------------------------------------------
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

//  Class Project------------------------------------------------------------
public class Project : ILoggable    
{
    private List<TaskItem> _tasks;

    public Guid Id { get; }
    public string Name { get; private set; }
    public int TaskCount => _tasks.Count;

    public Project(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be empty");

        Id = Guid.NewGuid();
        Name = name;
        _tasks = new List<TaskItem>();
    }

    public void AddTask(TaskItem task)
    {
        _tasks.Add(task);
    }

    public void LogActivity()
    {
        Console.WriteLine($"[Project Log] '{Name}' - Total Tasks: {TaskCount}");
    }
}


