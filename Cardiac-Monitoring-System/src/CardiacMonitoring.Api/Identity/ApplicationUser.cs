using Microsoft.AspNetCore.Identity;

namespace CardiacMonitoring.Api.Identity;

// Extends the default IdentityUser with domain-specific fields real medical
// staff accounts actually need — a display name (IdentityUser only has
// UserName/Email, neither meant for display) and a professional license
// number, relevant for both Nurse and Doctor accounts and useful groundwork
// for the Auditor role's compliance reviews later this sprint.
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
}
