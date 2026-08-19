using Microsoft.AspNetCore.Mvc;

namespace CardiacMonitoring.Api.Controllers;

// A deliberately minimal controller whose only purpose is to prove the
// global exception handler works end-to-end — not part of the actual
// domain API. Safe to keep in the codebase as a permanent, quick way to
// verify error handling after future changes to the pipeline.
[ApiController]
[Route("api/v1/[controller]")]
public class DiagnosticsController : ControllerBase
{
    [HttpGet("trigger-error")]
    public IActionResult TriggerError()
    {
        throw new InvalidOperationException(
            "This is a deliberately thrown test exception to verify the global exception handler.");
    }
}
