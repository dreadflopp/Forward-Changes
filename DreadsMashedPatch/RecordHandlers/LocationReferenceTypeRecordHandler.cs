using System;
using System.Drawing;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: color and core record metadata via reflection handlers.
    // - Kept specialized: none.
    // - Rationale: the surface is a single nullable color property on a standard major record.
    public class LocationReferenceTypeRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Color", new SimpleReflectionPropertyHandler<Color?, ILocationReferenceType, ILocationReferenceTypeGetter>("Color") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not ILocationReferenceTypeGetter locationReferenceType)
            {
                throw new InvalidOperationException($"Expected ILocationReferenceTypeGetter but got {winningContext.Record.GetType()}");
            }

            return locationReferenceType
                .ToLink<ILocationReferenceTypeGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ILocationReferenceType, ILocationReferenceTypeGetter>(state.LinkCache)
                .ToArray();
        }
    }
}