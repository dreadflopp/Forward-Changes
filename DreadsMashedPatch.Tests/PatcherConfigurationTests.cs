using DreadsMashedPatch.Enums;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class PatcherConfigurationTests
{
    [Fact]
    public void DefaultsPreserveForwardingPoliciesAndDisableStructurallyRiskyRecordTypes()
    {
        var settings = new PatcherConfiguration();

        Assert.Equal(
            new[]
            {
                typeof(IDialogBranchGetter).FullName!,
                typeof(IDialogResponsesGetter).FullName!,
                typeof(IDialogTopicGetter).FullName!,
                typeof(IDialogViewGetter).FullName!,
                typeof(INavigationMeshGetter).FullName!,
                typeof(IPackageGetter).FullName!
            },
            settings.DisabledRecordTypes.Order(StringComparer.Ordinal));
        Assert.Equal(
            ["True Light.esp", "True Light - USSEP Patch.esp"],
            settings.IgnoredMods);
        Assert.Empty(settings.AlwaysWinningMods);
        Assert.True(settings.Forwarding.TreatCreationClubAsVanilla);
        Assert.True(settings.Forwarding.EnforceSingleVanillaWeaponTypeKeyword);
        Assert.Equal(9, settings.Forwarding.VanillaWeaponTypeKeywords.Count);
        Assert.Contains("01E711:Skyrim.esm", settings.Forwarding.VanillaWeaponTypeKeywords);
        Assert.Equal(
            EditorIdForwardingPolicy.ForwardOnlyWithOtherChanges,
            settings.Forwarding.EditorIdPolicy);
        Assert.Equal(
            ProtectionForwardingPolicy.PreferHigherWithAuthorizedDowngrades,
            settings.Forwarding.ProtectionPolicy);
        Assert.False(settings.Diagnostics.DebugMode);
        Assert.Equal(PatcherLogVerbosity.ContextChanges, settings.Diagnostics.Verbosity);
        Assert.Collection(
            settings.CompatibilityRules,
            ussepRule =>
            {
                Assert.Equal("Unofficial Skyrim Special Edition Patch.esp", ussepRule.InjectedMaster);
                Assert.DoesNotContain("Unofficial Skyrim Creation Club Content Patch.esl", ussepRule.TargetMods);
            },
            creationClubPatchRule =>
            {
                Assert.Equal(
                    "Unofficial Skyrim Creation Club Content Patch.esl",
                    creationClubPatchRule.InjectedMaster);
                Assert.Contains("Creation Club Rebalancing.esp", creationClubPatchRule.TargetMods);
                Assert.Contains("Masterwork - Bittercup.esp", creationClubPatchRule.TargetMods);
                Assert.Contains("Masterwork - Umbra.esp", creationClubPatchRule.TargetMods);
            },
            apothecaryRule =>
            {
                Assert.Equal("Apothecary.esp", apothecaryRule.InjectedMaster);
                Assert.Contains("StarfrostInjuries.esp", apothecaryRule.TargetMods);
            },
            brumaUnofficialFixesRule =>
            {
                Assert.Equal(
                    "BSHeartland - Unofficial Fixes.esp",
                    brumaUnofficialFixesRule.InjectedMaster);
                Assert.Contains(
                    "BS Bruma - CC Curios Patch.esp",
                    brumaUnofficialFixesRule.TargetMods);
            });
    }

    [Fact]
    public void DefaultCompatibilityRulesAreIndependent()
    {
        var first = new PatcherConfiguration();
        var second = new PatcherConfiguration();

        foreach (var rule in first.CompatibilityRules)
        {
            rule.TargetMods.Clear();
        }

        Assert.All(second.CompatibilityRules, rule => Assert.NotEmpty(rule.TargetMods));
    }

    [Fact]
    public void CopyCreatesAnIndependentRuntimeSnapshot()
    {
        var settings = new PatcherConfiguration
        {
            DisabledRecordTypes = ["Mutagen.Bethesda.Skyrim.IQuestGetter"],
            IgnoredMods = ["True Light.esp", "Ignored.esp"],
            AlwaysWinningMods = ["Priority.esp"],
            Forwarding = new ForwardingSettings
            {
                TreatCreationClubAsVanilla = false,
                EditorIdPolicy = EditorIdForwardingPolicy.ForwardOnlyWithOtherChanges
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
        settings.IgnoredMods.Clear();
        settings.AlwaysWinningMods.Clear();
        settings.Diagnostics.DeepDiveProperties.Clear();
        settings.Forwarding.VanillaWeaponTypeKeywords.Clear();

        Assert.Contains("Mutagen.Bethesda.Skyrim.IQuestGetter", copy.DisabledRecordTypes);
        Assert.Contains("Ignored.esp", copy.IgnoredMods);
        Assert.Contains("Priority.esp", copy.AlwaysWinningMods);
        Assert.False(copy.Forwarding.TreatCreationClubAsVanilla);
        Assert.Equal(EditorIdForwardingPolicy.ForwardOnlyWithOtherChanges, copy.Forwarding.EditorIdPolicy);
        Assert.Contains("Aliases", copy.Diagnostics.DeepDiveProperties);
        Assert.Contains("QUST", copy.Diagnostics.DeepDiveRecordSignatures);
        Assert.Equal(9, copy.Forwarding.VanillaWeaponTypeKeywords.Count);
    }

    [Fact]
    public void AlwaysWinningModsPreserveOrderAndLastDuplicatePosition()
    {
        var settings = new PatcherConfiguration
        {
            AlwaysWinningMods = [" First.esp ", "Second.esp", "FIRST.esp"]
        };

        settings.Normalize();

        Assert.Equal(["Second.esp", "FIRST.esp"], settings.AlwaysWinningMods);
    }
}
