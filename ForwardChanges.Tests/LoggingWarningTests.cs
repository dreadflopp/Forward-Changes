using Xunit;

namespace ForwardChanges.Tests;

public sealed class LoggingWarningTests
{
    [Fact]
    public void WarningBypassesSummaryVerbosityFiltering()
    {
        LogCollector.Clear();
        LogCollector.SetRecordLoggingContext(deepDiveRecord: false, detailedRecord: false);

        LogCollector.Add("Placement", "[Warning] formatter needs attention");

        Assert.Equal(1, LogCollector.GetTotalCount());
        Assert.Contains(LogCollector.GetAll(), line => line.Contains("[Warning]", StringComparison.Ordinal));
        LogCollector.Clear();
    }

    [Fact]
    public void OrdinaryDetailRemainsSuppressedInSummaryMode()
    {
        LogCollector.Clear();
        LogCollector.SetRecordLoggingContext(deepDiveRecord: false, detailedRecord: false);

        LogCollector.Add("Placement", "ordinary detailed trace");

        Assert.Equal(0, LogCollector.GetTotalCount());
        LogCollector.Clear();
    }
}
