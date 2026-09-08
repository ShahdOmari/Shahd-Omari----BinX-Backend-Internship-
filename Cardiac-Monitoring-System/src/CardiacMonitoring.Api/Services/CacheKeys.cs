namespace CardiacMonitoring.Api.Services;

public static class CacheKeys
{
    public static string Patients(int page, int pageSize, string? gender, int? minAge, string sortBy, string sortDirection)
        => $"patients:page={page}&size={pageSize}&gender={gender ?? ""}&minAge={minAge}&sort={sortBy}&dir={sortDirection}";

    public const string PatientsBase = "patients:page=1&size=10&gender=&minAge=&sort=name&dir=asc";
}
