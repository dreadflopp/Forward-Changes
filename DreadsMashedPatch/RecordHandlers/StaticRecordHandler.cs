using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using DreadsMashedPatch.PropertyHandlers.Static;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using System;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: semantic scalar and link fields use shared reflection handlers.
    // - Kept specialized/atomic: Model (file/data/alternate textures) and Lod (all four levels)
    //   remain cohesive values because their members describe one mesh or LOD set.
    // - Flag decision: the raw record-header handler is the sole storage path and owns common Skyrim plus STAT flags.
    //   The snow DNAM flag retains its project-approved per-bit handler.
    // - Intentionally excluded: Unused is serialization-only storage.
    // - Rationale: unused storage is not an xEdit-visible conflict surface; retained fields have established deep-copy or flag behavior.
    public class StaticRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler(typeof(SkyrimMajorRecord.SkyrimMajorRecordFlag), typeof(Mutagen.Bethesda.Skyrim.Static.MajorFlag)) },
            { "ModelAndBounds", new ModelBoundsHandler() },
            { "MaxAngle", new SimpleReflectionPropertyHandler<float, IStatic, IStaticGetter>("MaxAngle", 0.0001f) },
            { "Material", new SimpleReflectionFormLinkPropertyHandler<IMaterialObjectGetter, IStatic, IStaticGetter>("Material") },
            { "Flags", new FlagsHandler() },
            { "Lod", new LodHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IStaticGetter staticRecord)
            {
                throw new InvalidOperationException($"Expected IStaticGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = staticRecord
                .ToLink<IStaticGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IStatic, IStaticGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}
