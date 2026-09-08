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

    [HttpGet]
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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var patient = await _repository.GetByIdAsync(id);
        if (patient is null)
            return NotFound();

        return Ok(new PatientResponse(patient.Id, patient.FullName, patient.DateOfBirth, patient.Gender));
    }

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
        await _cache.RemoveAsync(CacheKeys.PatientsBase);

        return CreatedAtAction(nameof(GetById), new { id = patient.Id },
            new PatientResponse(patient.Id, patient.FullName, patient.DateOfBirth, patient.Gender));
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
        await _cache.RemoveAsync(CacheKeys.PatientsBase);

        return Ok(new PatientResponse(patient.Id, patient.FullName, patient.DateOfBirth, patient.Gender));
    }

    [Authorize(Roles = "Doctor,Admin")]
    [HttpDelete("{id:int}")]
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
