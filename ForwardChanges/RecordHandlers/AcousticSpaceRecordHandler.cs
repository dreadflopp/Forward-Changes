using System;
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
    // - Generalized: AmbientSound, UseSoundFromRegion, EnvironmentType via reflection form-link handlers.
    // - Kept specialized: ObjectBounds via shared handler.
    // - Rationale: preserves existing object-bound behavior and keeps remaining fields simple.
    public class AcousticSpaceRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "AmbientSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IAcousticSpace, IAcousticSpaceGetter>("AmbientSound") },
            { "UseSoundFromRegion", new SimpleReflectionFormLinkPropertyHandler<IRegionGetter, IAcousticSpace, IAcousticSpaceGetter>("UseSoundFromRegion") },
            { "EnvironmentType", new SimpleReflectionFormLinkPropertyHandler<IReverbParametersGetter, IAcousticSpace, IAcousticSpaceGetter>("EnvironmentType") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IAcousticSpaceGetter acousticSpace)
            {
                throw new InvalidOperationException($"Expected IAcousticSpaceGetter but got {winningContext.Record.GetType()}");
            }

            return acousticSpace
                .ToLink<IAcousticSpaceGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IAcousticSpace, IAcousticSpaceGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
