using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.RecordHandlers.Abstracts;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: Camera shot scalar/form-link fields and flags via reflection handlers.
    // - Kept specialized: Model via shared model handler.
    // - Intentionally excluded: DATADataTypeState is Mutagen serialization state, not an xEdit field.
    // - Rationale: semantic fields are forwarded while the winning record retains its binary DATA layout.
    public class CameraShotRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Model", new ModelHandler() },
            { "Action", new SimpleReflectionPropertyHandler<Mutagen.Bethesda.Skyrim.CameraShot.ActionType, ICameraShot, ICameraShotGetter>("Action") },
            { "Location", new SimpleReflectionPropertyHandler<Mutagen.Bethesda.Skyrim.CameraShot.LocationType, ICameraShot, ICameraShotGetter>("Location") },
            { "Target", new SimpleReflectionPropertyHandler<Mutagen.Bethesda.Skyrim.CameraShot.LocationType, ICameraShot, ICameraShotGetter>("Target") },
            { "Flags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.CameraShot.Flag, ICameraShot, ICameraShotGetter>("Flags") },
            { "TimeMultiplierPlayer", new SimpleReflectionPropertyHandler<float, ICameraShot, ICameraShotGetter>("TimeMultiplierPlayer") },
            { "TimeMultiplierTarget", new SimpleReflectionPropertyHandler<float, ICameraShot, ICameraShotGetter>("TimeMultiplierTarget") },
            { "TimeMultiplierGlobal", new SimpleReflectionPropertyHandler<float, ICameraShot, ICameraShotGetter>("TimeMultiplierGlobal") },
            { "MaxTime", new SimpleReflectionPropertyHandler<float, ICameraShot, ICameraShotGetter>("MaxTime") },
            { "MinTime", new SimpleReflectionPropertyHandler<float, ICameraShot, ICameraShotGetter>("MinTime") },
            { "TargetPercentBetweenActors", new SimpleReflectionPropertyHandler<float, ICameraShot, ICameraShotGetter>("TargetPercentBetweenActors") },
            { "NearTargetDistance", new SimpleReflectionPropertyHandler<float, ICameraShot, ICameraShotGetter>("NearTargetDistance") },
            { "ImageSpaceModifier", new SimpleReflectionFormLinkPropertyHandler<IImageSpaceAdapterGetter, ICameraShot, ICameraShotGetter>("ImageSpaceModifier") },
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not ICameraShotGetter cameraShot)
            {
                throw new InvalidOperationException($"Expected ICameraShotGetter but got {winningContext.Record.GetType()}");
            }

            return cameraShot
                .ToLink<ICameraShotGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ICameraShot, ICameraShotGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
