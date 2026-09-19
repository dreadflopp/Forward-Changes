using System;
using System.Drawing;
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
    // - Generalized: colors, scalars, and nested ambient color blocks via reflection handlers.
    // - Kept specialized: none.
    // - Intentionally excluded: DATADataTypeState is Mutagen serialization state; Unknown is outside the semantic conflict surface.
    // - Rationale: semantic fields are forwarded while the winning record retains its binary DATA layout.
    public class LightingTemplateRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "AmbientColor", new SimpleReflectionPropertyHandler<Color, ILightingTemplate, ILightingTemplateGetter>("AmbientColor") },
            { "DirectionalColor", new SimpleReflectionPropertyHandler<Color, ILightingTemplate, ILightingTemplateGetter>("DirectionalColor") },
            { "FogNearColor", new SimpleReflectionPropertyHandler<Color, ILightingTemplate, ILightingTemplateGetter>("FogNearColor") },
            { "FogNear", new SimpleReflectionPropertyHandler<float, ILightingTemplate, ILightingTemplateGetter>("FogNear") },
            { "FogFar", new SimpleReflectionPropertyHandler<float, ILightingTemplate, ILightingTemplateGetter>("FogFar") },
            { "DirectionalRotationXY", new SimpleReflectionPropertyHandler<int, ILightingTemplate, ILightingTemplateGetter>("DirectionalRotationXY") },
            { "DirectionalRotationZ", new SimpleReflectionPropertyHandler<int, ILightingTemplate, ILightingTemplateGetter>("DirectionalRotationZ") },
            { "DirectionalFade", new SimpleReflectionPropertyHandler<float, ILightingTemplate, ILightingTemplateGetter>("DirectionalFade") },
            { "FogClipDistance", new SimpleReflectionPropertyHandler<float, ILightingTemplate, ILightingTemplateGetter>("FogClipDistance") },
            { "FogPower", new SimpleReflectionPropertyHandler<float, ILightingTemplate, ILightingTemplateGetter>("FogPower") },
            { "AmbientColors", new ComplexReflectionPropertyHandler<IAmbientColorsGetter, ILightingTemplate, ILightingTemplateGetter>("AmbientColors") },
            { "FogFarColor", new SimpleReflectionPropertyHandler<Color, ILightingTemplate, ILightingTemplateGetter>("FogFarColor") },
            { "FogMax", new SimpleReflectionPropertyHandler<float, ILightingTemplate, ILightingTemplateGetter>("FogMax") },
            { "LightFadeStartDistance", new SimpleReflectionPropertyHandler<float, ILightingTemplate, ILightingTemplateGetter>("LightFadeStartDistance") },
            { "LightFadeEndDistance", new SimpleReflectionPropertyHandler<float, ILightingTemplate, ILightingTemplateGetter>("LightFadeEndDistance") },
            { "DirectionalAmbientColors", new ComplexReflectionPropertyHandler<IAmbientColorsGetter, ILightingTemplate, ILightingTemplateGetter>("DirectionalAmbientColors") },
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not ILightingTemplateGetter lightingTemplate)
            {
                throw new InvalidOperationException($"Expected ILightingTemplateGetter but got {winningContext.Record.GetType()}");
            }

            return lightingTemplate
                .ToLink<ILightingTemplateGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ILightingTemplate, ILightingTemplateGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
