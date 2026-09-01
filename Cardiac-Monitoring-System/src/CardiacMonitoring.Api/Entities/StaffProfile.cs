namespace CardiacMonitoring.Api.Entities;

// Deliberately separate from ApplicationUser — Identity's table handles
// authentication concerns (email, password hash, roles); it has no business
// knowing a staff member's department or hire date. Linking via a foreign
// key back to ApplicationUser.Id keeps the two concerns cleanly separated
// while still letting the API resolve "which staff profile does this
// authenticated request belong to?" in one query via a claim in the JWT.
public class StaffProfile
{
    public int Id { get; set; }
    public string ApplicationUserId { get; set; } = null!;
    public string Department { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
}
