namespace TaskTrackerApi.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Budget { get; set; }

    // Changed from int to string to match IdentityUser's Id type.
    public string OwnerId { get; set; } = string.Empty;

    public List<TaskItem> Tasks { get; set; } = new();
}