using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration; 
    private readonly RoleManager<IdentityRole> _roleManager;


public AuthController(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration)
{
    _userManager = userManager;
    _signInManager = signInManager;
    _roleManager = roleManager;
    _configuration = configuration;
}

[HttpPost("assign-role")]
public async Task<IActionResult> AssignRole(string email, string role)
{
    if (!await _roleManager.RoleExistsAsync(role))
        await _roleManager.CreateAsync(new IdentityRole(role));

    var user = await _userManager.FindByEmailAsync(email);
    if (user == null)
        return NotFound(new { message = "User not found." });

    await _userManager.AddToRoleAsync(user, role);
    return Ok(new { message = $"Role '{role}' assigned to {email}." });
}

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = new IdentityUser { UserName = request.Email, Email = request.Email };
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { errors });
        }

        return Ok(new { message = "User registered successfully.", userId = user.Id });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Checking for null first, and using a generic "Invalid email or
        // password" message either way — never confirm to the caller
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

    private async Task<string> GenerateJwtToken(IdentityUser user)
{
    var roles = await _userManager.GetRolesAsync(user);

    var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Email, user.Email!),
    };

    // Adding one role claim per role the user holds — this is what
    // [Authorize(Roles = "Admin")] actually checks against.
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
        expires: DateTime.UtcNow.AddMinutes(15),
        signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
}
}