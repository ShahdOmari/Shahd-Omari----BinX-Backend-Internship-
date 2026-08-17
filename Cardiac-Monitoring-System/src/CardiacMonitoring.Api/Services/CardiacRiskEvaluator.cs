using CardiacMonitoring.Api.Entities;

namespace CardiacMonitoring.Api.Services;

// Pure, stateless threshold logic — deliberately simple and explainable
// clinical thresholds, since a student prototype is not the place for
// unvalidated medical modeling. Easy to point to and explain in a demo.
public class CardiacRiskEvaluator : IRiskEvaluator
{
    public RiskLevel Evaluate(VitalSign v)
    {
        bool isCritical = v.HeartRateBpm > 130 || v.HeartRateBpm < 40
            || v.SystolicBp > 180 || v.SystolicBp < 80
            || v.OxygenSaturationPercent < 90;

        bool isWatch = v.HeartRateBpm is > 100 or < 50
            || v.SystolicBp is > 140 or < 90
            || v.OxygenSaturationPercent < 95;

        return isCritical ? RiskLevel.Critical
             : isWatch    ? RiskLevel.Watch
             : RiskLevel.Normal;
    }
}
