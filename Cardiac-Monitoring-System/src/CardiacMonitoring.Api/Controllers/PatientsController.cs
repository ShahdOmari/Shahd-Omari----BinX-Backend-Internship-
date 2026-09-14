using CardiacMonitoring.Api.DTOs.Patients;
using CardiacMonitoring.Api.Entities;
using CardiacMonitoring.Api.Repositories;
using CardiacMonitoring.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;
using CardiacMonitoring.Api.DTOs.Common;
using Microsoft.EntityFrameworkCore;

namespace CardiacMonitoring.Api.Controllers;

/// <summary>
/// Manages cardiac patient records.
/// All authenticated staff can read and create patients; only Doctor and Admin can delete.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[EnableRateLimiting("general")]
public class PatientsController : ControllerBase
{
    private readonly IRepository<Patient> _repository;
    private readonly IValidator<CreatePatientRequest> _validator;
    private readonly ICacheService _cache;

    public PatientsController(
        IRepository<Patient> repository,
        IValidator<CreatePatientRequest> validator,
        ICacheService cache)
    {
        _repository = repository;
        _validator = validator;
        _cache = cache;
    }

    /// <summary>
    /// Returns a paginated, filterable list of all patients.
    /// </summary>
    /// <remarks>
    /// Results are cached in Redis for 10 minutes. The cache is invalidated automatically
    /// whenever a patient is created, updated, or deleted.
    ///
    /// Supported sort fields: name (default), age.
    /// Supported sort directions: asc (default), desc.
    /// </remarks>
    /// <param name="page">Page number, 1-based. Defaults to 1.</param>
    /// <param name="pageSize">Items per page, 1-100. Defaults to 10.</param>
    /// <param name="gender">Optional filter: Male or Female.</param>
    /// <param name="minAge">Optional filter: minimum age in years.</param>
    /// <param name="sortBy">Sort field: name or age.</param>
    /// <param name="sortDirection">Sort direction: asc or desc.</param>
    /// <response code="200">Paginated patient list with total count.</response>
    /// <response code="401">No valid JWT token provided.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PatientResponse>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? gender = null,
        [FromQuery] int? minAge = null,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortDirection = "asc")
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

        var cacheKey = CacheKeys.Patients(page, pageSize, gender, minAge, sortBy, sortDirection);
        var cached = await _cache.GetAsync<PagedResult<PatientResponse>>(cacheKey);
        if (cached is not null)
            return Ok(cached);

        var query = _repository.Query();

        if (!string.IsNullOrWhiteSpace(gender))
            query = query.Where(p => p.Gender == gender);

        if (minAge.HasValue)
        {
            var cutoffDate = DateTime.UtcNow.AddYears(-minAge.Value);
            query = query.Where(p => p.DateOfBirth <= cutoffDate);
        }

        query = (sortBy.ToLower(), sortDirection.ToLower()) switch
        {
            ("age", "desc") => query.OrderBy(p => p.DateOfBirth),
            ("age", _) => query.OrderByDescending(p => p.DateOfBirth),
            ("name", "desc") => query.OrderByDescending(p => p.FullName),
            _ => query.OrderBy(p => p.FullName),
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PatientResponse(p.Id, p.FullName, p.DateOfBirth, p.Gender))
            .ToListAsync();

        var result = new PagedResult<PatientResponse>(items, page, pageSize, totalCount);
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
        return Ok(result);
    }

    /// <summary>
    /// Returns a single patient by ID.
    /// </summary>
    /// <param name="id">The patient ID.</param>
    /// <response code="200">The requested patient record.</response>
    /// <response code="401">No valid JWT token provided.</response>
    /// <response code="404">No patient found with the given ID.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PatientResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var patient = await _repository.GetByIdAsync(id);
        if (patient is null)
            return NotFound();
        return Ok(new PatientResponse(patient.Id, patient.FullName, patient.DateOfBirth, patient.Gender));
    }

    /// <summary>
    /// Registers a new patient in the system.
    /// </summary>
    /// <remarks>
    /// Available to all authenticated staff. Typically performed by Nurses during intake.
    ///
    /// Example request:
    ///
    ///     POST /api/v1/Patients
    ///     {
    ///         "fullName": "Layla Ahmad",
    ///         "dateOfBirth": "1968-04-12",
    ///         "gender": "Female"
    ///     }
    ///
    /// </remarks>
    /// <param name="request">The new patient's details.</param>
    /// <response code="201">Patient created. Returns the new patient record including assigned ID.</response>
    /// <response code="400">Validation failed — FullName is required, DateOfBirth must be in the past.</response>
    /// <response code="401">No valid JWT token provided.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PatientResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Create(CreatePatientRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

        var patient = new Patient
        {
            FullName = request.FullName,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender
        };

        await _repository.AddAsync(patient);
        await _repository.SaveChangesAsync();
        await _cache.RemoveAsync(CacheKeys.PatientsBase);

        return CreatedAtAction(nameof(GetById), new { id = patient.Id },
            new PatientResponse(patient.Id, patient.FullName, patient.DateOfBirth, patient.Gender));
    }

    /// <summary>
    /// Updates an existing patient's details.
    /// </summary>
    /// <param name="id">The ID of the patient to update.</param>
    /// <param name="request">The updated patient details.</param>
    /// <response code="200">Patient updated successfully.</response>
    /// <response code="401">No valid JWT token provided.</response>
    /// <response code="404">No patient found with the given ID.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PatientResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, CreatePatientRequest request)
    {
        var patient = await _repository.GetByIdAsync(id);
        if (patient is null)
            return NotFound();

        patient.FullName = request.FullName;
        patient.DateOfBirth = request.DateOfBirth;
        patient.Gender = request.Gender;

        _repository.Update(patient);
        await _repository.SaveChangesAsync();
        await _cache.RemoveAsync(CacheKeys.PatientsBase);

        return Ok(new PatientResponse(patient.Id, patient.FullName, patient.DateOfBirth, patient.Gender));
    }

    /// <summary>
    /// Permanently deletes a patient and all linked records (cascade).
    /// </summary>
    /// <remarks>
    /// This action is irreversible. All linked VitalSigns, Medications, and Appointments
    /// are deleted automatically via cascade. Restricted to Doctor and Admin.
    /// </remarks>
    /// <param name="id">The ID of the patient to delete.</param>
    /// <response code="204">Patient deleted successfully.</response>
    /// <response code="401">No valid JWT token provided.</response>
    /// <response code="403">Caller does not have Doctor or Admin role.</response>
    /// <response code="404">No patient found with the given ID.</response>
    [Authorize(Roles = "Doctor,Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var patient = await _repository.GetByIdAsync(id);
        if (patient is null)
            return NotFound();

        _repository.Remove(patient);
        await _repository.SaveChangesAsync();
        await _cache.RemoveAsync(CacheKeys.PatientsBase);
        return NoContent();
    }
}
