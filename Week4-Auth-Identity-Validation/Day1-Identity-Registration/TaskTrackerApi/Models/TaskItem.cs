namespace TaskTrackerApi.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PriorityLevel { get; set; }
    public bool IsCompleted { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    // Changed from int to string to match IdentityUser's Id type.
    public string AssignedToUserId { get; set; } = string.Empty;

    public List<Tag> Tags { get; set; } = new();
}