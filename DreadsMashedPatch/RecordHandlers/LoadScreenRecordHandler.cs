using System;
using System.Drawing;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using Noggog;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: icons, translated text, links, and scalars via reflection handlers.
    // - Kept specialized: Conditions uses the shared polymorphic condition handler.
    // - Rationale: Condition is abstract and must be copied through generated subtype dispatch.
    public class LoadScreenRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Icons", new SimpleReflectionIconsPropertyHandler<ILoadScreen, ILoadScreenGetter>("Icons") },
            { "Description", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, ILoadScreen, ILoadScreenGetter>("Description") },
            { "Conditions", new ConditionsHandler<ILoadScreen, ILoadScreenGetter>(record => record.Conditions, record => record.Conditions) },
            { "LoadingScreenNif", new SimpleReflectionFormLinkPropertyHandler<IStaticGetter, ILoadScreen, ILoadScreenGetter>("LoadingScreenNif") },
            { "InitialScale", new SimpleReflectionPropertyHandler<float?, ILoadScreen, ILoadScreenGetter>("InitialScale") },
            { "InitialRotation", new SimpleReflectionPropertyHandler<P3Int16?, ILoadScreen, ILoadScreenGetter>("InitialRotation") },
            { "RotationOffsetConstraints", new ComplexReflectionPropertyHandler<IInt16MinMaxGetter, ILoadScreen, ILoadScreenGetter>("RotationOffsetConstraints") },
            { "InitialTranslationOffset", new SimpleReflectionPropertyHandler<P3Float?, ILoadScreen, ILoadScreenGetter>("InitialTranslationOffset") },
            { "CameraPath", new SimpleReflectionAssetLinkPropertyHandler<SkyrimModelAssetType, ILoadScreen, ILoadScreenGetter>("CameraPath") },
            { "MajorFlags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.LoadScreen.MajorFlag, ILoadScreen, ILoadScreenGetter>("MajorFlags") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not ILoadScreenGetter loadScreen)
            {
                throw new InvalidOperationException($"Expected ILoadScreenGetter but got {winningContext.Record.GetType()}");
            }

            return loadScreen
                .ToLink<ILoadScreenGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ILoadScreen, ILoadScreenGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
