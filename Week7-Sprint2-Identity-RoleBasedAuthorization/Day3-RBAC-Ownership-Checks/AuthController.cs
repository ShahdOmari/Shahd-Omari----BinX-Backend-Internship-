using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CardiacMonitoring.Api.DTOs.Auth; 
using CardiacMonitoring.Api.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens; 
using Microsoft.AspNetCore.RateLimiting;  
using Microsoft.AspNetCore.Authorization;  
using CardiacMonitoring.Api.Data;
using CardiacMonitoring.Api.Entities; 
using Microsoft.EntityFrameworkCore;


namespace CardiacMonitoring.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[EnableRateLimiting("general")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    private readonly AppDbContext _context;

    public AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration,
    AppDbContext context)
{
    _userManager = userManager;
    _signInManager = signInManager;
    _roleManager = roleManager;
    _configuration = configuration;
    _context = context;
}

    [HttpPost("register")]
public async Task<IActionResult> Register(RegisterRequest request)
{
    // Both records must be created together — a user who can log in but
    // has no matching StaffProfile would hit confusing errors on every
    // domain-specific endpoint afterward. Same transaction pattern as
    // Sprint 1 Day 4's critical-reading follow-up scheduling.
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            await transaction.RollbackAsync();
            return BadRequest(result.Errors);
        }

        var profile = new StaffProfile
        {
            ApplicationUserId = user.Id,
            Department = request.Department,
            HireDate = DateTime.UtcNow
        };
        _context.StaffProfiles.Add(profile);
        await _context.SaveChangesAsync();

                // Every new registration becomes a Nurse by default — the most
        // common day-to-day account type (recording vitals, viewing
        // patients). Doctor and Auditor are privileged roles granted
        // explicitly afterward via assign-role, never self-selected.
        const string defaultRole = "Nurse";
        if (!await _roleManager.RoleExistsAsync(defaultRole))
            await _roleManager.CreateAsync(new IdentityRole(defaultRole));

        await _userManager.AddToRoleAsync(user, defaultRole);

        await transaction.CommitAsync();

        return Ok(new { message = "User registered successfully.", userId = user.Id, staffProfileId = profile.Id, role = defaultRole });
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
    [EnableRateLimiting("login")]
    [HttpPost("login")]
public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Generic error message either way — never confirm to the caller
        // whether the email specifically exists, which would leak
        // information useful for enumerating valid accounts.
        if (user == null)
            return Unauthorized(new { message = "Invalid email or password." });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            return Unauthorized(new { message = "Invalid email or password." });

        var token = await GenerateJwtToken(user);
        return Ok(new { token });
    }

    // Admin-only: without this restriction, any unauthenticated caller
    // could grant themselves (or anyone else) Doctor or Admin privileges.
    [Authorize(Roles = "Admin")]
    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole([FromQuery] string email, [FromQuery] string role)
    {
        if (!await _roleManager.RoleExistsAsync(role))
            await _roleManager.CreateAsync(new IdentityRole(role));

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound(new { message = "User not found." });

        await _userManager.AddToRoleAsync(user, role);
        return Ok(new { message = $"Role '{role}' assigned to {email}." });
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Email, user.Email!),
        };  
        // Domain-relevant claim beyond the standard identity ones — lets every
        // subsequent authenticated request resolve "which StaffProfile does this
        // token belong to" directly from the token itself, without an extra
        // database lookup by email on every single request.
        var staffProfile = await _context.StaffProfiles
        .FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);
        if (staffProfile is not null)
        {
        claims.Add(new Claim("staffProfileId", staffProfile.Id.ToString()));
        }
        // Adding one role claim per role the user holds — this is exactly
        // what [Authorize(Roles = "...")] checks against. Assigning a role
        // in the database alone is not enough; it must be embedded in the
        // token itself at login time (Week 4 Day 3's core lesson).
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
