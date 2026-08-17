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

        return Ok(ToResponse(vital));
    }

[HttpPost]
public async Task<IActionResult> Create(CreateVitalSignRequest request)
{
    var validationResult = await _validator.ValidateAsync(request);
    if (!validationResult.IsValid)
        return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

    var response = await _vitalSignService.RecordReadingAsync(request);
    return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
}
    // GET /api/v1/VitalSigns/critical
    // Returns the single most recent reading per patient, filtered to only
    // patients whose latest reading is currently Critical — a genuinely
    // useful "who needs attention right now" view, built with LINQ's
    // GroupBy + OrderByDescending + First over data already fetched async
    // from the database (Week 2's grouping lesson, applied for real).
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
