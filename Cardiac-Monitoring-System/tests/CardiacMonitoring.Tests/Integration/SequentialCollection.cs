using Xunit;

namespace CardiacMonitoring.Tests.Integration;

// Marker class for xUnit's collection-fixture mechanism — any test class
// decorated with [Collection("Sequential")] runs its tests sequentially
// relative to other classes in the same named collection, rather than in
// parallel with them. Used here because several RBAC tests promote users
// via the shared seeded Admin account and the rate-limited login endpoint
// (5 requests/minute), so parallel execution risks tripping that limiter.
[CollectionDefinition("Sequential", DisableParallelization = true)]
public class SequentialCollection
{
}
