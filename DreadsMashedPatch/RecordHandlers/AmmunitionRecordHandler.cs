using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Ammunition;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: AMMO scalar/form-link fields and flags through reflection-based handlers.
    // - Kept specialized: Name/ObjectBounds/Model/Icons/Destructible/Keywords/Value/Weight and Skyrim flag handlers.
    // - Intentionally excluded: DATADataTypeState is Mutagen serialization state, not an xEdit field.
    // - Rationale: semantic fields are forwarded while the winning record retains its binary DATA layout.
    public class AmmunitionRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "Name", new NameHandler() },
            { "Model", new ModelHandler() },
            { "Icons", new IconsHandler() },
            { "Destructible", new DestructibleHandler() },
            { "PickUpSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IAmmunition, IAmmunitionGetter>("PickUpSound") },
            { "PutDownSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IAmmunition, IAmmunitionGetter>("PutDownSound") },
            { "Description", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IAmmunition, IAmmunitionGetter>("Description") },
            { "Keywords", new KeywordListHandler() },
            { "Projectile", new SimpleReflectionFormLinkPropertyHandler<IProjectileGetter, IAmmunition, IAmmunitionGetter>("Projectile") },
            { "Flags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Ammunition.Flag, IAmmunition, IAmmunitionGetter>("Flags") },
            { "Damage", new SimpleReflectionPropertyHandler<float, IAmmunition, IAmmunitionGetter>("Damage") },
            { "Value", new ValueHandler() },
            { "Weight", new WeightHandler() },
            { "ShortName", new SimpleReflectionPropertyHandler<string, IAmmunition, IAmmunitionGetter>("ShortName") },
            { "MajorFlags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Ammunition.MajorFlag, IAmmunition, IAmmunitionGetter>("MajorFlags") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IAmmunitionGetter ammunitionRecord)
            {
                throw new InvalidOperationException($"Expected IAmmunitionGetter but got {winningContext.Record.GetType()}");
            }

            return ammunitionRecord
                .ToLink<IAmmunitionGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IAmmunition, IAmmunitionGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
