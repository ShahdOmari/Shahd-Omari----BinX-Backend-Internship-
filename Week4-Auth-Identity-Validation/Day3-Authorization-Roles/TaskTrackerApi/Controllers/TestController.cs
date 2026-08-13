using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskTrackerApi.Controllers;

[ApiController]
public class TestController : ControllerBase
{
    [Authorize]
    [HttpGet("api/v1/test/protected")]
    public IActionResult ProtectedEndpoint() => Ok(new { message = "You are authenticated!" });
}