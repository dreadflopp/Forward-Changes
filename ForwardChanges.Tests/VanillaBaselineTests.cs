using Mutagen.Bethesda.Plugins;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class VanillaBaselineTests
{
    [Fact]
    public void InitializesFromOnlyPluginsPresentInLoadOrder()
    {
        var skyrim = ModKey.FromNameAndExtension("Skyrim.esm");
        var vr = ModKey.FromNameAndExtension("SkyrimVR.esm");
        var creation = ModKey.FromNameAndExtension("ccTest.esl");

        var summary = Utility.InitializeVanillaMods(
            [skyrim, creation],
            [creation],
            includeCreationClub: true);

        Assert.True(Utility.IsVanilla(skyrim));
        Assert.True(Utility.IsVanilla(creation));
        Assert.False(Utility.IsVanilla(vr));
        Assert.Equal(2, summary.PresentCount);
    }

    [Fact]
    public void CanTreatCreationClubAsNormalMods()
    {
        var skyrim = ModKey.FromNameAndExtension("Skyrim.esm");
        var creation = ModKey.FromNameAndExtension("ccTest.esl");

        var summary = Utility.InitializeVanillaMods(
            [skyrim, creation],
            [creation],
            includeCreationClub: false);

        Assert.True(Utility.IsVanilla(skyrim));
        Assert.False(Utility.IsVanilla(creation));
        Assert.False(summary.IncludesCreationClub);
    }
}
