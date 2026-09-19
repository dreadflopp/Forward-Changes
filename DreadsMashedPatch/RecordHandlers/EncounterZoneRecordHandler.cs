using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.EncounterZone;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using System;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: ECZN owner/location links and level/rank scalars use shared semantic handlers.
    // - Kept specialized: Flags retains the approved record-specific flag handler.
    // - Intentionally excluded: DATADataTypeState is serializer layout state.
    public class EncounterZoneRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Owner", new SimpleReflectionFormLinkPropertyHandler<IOwnerGetter, IEncounterZone, IEncounterZoneGetter>("Owner") },
            { "Location", new SimpleReflectionFormLinkPropertyHandler<ILocationGetter, IEncounterZone, IEncounterZoneGetter>("Location") },
            { "Rank", new SimpleReflectionPropertyHandler<byte, IEncounterZone, IEncounterZoneGetter>("Rank") },
            { "MinLevel", new SimpleReflectionPropertyHandler<byte, IEncounterZone, IEncounterZoneGetter>("MinLevel") },
            { "MaxLevel", new SimpleReflectionPropertyHandler<byte, IEncounterZone, IEncounterZoneGetter>("MaxLevel") },
            { "Flags", new FlagsHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IEncounterZoneGetter encounterZoneRecord)
            {
                throw new InvalidOperationException($"Expected IEncounterZoneGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = encounterZoneRecord
                .ToLink<IEncounterZoneGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IEncounterZone, IEncounterZoneGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}
