using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Strings;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.Scroll;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers;

public class ScrollRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Name", new NameHandler() },
        { "ObjectBounds", new ObjectBoundsHandler() },
        { "Keywords", new KeywordListHandler() },
        { "MenuDisplayObject", new SimpleReflectionFormLinkPropertyHandler<IStaticGetter, IScroll, IScrollGetter>("MenuDisplayObject") },
        { "EquipmentType", new SimpleReflectionFormLinkPropertyHandler<IEquipTypeGetter, IScroll, IScrollGetter>("EquipmentType") },
        { "Description", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IScroll, IScrollGetter>("Description") },
        { "Model", new ModelHandler() },
        { "Destructible", new ComplexReflectionPropertyHandler<IDestructibleGetter, IScroll, IScrollGetter>("Destructible") },
        { "PickUpSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IScroll, IScrollGetter>("PickUpSound") },
        { "PutDownSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IScroll, IScrollGetter>("PutDownSound") },
        { "Value", new SimpleReflectionPropertyHandler<uint, IScroll, IScrollGetter>("Value") },
        { "Weight", new SimpleReflectionPropertyHandler<float, IScroll, IScrollGetter>("Weight") },
        { "BaseCost", new SimpleReflectionPropertyHandler<uint, IScroll, IScrollGetter>("BaseCost") },
        { "Flags", new SimpleReflectionFlagPropertyHandler<SpellDataFlag, IScroll, IScrollGetter>("Flags") },
        { "Type", new SimpleReflectionPropertyHandler<SpellType, IScroll, IScrollGetter>("Type") },
        { "ChargeTime", new SimpleReflectionPropertyHandler<float, IScroll, IScrollGetter>("ChargeTime", 0.001f) },
        { "CastType", new SimpleReflectionPropertyHandler<CastType, IScroll, IScrollGetter>("CastType") },
        { "TargetType", new SimpleReflectionPropertyHandler<TargetType, IScroll, IScrollGetter>("TargetType") },
        { "CastDuration", new SimpleReflectionPropertyHandler<float, IScroll, IScrollGetter>("CastDuration", 0.001f) },
        { "Range", new SimpleReflectionPropertyHandler<float, IScroll, IScrollGetter>("Range", 0.001f) },
        { "HalfCostPerk", new SimpleReflectionFormLinkPropertyHandler<IPerkGetter, IScroll, IScrollGetter>("HalfCostPerk") },
        { "Effects", new EffectsHandler() }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IScrollGetter scrollRecord)
        {
            throw new InvalidOperationException($"Expected IScrollGetter but got {winningContext.Record.GetType()}");
        }

        return scrollRecord
            .ToLink<IScrollGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IScroll, IScrollGetter>(state.LinkCache)
            .ToArray();
    }
}