namespace TaskTrackerApi.Models;

public record CreateTaskRequest(string Title, int PriorityLevel, int ProjectId, string AssignedToUserId);
