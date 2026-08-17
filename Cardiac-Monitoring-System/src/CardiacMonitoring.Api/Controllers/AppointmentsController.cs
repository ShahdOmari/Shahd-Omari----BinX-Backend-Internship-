using CardiacMonitoring.Api.DTOs.Appointments;
using CardiacMonitoring.Api.Entities;
using CardiacMonitoring.Api.Repositories;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Authorization; 
using FluentValidation;

namespace CardiacMonitoring.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IRepository<Appointment> _repository;
    private readonly IValidator<CreateAppointmentRequest> _validator;

    public AppointmentsController(IRepository<Appointment> repository, IValidator<CreateAppointmentRequest> validator)
    {
    _repository = repository;
    _validator = validator;
    }

    // GET /api/v1/Appointments?upcomingOnly=true
    // A simple LINQ Where filter driven by an optional query parameter —
    // demonstrates filtering/search on top of the generic repository
    // without needing a dedicated repository method for every filter shape.
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool upcomingOnly = false)
    {
        var appointments = await _repository.GetAllAsync();

        var filtered = upcomingOnly
            ? appointments.Where(a => a.ScheduledAtUtc >= DateTime.UtcNow)
            : appointments;

        return Ok(filtered.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _repository.GetByIdAsync(id);
        if (appointment is null)
            return NotFound();

        return Ok(ToResponse(appointment));
    }

    [HttpPost]
public async Task<IActionResult> Create(CreateAppointmentRequest request)
{
    var validationResult = await _validator.ValidateAsync(request);
    if (!validationResult.IsValid)
        return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

    var appointment = new Appointment
        {
            PatientId = request.PatientId,
            ScheduledAtUtc = request.ScheduledAtUtc,
            DoctorName = request.DoctorName,
            Reason = request.Reason
        };

        await _repository.AddAsync(appointment);
        await _repository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, ToResponse(appointment));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateAppointmentRequest request)
    {
        var appointment = await _repository.GetByIdAsync(id);
        if (appointment is null)
            return NotFound();

        appointment.ScheduledAtUtc = request.ScheduledAtUtc;
        appointment.DoctorName = request.DoctorName;
        appointment.Reason = request.Reason;

        _repository.Update(appointment);
        await _repository.SaveChangesAsync();

        return Ok(ToResponse(appointment));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var appointment = await _repository.GetByIdAsync(id);
        if (appointment is null)
            return NotFound();

        _repository.Remove(appointment);
        await _repository.SaveChangesAsync();
        return NoContent();
    }

    private static AppointmentResponse ToResponse(Appointment a) =>
        new(a.Id, a.PatientId, a.ScheduledAtUtc, a.DoctorName, a.Reason);
}
