using CardiacMonitoring.Api.Entities;
using CardiacMonitoring.Api.Services;
using Xunit;

namespace CardiacMonitoring.Tests.Services;

public class CardiacRiskEvaluatorTests
{
    private readonly CardiacRiskEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_ReturnsNormal_ForHealthyReading()
    {
        // Arrange
        var reading = new VitalSign
        {
            HeartRateBpm = 75,
            SystolicBp = 120,
            DiastolicBp = 80,
            OxygenSaturationPercent = 98
        };

        // Act
        var result = _evaluator.Evaluate(reading);

        // Assert
        Assert.Equal(RiskLevel.Normal, result);
    }

    [Fact]
    public void Evaluate_ReturnsCritical_WhenOxygenSaturationIsDangerouslyLow()
    {
        var reading = new VitalSign
        {
            HeartRateBpm = 80,
            SystolicBp = 120,
            DiastolicBp = 80,
            OxygenSaturationPercent = 85 // below the critical threshold of 90
        };

        var result = _evaluator.Evaluate(reading);

        Assert.Equal(RiskLevel.Critical, result);
    }

    [Fact]
    public void Evaluate_ReturnsCritical_WhenHeartRateIsDangerouslyHigh()
    {
        var reading = new VitalSign
        {
            HeartRateBpm = 145, // above the critical threshold of 130
            SystolicBp = 120,
            DiastolicBp = 80,
            OxygenSaturationPercent = 98
        };

        var result = _evaluator.Evaluate(reading);

        Assert.Equal(RiskLevel.Critical, result);
    }

    [Fact]
    public void Evaluate_ReturnsWatch_WhenHeartRateIsElevatedButNotCritical()
    {
        var reading = new VitalSign
        {
            HeartRateBpm = 110, // between Watch (100) and Critical (130) thresholds
            SystolicBp = 120,
            DiastolicBp = 80,
            OxygenSaturationPercent = 98
        };

        var result = _evaluator.Evaluate(reading);

        Assert.Equal(RiskLevel.Watch, result);
    }

    // A [Theory] test covers several heart-rate boundary values in one
    // method instead of writing a near-identical [Fact] for each — useful
    // here since the thresholds themselves are the exact thing most likely
    // to be changed (and accidentally broken) later.
    [Theory]
    [InlineData(39, RiskLevel.Critical)]   // just below the low critical cutoff
    [InlineData(40, RiskLevel.Watch)]      // exactly on the low critical boundary
    [InlineData(100, RiskLevel.Normal)]    // exactly on the high watch boundary
    [InlineData(101, RiskLevel.Watch)]     // just above it
    [InlineData(130, RiskLevel.Watch)]     // exactly on the high critical boundary
    [InlineData(131, RiskLevel.Critical)]  // just above it
    public void Evaluate_ClassifiesHeartRate_AtEachBoundary(int heartRate, RiskLevel expected)
    {
        var reading = new VitalSign
        {
            HeartRateBpm = heartRate,
            SystolicBp = 120,
            DiastolicBp = 80,
            OxygenSaturationPercent = 98
        };

        var result = _evaluator.Evaluate(reading);

        Assert.Equal(expected, result);
    }
}
