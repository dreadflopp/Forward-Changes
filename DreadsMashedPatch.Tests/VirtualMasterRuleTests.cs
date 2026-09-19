using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
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

    [Fact]
    public void RuleForExistingActualMasterDoesNotDuplicateOrModifyHeader()
    {
        var unofficialPatch = ModKey.FromNameAndExtension("Unofficial Skyrim Special Edition Patch.esp");
        var adamant = ModKey.FromNameAndExtension("Adamant.esp");
        var mod = new SkyrimMod(adamant, SkyrimRelease.SkyrimSE);
        mod.ModHeader.MasterReferences.Add(new MasterReference { Master = unofficialPatch });
        var configuration = new PatcherConfiguration
        {
            CompatibilityRules =
            [
                new VirtualMasterRule
                {
                    InjectedMaster = unofficialPatch.FileName.String,
                    TargetMods = [adamant.FileName.String]
                }
            ]
        };

        PatcherSettings.Apply(configuration);

        Assert.True(PatcherSettings.HasMasterOrVirtualMaster(mod, unofficialPatch.FileName.String));
        Assert.True(PatcherSettings.HasMasterOrVirtualMaster(adamant, [unofficialPatch], unofficialPatch));
        Assert.Single(mod.ModHeader.MasterReferences);
        Assert.Equal(unofficialPatch, mod.ModHeader.MasterReferences[0].Master);
    }
}
