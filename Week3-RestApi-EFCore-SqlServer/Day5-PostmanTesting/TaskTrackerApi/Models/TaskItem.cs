namespace TaskTrackerApi.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PriorityLevel { get; set; }
    public bool IsCompleted { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int AssignedToUserId { get; set; }
    public User AssignedToUser { get; set; } = null!;

    public List<Tag> Tags { get; set; } = new();
}