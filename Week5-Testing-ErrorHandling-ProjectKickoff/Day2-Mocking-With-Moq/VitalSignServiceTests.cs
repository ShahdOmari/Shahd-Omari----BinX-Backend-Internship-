using CardiacMonitoring.Api.DTOs.VitalSigns;
using CardiacMonitoring.Api.Entities;
using CardiacMonitoring.Api.Repositories;
using CardiacMonitoring.Api.Services;
using Moq;
using Xunit;

namespace CardiacMonitoring.Tests.Services;

public class VitalSignServiceTests
{
    private readonly Mock<IRepository<VitalSign>> _repositoryMock = new();
    private readonly Mock<IRiskEvaluator> _riskEvaluatorMock = new();
    private readonly VitalSignService _service;

    public VitalSignServiceTests()
    {
        // The service under test never touches a real repository or a real
        // risk evaluator — both dependencies are replaced with mocks, so
        // this test is purely about VitalSignService's own coordination
        // logic, not the database or the scoring rules themselves (those
        // already have their own dedicated tests from Day 1).
        _service = new VitalSignService(_repositoryMock.Object, _riskEvaluatorMock.Object);
    }

    [Fact]
    public async Task RecordReadingAsync_AssignsRiskLevel_FromEvaluator()
    {
        // Arrange
        var request = new CreateVitalSignRequest(
            PatientId: 1, HeartRateBpm: 145, SystolicBp: 190,
            DiastolicBp: 100, OxygenSaturationPercent: 85);

        _riskEvaluatorMock
            .Setup(e => e.Evaluate(It.IsAny<VitalSign>()))
            .Returns(RiskLevel.Critical);

        // Act
        var result = await _service.RecordReadingAsync(request);

        // Assert
        Assert.Equal(RiskLevel.Critical, result.RiskLevel);
    }

    [Fact]
    public async Task RecordReadingAsync_SavesExactlyOnce()
    {
        var request = new CreateVitalSignRequest(1, 75, 120, 80, 98);

        _riskEvaluatorMock
            .Setup(e => e.Evaluate(It.IsAny<VitalSign>()))
            .Returns(RiskLevel.Normal);

        await _service.RecordReadingAsync(request);

        // Verify confirms the repository was actually asked to persist the
        // reading — a bug that skipped or duplicated the save call would
        // pass a naive "does it return the right value" test but fail this.
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<VitalSign>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RecordReadingAsync_PassesCorrectPatientIdToRepository()
    {
        var request = new CreateVitalSignRequest(
            PatientId: 42, HeartRateBpm: 75, SystolicBp: 120,
            DiastolicBp: 80, OxygenSaturationPercent: 98);

        _riskEvaluatorMock
            .Setup(e => e.Evaluate(It.IsAny<VitalSign>()))
            .Returns(RiskLevel.Normal);

        VitalSign? capturedEntity = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<VitalSign>()))
            .Callback<VitalSign>(v => capturedEntity = v)
            .Returns(Task.CompletedTask);

        await _service.RecordReadingAsync(request);

        Assert.NotNull(capturedEntity);
        Assert.Equal(42, capturedEntity!.PatientId);
    }
}
