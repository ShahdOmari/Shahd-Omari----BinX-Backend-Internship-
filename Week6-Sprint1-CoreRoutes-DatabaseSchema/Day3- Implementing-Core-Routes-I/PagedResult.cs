namespace CardiacMonitoring.Api.DTOs.Common;

// Generic paginated response wrapper — reusable for any resource's list
// endpoint, not just Patients. Includes TotalCount so a client can build
// pagination controls (total pages) without a separate count request.
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
