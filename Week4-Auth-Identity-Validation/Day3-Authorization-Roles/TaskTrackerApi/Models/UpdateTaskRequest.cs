namespace TaskTrackerApi.Models;

public record UpdateTaskRequest(string Title, int PriorityLevel, bool IsCompleted);