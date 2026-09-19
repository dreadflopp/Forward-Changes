using System.Collections;
using DreadsMashedPatch.Contexts.Interfaces;
using DreadsMashedPatch.Enums;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class PerkRecordHandlerTests
{
    [Fact]
    public void AtomicCoupledForwardingIsTheDefaultPolicy()
    {
        Assert.Equal(
            PerkForwardingPolicy.AtomicOnCoupledPropertyChange,
            PatcherSettings.PerkPolicy);

        var handler = new TestablePerkRecordHandler();
        Assert.Equal(PerkForwardingPolicy.AtomicOnCoupledPropertyChange, handler.ForwardingPolicy);
        Assert.Equal(
            new HashSet<string>(StringComparer.Ordinal)
            {
                "VirtualMachineAdapter",
                "Conditions",
                "Trait",
                "Level",
                "NumRanks",
                "Playable",
                "Hidden",
                "NextPerk",
                "Effects"
            },
            handler.AtomicTriggers);
    }

    [Fact]
    public void StandardForwardingDisablesAtomicTriggers()
    {
        var handler = new TestablePerkRecordHandler(PerkForwardingPolicy.StandardForwarding);
        var previous = CreatePerk();
        var current = CreatePerk();
        current.Level = 10;

        Assert.Empty(handler.AtomicTriggers);
        Assert.Empty(handler.GetChangedTriggers(previous, current));
    }

    [Theory]
    [InlineData("VirtualMachineAdapter")]
    [InlineData("Conditions")]
    [InlineData("Trait")]
    [InlineData("Level")]
    [InlineData("NumRanks")]
    [InlineData("Playable")]
    [InlineData("Hidden")]
    [InlineData("NextPerk")]
    [InlineData("Effects")]
    public void EveryCoupledPropertyTriggersAtomicOwnership(string propertyName)
    {
        var handler = new TestablePerkRecordHandler();
        var previous = CreatePerk();
        var current = CreatePerk();
        ChangeProperty(current, propertyName);

        Assert.Equal([propertyName], handler.GetChangedTriggers(previous, current));
    }

    [Fact]
    public void UncoupledChangesDoNotTriggerAtomicOwnership()
    {
        var handler = new TestablePerkRecordHandler();
        var previous = CreatePerk();
        var current = CreatePerk();
        current.EditorID = "ChangedEditorId";
        current.Name = "Changed name";
        current.Description = "Changed description";
        current.MajorRecordFlagsRaw = 0x4000;

        Assert.Empty(handler.GetChangedTriggers(previous, current));
    }

    [Fact]
    public void CoupledChangeResetsEveryPropertyValueAndOwnerToCurrentOverride()
    {
        var handler = new TestablePerkRecordHandler();
        var original = CreatePerk();
        original.EditorID = "OriginalEditorId";
        original.Name = "Original name";
        original.Description = "Original description";
        original.Level = 1;

        var atomic = CreatePerk();
        atomic.EditorID = "AtomicEditorId";
        atomic.Name = "Atomic name";
        atomic.Description = "Atomic description";
        atomic.Level = 2;
        atomic.NumRanks = 3;
        atomic.MajorRecordFlagsRaw =
            (int)Perk.MajorFlag.NonPlayable
            | (int)SkyrimMajorRecord.SkyrimMajorRecordFlag.InitiallyDisabled
            | 0x4000;
        atomic.Conditions.Add(CreateCondition());
        atomic.Effects.Add(new PerkQuestEffect { Stage = 10, Rank = 1 });

        var originalContext = CreateContext(OriginalModKey, original);
        var atomicContext = CreateContext(AtomicModKey, atomic);
        handler.InitializeForTest(originalContext);

        var changedProperties = handler.ResetForTest(originalContext, atomicContext);

        Assert.Contains("Level", changedProperties);
        Assert.Contains("NumRanks", changedProperties);
        Assert.Contains("Conditions", changedProperties);
        Assert.Contains("Effects", changedProperties);

        foreach (var (propertyName, propertyHandler) in handler.PropertyHandlers)
        {
            Assert.True(
                propertyHandler.AreValuesEqual(
                    propertyHandler.GetValue(atomic),
                    handler.GetForwardValue(propertyName)),
                $"{propertyName} did not reset to the atomic override value.");

            Assert.All(
                handler.GetForwardOwners(propertyName),
                owner => Assert.Equal(AtomicModKey.ToString(), owner));
        }
    }

    [Fact]
    public void PerkUsesOneCompositeRecordHeaderFlagPath()
    {
        var recordHandler = new PerkRecordHandler();
        var handlers = recordHandler.PropertyHandlers;

        Assert.Contains("MajorRecordFlagsRaw", handlers.Keys);
        Assert.DoesNotContain("MajorFlags", handlers.Keys);
        Assert.DoesNotContain("SkyrimMajorRecordFlags", handlers.Keys);

        var rawHandler = Assert.IsType<MajorRecordFlagsRawHandler>(handlers["MajorRecordFlagsRaw"]);
        Assert.False(rawHandler.AreValuesEqual(0, (int)Perk.MajorFlag.NonPlayable));
        Assert.False(rawHandler.AreValuesEqual(
            0,
            (int)SkyrimMajorRecord.SkyrimMajorRecordFlag.InitiallyDisabled));

        var perk = CreatePerk();
        var expected = (int)Perk.MajorFlag.NonPlayable
            | (int)SkyrimMajorRecord.SkyrimMajorRecordFlag.InitiallyDisabled
            | 0x4000;
        recordHandler.ApplyForwardedProperties(perk, new Dictionary<string, object?>
        {
            ["MajorRecordFlagsRaw"] = expected
        });

        Assert.Equal(expected, perk.MajorRecordFlagsRaw);
        Assert.True(perk.MajorFlags.HasFlag(Perk.MajorFlag.NonPlayable));
        Assert.True(perk.SkyrimMajorRecordFlags.HasFlag(
            SkyrimMajorRecord.SkyrimMajorRecordFlag.InitiallyDisabled));
    }

    private static void ChangeProperty(Perk perk, string propertyName)
    {
        switch (propertyName)
        {
            case "VirtualMachineAdapter":
                perk.VirtualMachineAdapter = new PerkAdapter();
                break;
            case "Conditions":
                perk.Conditions.Add(CreateCondition());
                break;
            case "Trait":
                perk.Trait = !perk.Trait;
                break;
            case "Level":
                perk.Level++;
                break;
            case "NumRanks":
                perk.NumRanks++;
                break;
            case "Playable":
                perk.Playable = !perk.Playable;
                break;
            case "Hidden":
                perk.Hidden = !perk.Hidden;
                break;
            case "NextPerk":
                perk.NextPerk.SetTo(new FormKey(ReferencedModKey, 0x200));
                break;
            case "Effects":
                perk.Effects.Add(new PerkQuestEffect { Stage = 10, Rank = 1 });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(propertyName), propertyName, null);
        }
    }

    private static Perk CreatePerk() =>
        new(new FormKey(OriginalModKey, 0x100), SkyrimRelease.SkyrimSE);

    private static ConditionFloat CreateCondition() => new()
    {
        ComparisonValue = 1,
        Data = new GetRandomPercentConditionData()
    };

    private static IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> CreateContext(
        ModKey modKey,
        IMajorRecord record)
        => new ModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>(
            modKey,
            record,
            (_, _) => throw new NotSupportedException(),
            (_, _, _, _) => throw new NotSupportedException());

    private sealed class TestablePerkRecordHandler(
        PerkForwardingPolicy? forwardingPolicy = null)
        : PerkRecordHandler(forwardingPolicy)
    {
        public IReadOnlySet<string> AtomicTriggers => AtomicOwnershipTriggerProperties;

        public IReadOnlyList<string> GetChangedTriggers(IPerkGetter previous, IPerkGetter current) =>
            GetChangedAtomicOwnershipTriggerProperties(previous, current);

        public void InitializeForTest(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context) =>
            InitializePropertyContexts(context, context);

        public IReadOnlyList<string> ResetForTest(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> previousContext,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> currentContext) =>
            ResetPropertyContextsIfAtomicOwnershipTriggered(previousContext, currentContext);

        public object? GetForwardValue(string propertyName) =>
            PropertyContexts[propertyName].GetForwardValue();

        public IReadOnlyList<string> GetForwardOwners(string propertyName)
        {
            var context = PropertyContexts[propertyName];
            var owners = new List<string>();
            AddOwner(context.GetType().GetProperty("ForwardValueContext")?.GetValue(context), owners);
            AddOwners(context.GetType().GetProperty("ForwardValueContexts")?.GetValue(context), owners);
            AddOwners(context.GetType().GetProperty("ForwardFlagContexts")?.GetValue(context), owners);

            if (context.GetType().GetProperty("ForwardPresenceOwnerMod")?.GetValue(context) is string presenceOwner)
            {
                owners.Add(presenceOwner);
            }

            Assert.NotEmpty(owners);
            return owners;
        }

        private static void AddOwners(object? values, ICollection<string> owners)
        {
            if (values is not IEnumerable enumerable)
            {
                return;
            }

            foreach (var value in enumerable)
            {
                AddOwner(value, owners);
            }
        }

        private static void AddOwner(object? value, ICollection<string> owners)
        {
            if (value?.GetType().GetProperty("OwnerMod")?.GetValue(value) is string owner)
            {
                owners.Add(owner);
            }
        }
    }

    private static readonly ModKey OriginalModKey = new("Skyrim.esm", ModType.Master);
    private static readonly ModKey AtomicModKey = new("AtomicPerk.esp", ModType.Plugin);
    private static readonly ModKey ReferencedModKey = new("Referenced.esp", ModType.Plugin);
}
