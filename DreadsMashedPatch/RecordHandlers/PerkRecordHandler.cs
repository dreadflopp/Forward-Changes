using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Strings;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.Perk;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.Enums;

namespace ForwardChanges.RecordHandlers;

// Migration note:
// - Generalized: PERK scalar, translated text, and link fields.
// - Kept specialized: polymorphic effects and condition lists.
// - Flag decision: the raw record-header handler is the sole storage path and owns common Skyrim plus PERK flags.
// - Coupled forwarding: gameplay-tree changes establish a complete PERK ownership boundary by default.
// - Rationale: generated DeepCopy dispatch preserves each concrete abstract-base subtype, while atomic ownership
//   prevents independently merged ranks, entry points, conditions, and predecessor links from forming invalid trees.
public class PerkRecordHandler : AbstractRecordHandler
{
    private static readonly IReadOnlySet<string> CoupledPropertyNames = new HashSet<string>(StringComparer.Ordinal)
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
    };

    private readonly PerkForwardingPolicy _forwardingPolicy;

    public PerkRecordHandler(PerkForwardingPolicy? forwardingPolicy = null)
    {
        _forwardingPolicy = forwardingPolicy ?? PatcherSettings.PerkPolicy;
    }

    public PerkForwardingPolicy ForwardingPolicy => _forwardingPolicy;

    protected override IReadOnlySet<string> AtomicOwnershipTriggerProperties =>
        _forwardingPolicy == PerkForwardingPolicy.AtomicOnCoupledPropertyChange
            ? CoupledPropertyNames
            : EmptyAtomicOwnershipTriggerProperties;

    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler(typeof(SkyrimMajorRecord.SkyrimMajorRecordFlag), typeof(Perk.MajorFlag)) },
        { "VirtualMachineAdapter", new ComplexReflectionPropertyHandler<IPerkAdapterGetter, IPerk, IPerkGetter>("VirtualMachineAdapter") },
        { "Name", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IPerk, IPerkGetter>("Name") },
        { "Description", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IPerk, IPerkGetter>("Description") },
        { "Icons", new SimpleReflectionIconsPropertyHandler<IPerk, IPerkGetter>("Icons") },
        { "Conditions", new ConditionsHandler<IPerk, IPerkGetter>(record => record.Conditions, record => record.Conditions) },
        { "Trait", new SimpleReflectionPropertyHandler<bool, IPerk, IPerkGetter>("Trait") },
        { "Level", new SimpleReflectionPropertyHandler<byte, IPerk, IPerkGetter>("Level") },
        { "NumRanks", new SimpleReflectionPropertyHandler<byte, IPerk, IPerkGetter>("NumRanks") },
        { "Playable", new SimpleReflectionPropertyHandler<bool, IPerk, IPerkGetter>("Playable") },
        { "Hidden", new SimpleReflectionPropertyHandler<bool, IPerk, IPerkGetter>("Hidden") },
        { "NextPerk", new SimpleReflectionFormLinkPropertyHandler<IPerkGetter, IPerk, IPerkGetter>("NextPerk") },
        { "Effects", new EffectsHandler() }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IPerkGetter perkRecord)
        {
            throw new InvalidOperationException($"Expected IPerkGetter but got {winningContext.Record.GetType()}");
        }

        return perkRecord
            .ToLink<IPerkGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IPerk, IPerkGetter>(state.LinkCache)
            .ToArray();
    }
}
