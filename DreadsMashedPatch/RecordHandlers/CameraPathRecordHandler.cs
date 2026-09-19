using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.CameraPath;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: Zoom and ZoomMustHaveCameraShots via reflection handlers.
    // - Kept specialized: Conditions, RelatedPaths, and Shots via dedicated list handlers.
    // - Rationale: preserve list/formlink merge behavior with concrete copy semantics.
    public class CameraPathRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Conditions", new ConditionsHandler() },
            { "RelatedPaths", new AtomicReflectionListPropertyHandler<IFormLinkGetter<ICameraPathGetter>, ICameraPath, ICameraPathGetter>("RelatedPaths") },
            { "Zoom", new SimpleReflectionPropertyHandler<Mutagen.Bethesda.Skyrim.CameraPath.ZoomType, ICameraPath, ICameraPathGetter>("Zoom") },
            { "ZoomMustHaveCameraShots", new SimpleReflectionPropertyHandler<bool, ICameraPath, ICameraPathGetter>("ZoomMustHaveCameraShots") },
            { "Shots", new ShotsHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not ICameraPathGetter cameraPath)
            {
                throw new InvalidOperationException($"Expected ICameraPathGetter but got {winningContext.Record.GetType()}");
            }

            return cameraPath
                .ToLink<ICameraPathGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ICameraPath, ICameraPathGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
