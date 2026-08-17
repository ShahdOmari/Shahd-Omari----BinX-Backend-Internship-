using CardiacMonitoring.Api.Entities;

namespace CardiacMonitoring.Api.Services;

// Interface, not a concrete class directly — lets a different scoring
// strategy (e.g. age-adjusted thresholds) be swapped in later without
// touching any controller that depends on this abstraction. Same
// interface-based polymorphism principle from Week 1's OOP lesson.
public interface IRiskEvaluator
{
    RiskLevel Evaluate(VitalSign vitalSign);
}
