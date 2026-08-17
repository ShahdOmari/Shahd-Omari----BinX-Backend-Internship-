using CardiacMonitoring.Api.DTOs.Patients;
using CardiacMonitoring.Api.Entities;
using CardiacMonitoring.Api.Repositories;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Authorization; 
using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;
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
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var patients = await _repository.GetAllAsync();

        // Projecting entities to response DTOs with LINQ's Select — never
        // return EF Core entities directly, since that risks leaking
        // internal fields or navigation properties the client shouldn't see.
        var response = patients.Select(p =>
            new PatientResponse(p.Id, p.FullName, p.DateOfBirth, p.Gender));

        return Ok(response);
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

    [Authorize(Roles = "Doctor")]
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
