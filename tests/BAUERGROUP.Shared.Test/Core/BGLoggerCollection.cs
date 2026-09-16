namespace BAUERGROUP.Shared.Test.Core;

/// <summary>
/// Tests that touch <c>BGLogger.Configuration</c> or <c>LogManager</c> share process-global state,
/// so they must never run in parallel with each other or with any other collection.
/// </summary>
[CollectionDefinition("BGLogger", DisableParallelization = true)]
public sealed class BGLoggerCollection
{
}
