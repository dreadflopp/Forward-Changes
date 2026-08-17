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
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers;

// Migration note:
// - Generalized: PERK fields including conditions/effects lists via generic handlers.
// - Kept specialized: none.
// - Rationale: mutable list surfaces and translated text fields are already supported in project patterns.
public class PerkRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "VirtualMachineAdapter", new ComplexReflectionPropertyHandler<IPerkAdapterGetter, IPerk, IPerkGetter>("VirtualMachineAdapter") },
        { "Name", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IPerk, IPerkGetter>("Name") },
        { "Description", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IPerk, IPerkGetter>("Description") },
        { "Icons", new ComplexReflectionPropertyHandler<IIconsGetter, IPerk, IPerkGetter>("Icons") },
        { "Conditions", new SimpleReflectionListPropertyHandler<IConditionGetter, IPerk, IPerkGetter>("Conditions", ListOrdering.None) },
        { "Trait", new SimpleReflectionPropertyHandler<bool, IPerk, IPerkGetter>("Trait") },
        { "Level", new SimpleReflectionPropertyHandler<byte, IPerk, IPerkGetter>("Level") },
        { "NumRanks", new SimpleReflectionPropertyHandler<byte, IPerk, IPerkGetter>("NumRanks") },
        { "Playable", new SimpleReflectionPropertyHandler<bool, IPerk, IPerkGetter>("Playable") },
        { "Hidden", new SimpleReflectionPropertyHandler<bool, IPerk, IPerkGetter>("Hidden") },
        { "NextPerk", new SimpleReflectionFormLinkPropertyHandler<IPerkGetter, IPerk, IPerkGetter>("NextPerk") },
        { "Effects", new SimpleReflectionListPropertyHandler<IAPerkEffectGetter, IPerk, IPerkGetter>("Effects", ListOrdering.None) },
        { "MajorFlags", new SimpleReflectionFlagPropertyHandler<Perk.MajorFlag, IPerk, IPerkGetter>("MajorFlags") }
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