using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    // UserManager<IdentityUser> is Identity's main API for creating,
    // finding, and managing users — it handles password hashing
    // internally, so we never touch raw passwords ourselves.
    private readonly UserManager<IdentityUser> _userManager;

    public AuthController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        // CreateAsync hashes the password and persists the user in one
        // call. It returns an IdentityResult listing specific failures
        // (weak password, duplicate email, etc.) instead of just true/false,
        // so we can return a meaningful error message rather than a
        // generic "registration failed".
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { errors });
        }

        return Ok(new { message = "User registered successfully.", userId = user.Id });
    }
}