using Mutagen.Bethesda.Plugins;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class VirtualMasterRuleTests
{
    [Fact]
    public void RuleGrantsOnlyConfiguredTargetVirtualMasterPermission()
    {
        var unofficialPatch = ModKey.FromNameAndExtension("Unofficial Skyrim Special Edition Patch.esp");
        var simonMod = ModKey.FromNameAndExtension("MysticismMagic.esp");
        var unrelatedMod = ModKey.FromNameAndExtension("Unrelated.esp");
        var configuration = new PatcherConfiguration
        {
            CompatibilityRules =
            [
                new VirtualMasterRule
                {
                    InjectedMaster = unofficialPatch.FileName.String,
                    TargetMods = [simonMod.FileName.String]
                }
            ]
        };

        PatcherSettings.Apply(configuration);

        Assert.True(PatcherSettings.HasMasterOrVirtualMaster(simonMod, [], unofficialPatch));
        Assert.False(PatcherSettings.HasMasterOrVirtualMaster(unrelatedMod, [], unofficialPatch));
    }

    [Fact]
    public void ActualMasterPermissionStillWorksWithoutRule()
    {
        var owner = ModKey.FromNameAndExtension("Owner.esp");
        var current = ModKey.FromNameAndExtension("Current.esp");
        PatcherSettings.Apply(new PatcherConfiguration());

        Assert.True(PatcherSettings.HasMasterOrVirtualMaster(current, [owner], owner));
    }
}
