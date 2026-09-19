using DreadsMashedPatch.Enums;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class PatcherConfigurationTests
{
    [Fact]
    public void DefaultsPreserveForwardingPoliciesAndEnableEveryRecordType()
    {
        var settings = new PatcherConfiguration();

        Assert.Empty(settings.DisabledRecordTypes);
        Assert.True(settings.Forwarding.TreatCreationClubAsVanilla);
        Assert.Equal(
            ProtectionForwardingPolicy.PreferHigherWithAuthorizedDowngrades,
            settings.Forwarding.ProtectionPolicy);
        Assert.Equal(PerkForwardingPolicy.AtomicOnCoupledPropertyChange, settings.Forwarding.PerkPolicy);
        Assert.Equal(QuestForwardingPolicy.AtomicOnStructuralChange, settings.Forwarding.QuestPolicy);
        Assert.Equal(
            StoryManagerForwardingPolicy.AtomicOnConfigurationChange,
            settings.Forwarding.StoryManagerPolicy);
        Assert.False(settings.Diagnostics.DebugMode);
        Assert.Equal(PatcherLogVerbosity.ContextChanges, settings.Diagnostics.Verbosity);
    }

    [Fact]
    public void CopyCreatesAnIndependentRuntimeSnapshot()
    {
        var settings = new PatcherConfiguration
        {
            DisabledRecordTypes = ["Mutagen.Bethesda.Skyrim.IQuestGetter"],
            Forwarding = new ForwardingSettings
            {
                TreatCreationClubAsVanilla = false
            },
            Diagnostics = new DiagnosticsSettings
            {
                DebugMode = true,
                DeepDiveProperties = ["Aliases"],
                DeepDiveRecordSignatures = ["QUST"]
            }
        };

        var copy = settings.Copy();
        settings.DisabledRecordTypes.Clear();
        settings.Diagnostics.DeepDiveProperties.Clear();

        Assert.Contains("Mutagen.Bethesda.Skyrim.IQuestGetter", copy.DisabledRecordTypes);
        Assert.False(copy.Forwarding.TreatCreationClubAsVanilla);
        Assert.Contains("Aliases", copy.Diagnostics.DeepDiveProperties);
        Assert.Contains("QUST", copy.Diagnostics.DeepDiveRecordSignatures);
    }
}
