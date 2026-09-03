using CardiacMonitoring.Api.DTOs.Patients;
using CardiacMonitoring.Api.Entities;
using CardiacMonitoring.Api.Repositories;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Authorization; 
using FluentValidation;
using Microsoft.AspNetCore.RateLimiting; 
using CardiacMonitoring.Api.DTOs.Common;
using Microsoft.EntityFrameworkCore;
namespace CardiacMonitoring.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[EnableRateLimiting("general")]
public class PatientsController : ControllerBase
{
    private readonly IRepository<Patient> _repository;
    private readonly IValidator<CreatePatientRequest> _validator;

    public PatientsController(IRepository<Patient> repository, IValidator<CreatePatientRequest> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    // GET /api/v1/Patients?page=1&pageSize=10&gender=Female&minAge=40&sortBy=name&sortDirection=asc
    //
    // Open to any authenticated staff member (Nurse, Doctor, Auditor,
    // Admin) — browsing the patient roster is routine day-to-day work,
    // not a privileged action. Sprint 1, Day 3: paginated so it stays
    // fast regardless of how many patients exist, with optional filters
    // applied conditionally (only when actually supplied), and projected
    // to a DTO rather than exposing the Patient entity directly.
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? gender = null,
        [FromQuery] int? minAge = null,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortDirection = "asc")
    {
        // Defensive bounds — a client-supplied page/pageSize of 0 or a huge
        // number shouldn't be able to break the query or return everything.
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

        var query = _repository.Query();

        // Each filter is applied only when the caller actually supplied it —
        // the same endpoint serves both an unfiltered browse and a narrowed
        // search, rather than needing a separate endpoint per filter combination.
        if (!string.IsNullOrWhiteSpace(gender))
            query = query.Where(p => p.Gender == gender);

        if (minAge.HasValue)
        {
            var cutoffDate = DateTime.UtcNow.AddYears(-minAge.Value);
            query = query.Where(p => p.DateOfBirth <= cutoffDate);
        }

        // Sorting: only two supported fields, explicit and predictable rather
        // than accepting an arbitrary raw column name from the client (which
        // would risk exposing internal schema details or enabling injection
        // if ever built as raw SQL).
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
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var patient = await _repository.GetByIdAsync(id);
        if (patient is null)
            return NotFound();

        var response = new PatientResponse(patient.Id, patient.FullName, patient.DateOfBirth, patient.Gender);
        return Ok(response);
    }

    // Any authenticated staff member can register a new patient into the
    // system — this is routine intake work, most commonly done by a Nurse.
    [HttpPost]
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

        var response = new PatientResponse(patient.Id, patient.FullName, patient.DateOfBirth, patient.Gender);
        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, response);
    }

    [HttpPut("{id:int}")]
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

        var response = new PatientResponse(patient.Id, patient.FullName, patient.DateOfBirth, patient.Gender);
        return Ok(response);
    }

    // Deleting a patient record entirely is irreversible and affects every
    // linked reading, medication and appointment — restricted to Doctor
    // and Admin, never a routine Nurse action.
    [Authorize(Roles = "Doctor,Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var patient = await _repository.GetByIdAsync(id);
        if (patient is null)
            return NotFound();

        _repository.Remove(patient);
        await _repository.SaveChangesAsync();
        return NoContent();
    }
}
