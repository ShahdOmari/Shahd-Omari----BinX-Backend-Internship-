using CardiacMonitoring.Api.Data;
using CardiacMonitoring.Api.DTOs.VitalSigns;
using CardiacMonitoring.Api.Entities;
using CardiacMonitoring.Api.Repositories;
using CardiacMonitoring.Api.Services;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Authorization; 
using FluentValidation; 
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

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
    private readonly AppDbContext _context;

    public VitalSignsController(
        IRepository<VitalSign> repository,
        IVitalSignService vitalSignService,
        IValidator<CreateVitalSignRequest> validator,
        AppDbContext context)
    {
        _repository = repository;
        _vitalSignService = vitalSignService;
        _validator = validator;
        _context = context;
    }

    private int? CurrentStaffProfileId =>
        int.TryParse(User.FindFirst("staffProfileId")?.Value, out var id) ? id : null;

    [Authorize(Roles = "Doctor,Auditor,Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Sprint 3 Day 2: projection — joins VitalSigns with Patients in
        // one SQL query so the caller gets the patient name without a
        // second round-trip. Only the columns the response actually needs
        // cross the wire; the full Patient entity is never materialised.
        var vitals = await _context.VitalSigns
            .AsNoTracking()
            .Join(_context.Patients,
                v => v.PatientId,
                p => p.Id,
                (v, p) => new VitalSignWithPatientResponse(
                    v.Id, v.PatientId, p.FullName,
                    v.HeartRateBpm, v.SystolicBp, v.DiastolicBp,
                    v.OxygenSaturationPercent, v.RecordedAtUtc, v.RiskLevel))
            .ToListAsync();

        return Ok(vitals);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var vital = await _repository.GetByIdAsync(id);
        if (vital is null)
            return NotFound();

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

    // Sprint 3, Day 2: fixed the Full Table Load anti-pattern.
    //
    // BEFORE: GetAllAsync() pulled all 300 rows into RAM, then C# did
    // the GroupBy/Where. SQL generated: SELECT * FROM VitalSigns (no WHERE).
    //
    // AFTER: two-step approach that EF Core 10 can fully translate to SQL:
    // Step 1 — filter to Critical rows only (WHERE RiskLevel = 'Critical').
    // Step 2 — for each PatientId, keep only the row with the MAX RecordedAtUtc
    //           using a correlated subquery EF Core translates correctly.
    // Result: typically 7-15 rows returned instead of 300.
    // Query count: still 1. Row count reduction: ~97%.
    [Authorize(Roles = "Doctor,Admin")]
    [HttpGet("critical")]
    public async Task<IActionResult> GetCriticalPatients()
    {
        // Subquery: find the latest RecordedAtUtc per PatientId among
        // Critical readings. EF Core translates this to a correlated
        // subquery with WHERE + MAX(), which SQL Server executes as an
        // efficient index seek once the (RiskLevel, PatientId, RecordedAtUtc)
        // index is in place (added in Day 4).
        var latestCriticalPerPatient = await _context.VitalSigns
            .AsNoTracking()
            .Where(v => v.RiskLevel == RiskLevel.Critical)
            .Where(v => v.RecordedAtUtc == _context.VitalSigns
                .Where(inner => inner.PatientId == v.PatientId
                             && inner.RiskLevel == RiskLevel.Critical)
                .Max(inner => inner.RecordedAtUtc))
            .Select(v => new VitalSignResponse(
                v.Id, v.PatientId, v.HeartRateBpm, v.SystolicBp, v.DiastolicBp,
                v.OxygenSaturationPercent, v.RecordedAtUtc, v.RiskLevel))
            .ToListAsync();

        return Ok(latestCriticalPerPatient);
    }

    private static VitalSignResponse ToResponse(VitalSign v) => new(
        v.Id, v.PatientId, v.HeartRateBpm, v.SystolicBp, v.DiastolicBp,
        v.OxygenSaturationPercent, v.RecordedAtUtc, v.RiskLevel);
}
