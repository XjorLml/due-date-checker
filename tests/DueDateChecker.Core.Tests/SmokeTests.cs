namespace DueDateChecker.Core.Tests;

// S-1 only: proves `dotnet test` runs end to end on the laptop (and later in CI).
// Delete when the first real domain test (Money) lands in S0.
public class SmokeTests
{
    [Fact]
    public void Test_runner_executes()
    {
        Assert.True(true);
    }
}
