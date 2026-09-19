using ForwardChanges.PropertyHandlers.Scene;
using ForwardChanges.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class SceneRecordHandlerTests
{
    private static readonly ModKey TestModKey = ModKey.FromNameAndExtension("SceneTests.esp");

    [Fact]
    public void PhaseComparisonIgnoresXEditHiddenLegacyData()
    {
        var withLegacyData = new ScenePhase
        {
            Name = string.Empty,
            EditorWidth = 200,
            Unused = new ScenePhaseUnusedData
            {
                QNAM = new MemorySlice<byte>([1, 2, 3, 4]),
                SCHR = new MemorySlice<byte>(new byte[20])
            },
            Unused2 = new ScenePhaseUnusedData
            {
                QNAM = new MemorySlice<byte>([1, 2, 3, 4]),
                SCHR = new MemorySlice<byte>(new byte[20])
            }
        };
        var withoutLegacyData = new ScenePhase
        {
            Name = string.Empty,
            EditorWidth = 200,
            Unused2 = new ScenePhaseUnusedData()
        };
        var handler = new ScenePhasesHandler();

        Assert.True(handler.AreValuesEqual([withLegacyData], [withoutLegacyData]));
    }

    [Fact]
    public void PhaseCopyPreservesConditionsAndDropsHiddenLegacyData()
    {
        var source = CreateScene(0x800);
        var phase = new ScenePhase
        {
            Name = "Phase",
            EditorWidth = 250,
            Unused = new ScenePhaseUnusedData
            {
                QNAM = new MemorySlice<byte>([1, 2, 3, 4])
            }
        };
        phase.StartConditions.Add(CreateCondition(1));
        phase.CompletionConditions.Add(CreateCondition(2));
        source.Phases.Add(phase);
        var target = CreateScene(0x801);
        var handler = new ScenePhasesHandler();

        handler.SetValue(target, handler.GetValue(source));

        var copied = Assert.Single(target.Phases);
        Assert.Equal(1, Assert.IsType<ConditionFloat>(Assert.Single(copied.StartConditions)).ComparisonValue);
        Assert.Equal(2, Assert.IsType<ConditionFloat>(Assert.Single(copied.CompletionConditions)).ComparisonValue);
        Assert.Null(copied.Unused);
        Assert.Null(copied.Unused2);
    }

    [Fact]
    public void ActionCopyPreservesPackagesAndDropsHiddenLegacyData()
    {
        var packageKey = new FormKey(TestModKey, 0x900);
        var source = CreateScene(0x800);
        var action = new SceneAction
        {
            Type = SceneAction.TypeEnum.Package,
            Name = "Package action",
            Index = 4,
            Unused = new ScenePhaseUnusedData
            {
                QNAM = new MemorySlice<byte>([1, 2, 3, 4])
            }
        };
        action.Packages.Add(new FormLink<IPackageGetter>(packageKey));
        source.Actions.Add(action);
        var target = CreateScene(0x801);
        var handler = new SceneActionsHandler();

        handler.SetValue(target, handler.GetValue(source));

        var copied = Assert.Single(target.Actions);
        Assert.Equal(packageKey, Assert.Single(copied.Packages).FormKey);
        Assert.Null(copied.Unused);
    }

    [Fact]
    public void ScriptForwardingPreservesSceneFragments()
    {
        var source = CreateScene(0x800);
        source.VirtualMachineAdapter = new SceneAdapter
        {
            ScriptFragments = new SceneScriptFragments { FileName = "SceneFragments.pex" }
        };
        source.VirtualMachineAdapter.Scripts.Add(new ScriptEntry { Name = "SceneScript" });
        var target = CreateScene(0x801);
        target.VirtualMachineAdapter = new SceneAdapter
        {
            ScriptFragments = new SceneScriptFragments { FileName = "KeepMe.pex" }
        };
        var handler = new SceneScriptsHandler();

        handler.SetValue(target, handler.GetValue(source));

        Assert.Equal("SceneScript", Assert.Single(target.VirtualMachineAdapter.Scripts).Name);
        Assert.Equal("KeepMe.pex", target.VirtualMachineAdapter.ScriptFragments?.FileName);
    }

    [Fact]
    public void SceneFragmentsAreDeepCopiedWithPhaseIndexes()
    {
        var source = CreateScene(0x800);
        source.VirtualMachineAdapter = new SceneAdapter
        {
            ScriptFragments = new SceneScriptFragments { FileName = "SceneFragments.pex" }
        };
        source.VirtualMachineAdapter.ScriptFragments.PhaseFragments.Add(new ScenePhaseFragment
        {
            Index = 2,
            Flags = ScenePhaseFragment.Flag.OnStart,
            ScriptName = "FragmentScript",
            FragmentName = "Fragment_2"
        });
        var target = CreateScene(0x801);
        var handler = new SceneScriptFragmentsHandler();

        handler.SetValue(target, handler.GetValue(source));

        var copied = Assert.Single(target.VirtualMachineAdapter!.ScriptFragments!.PhaseFragments);
        Assert.Equal((byte)2, copied.Index);
        Assert.Equal("Fragment_2", copied.FragmentName);
        Assert.NotSame(source.VirtualMachineAdapter.ScriptFragments, target.VirtualMachineAdapter.ScriptFragments);
    }

    [Fact]
    public void StructuralChangeTriggersCompleteSceneOwnership()
    {
        var previous = CreateScene(0x800);
        previous.Phases.Add(new ScenePhase { Name = "Before", EditorWidth = 200 });
        var current = CreateScene(0x801);
        current.Phases.Add(new ScenePhase { Name = "After", EditorWidth = 200 });
        var handler = new TestSceneRecordHandler();

        Assert.Contains("Phases", handler.GetAtomicChanges(previous, current));
    }

    [Fact]
    public void SceneHandlerHasNoOverlappingWholeVmadRegistration()
    {
        var handlers = new SceneRecordHandler().PropertyHandlers;

        Assert.DoesNotContain("VirtualMachineAdapter", handlers.Keys);
        Assert.Contains("VirtualMachineAdapter.Presence", handlers.Keys);
        Assert.Contains("VirtualMachineAdapter.Scripts", handlers.Keys);
        Assert.Contains("VirtualMachineAdapter.ScriptFragments", handlers.Keys);
    }

    private static Mutagen.Bethesda.Skyrim.Scene CreateScene(uint id) =>
        new(new FormKey(TestModKey, id), SkyrimRelease.SkyrimSE);

    private static ConditionFloat CreateCondition(float comparisonValue) => new()
    {
        ComparisonValue = comparisonValue,
        Data = new GetRandomPercentConditionData()
    };

    private sealed class TestSceneRecordHandler : SceneRecordHandler
    {
        public IReadOnlyList<string> GetAtomicChanges(ISceneGetter previous, ISceneGetter current) =>
            GetChangedAtomicOwnershipTriggerProperties(previous, current);
    }
}
