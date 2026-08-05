namespace TaskTrackerApi.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Budget { get; set; }

    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public List<TaskItem> Tasks { get; set; } = new();
}