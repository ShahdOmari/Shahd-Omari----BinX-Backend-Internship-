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

/// <summary>
/// Manages vital sign readings for cardiac patients.
/// Nurses record readings; Doctors and Admins view aggregated data.
/// </summary>
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

    /// <summary>
    /// Returns all vital sign readings across all patients.
    /// </summary>
    /// <remarks>
    /// Restricted to Doctor, Auditor, and Admin — Nurses use GET /VitalSigns/{id}
    /// to access their own recorded readings only.
    ///
    /// Each item includes the patient name via a single SQL JOIN — no N+1 queries.
    /// </remarks>
    /// <response code="200">List of all vital sign readings with patient names.</response>
    /// <response code="401">No valid JWT token provided.</response>
    /// <response code="403">Caller has Nurse role — use GET /VitalSigns/{id} instead.</response>
    [Authorize(Roles = "Doctor,Auditor,Admin")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VitalSignWithPatientResponse>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetAll()
    {
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

    /// <summary>
    /// Returns a single vital sign reading by ID.
    /// </summary>
    /// <remarks>
    /// Nurses may only retrieve readings they personally recorded (ownership check).
    /// Doctors, Auditors, and Admins can retrieve any reading.
    /// </remarks>
    /// <param name="id">The ID of the vital sign reading.</param>
    /// <response code="200">The requested vital sign reading.</response>
    /// <response code="401">No valid JWT token provided.</response>
    /// <response code="403">Nurse is authenticated but this reading was recorded by a different nurse.</response>
    /// <response code="404">No reading found with the given ID.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VitalSignResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
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

    /// <summary>
    /// Records a new vital sign reading for a patient.
    /// </summary>
    /// <remarks>
    /// Available to all authenticated staff. The reading is automatically linked
    /// to the calling staff member's profile via the JWT staffProfileId claim —
    /// no extra database lookup required.
    ///
    /// Risk level (Normal / Watch / Critical) is computed automatically by the
    /// cardiac risk evaluation engine based on the supplied measurements.
    ///
    /// Example request:
    ///
    ///     POST /api/v1/VitalSigns
    ///     {
    ///         "patientId": 1,
    ///         "heartRateBpm": 88,
    ///         "systolicBp": 135,
    ///         "diastolicBp": 85,
    ///         "oxygenSaturationPercent": 96.5
    ///     }
    ///
    /// </remarks>
    /// <param name="request">The vital sign measurements to record.</param>
    /// <response code="201">Reading recorded successfully. Returns the created reading including computed risk level.</response>
    /// <response code="400">Validation failed — check heartRateBpm range (30-220), BP ranges, or oxygen saturation (70-100).</response>
    /// <response code="401">No valid JWT token provided.</response>
    [HttpPost]
    [ProducesResponseType(typeof(VitalSignResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Create(CreateVitalSignRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        var response = await _vitalSignService.RecordReadingAsync(request, CurrentStaffProfileId);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Returns the latest Critical-risk reading per patient across the entire system.
    /// </summary>
    /// <remarks>
    /// Restricted to Doctor and Admin — this is the primary triage dashboard endpoint.
    ///
    /// Uses a correlated subquery to find the most recent Critical reading per patient
    /// entirely in SQL — only the qualifying rows cross the network (typically 5-15),
    /// never the full readings table.
    /// </remarks>
    /// <response code="200">List of the latest Critical reading for each affected patient.</response>
    /// <response code="401">No valid JWT token provided.</response>
    /// <response code="403">Caller does not have Doctor or Admin role.</response>
    [Authorize(Roles = "Doctor,Admin")]
    [HttpGet("critical")]
    [ProducesResponseType(typeof(IEnumerable<VitalSignResponse>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetCriticalPatients()
    {
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
