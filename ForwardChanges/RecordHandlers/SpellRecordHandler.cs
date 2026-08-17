using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Spell;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    public class SpellRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Name", new NameHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "MenuDisplayObject", new SimpleReflectionFormLinkPropertyHandler<IStaticGetter, ISpell, ISpellGetter>("MenuDisplayObject") },
            { "Description", new DescriptionHandler() },
            { "Flags", new FlagsHandler() },
            { "Keywords", new KeywordListHandler() },
            { "EquipmentType", new SimpleReflectionFormLinkPropertyHandler<IEquipTypeGetter, ISpell, ISpellGetter>("EquipmentType") },
            { "BaseCost", new SimpleReflectionPropertyHandler<uint, ISpell, ISpellGetter>("BaseCost") },
            { "Type", new SimpleReflectionPropertyHandler<SpellType, ISpell, ISpellGetter>("Type") },
            { "ChargeTime", new SimpleReflectionPropertyHandler<float, ISpell, ISpellGetter>("ChargeTime", 0.001f) },
            { "CastType", new SimpleReflectionPropertyHandler<CastType, ISpell, ISpellGetter>("CastType") },
            { "TargetType", new SimpleReflectionPropertyHandler<TargetType, ISpell, ISpellGetter>("TargetType") },
            { "CastDuration", new SimpleReflectionPropertyHandler<float, ISpell, ISpellGetter>("CastDuration", 0.001f) },
            { "Range", new SimpleReflectionPropertyHandler<float, ISpell, ISpellGetter>("Range", 0.001f) },
            { "HalfCostPerk", new SimpleReflectionFormLinkPropertyHandler<IPerkGetter, ISpell, ISpellGetter>("HalfCostPerk") },
            { "Effects", new EffectsHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not ISpellGetter spellRecord)
            {
                throw new InvalidOperationException($"Expected ISpellGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = spellRecord
                .ToLink<ISpellGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ISpell, ISpellGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}