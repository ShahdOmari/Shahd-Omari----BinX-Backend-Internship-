using CardiacMonitoring.Api.DTOs.VitalSigns;
using CardiacMonitoring.Api.Entities;
using CardiacMonitoring.Api.Repositories;
using CardiacMonitoring.Api.Services;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Authorization; 
using FluentValidation; 
using Microsoft.AspNetCore.RateLimiting;

namespace CardiacMonitoring.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[EnableRateLimiting("general")]
public class VitalSignsController : ControllerBase
{
    private readonly IRepository<VitalSign> _repository;
    private readonly IVitalSignService _vitalSignService;
    private readonly IValidator<CreateVitalSignRequest> _validator;

    public VitalSignsController(
        IRepository<VitalSign> repository,
        IVitalSignService vitalSignService,
        IValidator<CreateVitalSignRequest> validator)
    {
        _repository = repository;
        _vitalSignService = vitalSignService;
        _validator = validator;
    }

    // Reads the staffProfileId claim embedded at login (AuthController.
    // GenerateJwtToken) — this is what makes ownership checks possible
    // without an extra database round-trip on every request.
    private int? CurrentStaffProfileId =>
        int.TryParse(User.FindFirst("staffProfileId")?.Value, out var id) ? id : null;

    // Only Doctor, Auditor and Admin see the full list across all
    // patients — a Nurse's day-to-day view is scoped to her own
    // recorded readings via GetById + ownership, not a global browse.
    [Authorize(Roles = "Doctor,Auditor,Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var vitals = await _repository.GetAllAsync();
        var response = vitals.Select(ToResponse);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var vital = await _repository.GetByIdAsync(id);
        if (vital is null)
            return NotFound();

        // Ownership check: a Nurse may only view a reading she recorded
        // herself. Doctor, Auditor and Admin bypass this — they need
        // visibility across all patients to do their job.
        var isPrivileged = User.IsInRole("Doctor") || User.IsInRole("Auditor") || User.IsInRole("Admin");
        if (!isPrivileged && vital.RecordedByStaffProfileId != CurrentStaffProfileId)
            return Forbid();

        return Ok(ToResponse(vital));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateVitalSignRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        var response = await _vitalSignService.RecordReadingAsync(request, CurrentStaffProfileId);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    // Clinically sensitive — restricted to the roles that act on critical
    // readings directly (Doctor) or oversee the whole system (Admin).
    [Authorize(Roles = "Doctor,Admin")]
    [HttpGet("critical")]
    public async Task<IActionResult> GetCriticalPatients()
    {
        var allVitals = await _repository.GetAllAsync();

        var latestCriticalPerPatient = allVitals
            .GroupBy(v => v.PatientId)
            .Select(g => g.OrderByDescending(v => v.RecordedAtUtc).First())
            .Where(v => v.RiskLevel == RiskLevel.Critical)
            .Select(ToResponse)
            .ToList();

        return Ok(latestCriticalPerPatient);
    }

    private static VitalSignResponse ToResponse(VitalSign v) => new(
        v.Id, v.PatientId, v.HeartRateBpm, v.SystolicBp, v.DiastolicBp,
        v.OxygenSaturationPercent, v.RecordedAtUtc, v.RiskLevel);
}
