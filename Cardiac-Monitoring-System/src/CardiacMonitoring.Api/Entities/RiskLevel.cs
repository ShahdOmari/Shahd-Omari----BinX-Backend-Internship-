namespace CardiacMonitoring.Api.Entities;

// Represents how urgently a recorded vital sign needs clinical attention.
// Kept as a simple enum (not a class) since it has no behavior of its own —
// the *rules* that decide the level live in a separate service
// (CardiacRiskEvaluator), not here. The entity just stores the outcome.
public enum RiskLevel
{
    Normal,
    Watch,
    Critical
}
