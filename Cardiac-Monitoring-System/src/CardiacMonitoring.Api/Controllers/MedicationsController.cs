using CardiacMonitoring.Api.DTOs.Medications;
using CardiacMonitoring.Api.Entities;
using CardiacMonitoring.Api.Repositories;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
namespace CardiacMonitoring.Api.Controllers; 
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[EnableRateLimiting("general")]
public class MedicationsController : ControllerBase
{
    private readonly IRepository<Medication> _repository;
private readonly IValidator<CreateMedicationRequest> _validator;

public MedicationsController(IRepository<Medication> repository, IValidator<CreateMedicationRequest> validator)
{
    _repository = repository;
    _validator = validator;
}

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var medications = await _repository.GetAllAsync();
        return Ok(medications.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var medication = await _repository.GetByIdAsync(id);
        if (medication is null)
            return NotFound();

        return Ok(ToResponse(medication));
    }

    [Authorize(Roles = "Doctor")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateMedicationRequest request)
{
    var validationResult = await _validator.ValidateAsync(request);
    if (!validationResult.IsValid)
        return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

    var medication = new Medication
        {
            PatientId = request.PatientId,
            Name = request.Name,
            DosageMg = request.DosageMg,
            Frequency = request.Frequency
        };

        await _repository.AddAsync(medication);
        await _repository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = medication.Id }, ToResponse(medication));
    }

    [Authorize(Roles = "Doctor")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateMedicationRequest request)
    {
        var medication = await _repository.GetByIdAsync(id);
        if (medication is null)
            return NotFound();

        medication.Name = request.Name;
        medication.DosageMg = request.DosageMg;
        medication.Frequency = request.Frequency;

        _repository.Update(medication);
        await _repository.SaveChangesAsync();

        return Ok(ToResponse(medication));
    }

    [Authorize(Roles = "Doctor")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var medication = await _repository.GetByIdAsync(id);
        if (medication is null)
            return NotFound();

        _repository.Remove(medication);
        await _repository.SaveChangesAsync();
        return NoContent();
    }

    private static MedicationResponse ToResponse(Medication m) =>
        new(m.Id, m.PatientId, m.Name, m.DosageMg, m.Frequency);
}
