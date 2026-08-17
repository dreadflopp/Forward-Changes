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
    // - Generalized: colors, scalars, and nested ambient color blocks via reflection handlers.
    // - Kept specialized: none.
    // - Rationale: the template is a structured value object with no bespoke list semantics.
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
            { "Unknown", new SimpleReflectionPropertyHandler<int, ILightingTemplate, ILightingTemplateGetter>("Unknown") },
            { "DirectionalAmbientColors", new ComplexReflectionPropertyHandler<IAmbientColorsGetter, ILightingTemplate, ILightingTemplateGetter>("DirectionalAmbientColors") },
            { "DATADataTypeState", new SimpleReflectionPropertyHandler<Mutagen.Bethesda.Skyrim.LightingTemplate.DATADataType, ILightingTemplate, ILightingTemplateGetter>("DATADataTypeState") }
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