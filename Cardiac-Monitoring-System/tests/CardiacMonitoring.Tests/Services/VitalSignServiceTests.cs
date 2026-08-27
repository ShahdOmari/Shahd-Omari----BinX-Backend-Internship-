using CardiacMonitoring.Api.Data;
using CardiacMonitoring.Api.DTOs.VitalSigns;
using CardiacMonitoring.Api.Entities;
using CardiacMonitoring.Api.Repositories;
using CardiacMonitoring.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CardiacMonitoring.Tests.Services;

// VitalSignService now needs a transaction (Database.BeginTransactionAsync),
// which only works against a real DbContext/connection — not something Moq
// can fake. A lightweight SQLite in-memory AppDbContext is used here instead,
// while IRiskEvaluator stays mocked, since that part genuinely has no
// database dependency and isolating it is still valuable.
public class VitalSignServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly Mock<IRiskEvaluator> _riskEvaluatorMock = new();
    private readonly VitalSignService _service;

    public VitalSignServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        var vitalSignRepository = new Repository<VitalSign>(_context);
        var appointmentRepository = new Repository<Appointment>(_context);

        _service = new VitalSignService(
            vitalSignRepository, appointmentRepository, _riskEvaluatorMock.Object, _context);
    }

    [Fact]
    public async Task RecordReadingAsync_AssignsRiskLevel_FromEvaluator()
    {
        var patient = new Patient { FullName = "Test Patient", DateOfBirth = new DateTime(1980, 1, 1), Gender = "Female" };
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        var request = new CreateVitalSignRequest(patient.Id, 145, 190, 100, 85);

        _riskEvaluatorMock.Setup(e => e.Evaluate(It.IsAny<VitalSign>())).Returns(RiskLevel.Critical);

        var result = await _service.RecordReadingAsync(request);

        Assert.Equal(RiskLevel.Critical, result.RiskLevel);
    }

    [Fact]
    public async Task RecordReadingAsync_SavesExactlyOnce()
    {
        var patient = new Patient { FullName = "Test Patient", DateOfBirth = new DateTime(1980, 1, 1), Gender = "Female" };
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        var request = new CreateVitalSignRequest(patient.Id, 75, 120, 80, 98);
        _riskEvaluatorMock.Setup(e => e.Evaluate(It.IsAny<VitalSign>())).Returns(RiskLevel.Normal);

        await _service.RecordReadingAsync(request);

        var savedCount = await _context.VitalSigns.CountAsync(v => v.PatientId == patient.Id);
        Assert.Equal(1, savedCount);
    }

    [Fact]
    public async Task RecordReadingAsync_PassesCorrectPatientIdToRepository()
    {
        var patient = new Patient { FullName = "Test Patient", DateOfBirth = new DateTime(1980, 1, 1), Gender = "Female" };
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        var request = new CreateVitalSignRequest(patient.Id, 75, 120, 80, 98);
        _riskEvaluatorMock.Setup(e => e.Evaluate(It.IsAny<VitalSign>())).Returns(RiskLevel.Normal);

        await _service.RecordReadingAsync(request);

        var saved = await _context.VitalSigns.FirstOrDefaultAsync(v => v.PatientId == patient.Id);
        Assert.NotNull(saved);
        Assert.Equal(patient.Id, saved!.PatientId);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
