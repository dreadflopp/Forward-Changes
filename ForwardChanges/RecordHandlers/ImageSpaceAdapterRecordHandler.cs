using System;
using Noggog;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: all scalar and list properties via shared reflection/list handlers.
    // - Kept specialized: none.
    // - Rationale: keyframe/colorframe collections map cleanly to generic list handlers.
    public class ImageSpaceAdapterRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Animatable", new SimpleReflectionPropertyHandler<bool, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Animatable") },
            { "Duration", new SimpleReflectionPropertyHandler<float, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Duration", 0.001f) },
            { "RadialBlurUseTarget", new SimpleReflectionPropertyHandler<bool, IImageSpaceAdapter, IImageSpaceAdapterGetter>("RadialBlurUseTarget") },
            { "RadialBlurCenter", new SimpleReflectionPropertyHandler<P2Float, IImageSpaceAdapter, IImageSpaceAdapterGetter>("RadialBlurCenter") },
            { "DepthOfFieldFlags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.ImageSpaceAdapter.DepthOfFieldFlag, IImageSpaceAdapter, IImageSpaceAdapterGetter>("DepthOfFieldFlags") },

            { "BlurRadius", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("BlurRadius", ListOrdering.None, true) },
            { "DoubleVisionStrength", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("DoubleVisionStrength", ListOrdering.None, true) },
            { "TintColor", new SimpleReflectionListPropertyHandler<IColorFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("TintColor", ListOrdering.None, true) },
            { "FadeColor", new SimpleReflectionListPropertyHandler<IColorFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("FadeColor", ListOrdering.None, true) },
            { "RadialBlurStrength", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("RadialBlurStrength", ListOrdering.None, true) },
            { "RadialBlurRampUp", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("RadialBlurRampUp", ListOrdering.None, true) },
            { "RadialBlurStart", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("RadialBlurStart", ListOrdering.None, true) },
            { "RadialBlurRampDown", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("RadialBlurRampDown", ListOrdering.None, true) },
            { "RadialBlurDownStart", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("RadialBlurDownStart", ListOrdering.None, true) },
            { "DepthOfFieldStrength", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("DepthOfFieldStrength", ListOrdering.None, true) },
            { "DepthOfFieldDistance", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("DepthOfFieldDistance", ListOrdering.None, true) },
            { "DepthOfFieldRange", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("DepthOfFieldRange", ListOrdering.None, true) },
            { "MotionBlurStrength", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("MotionBlurStrength", ListOrdering.None, true) },
            { "HdrEyeAdaptSpeedMult", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("HdrEyeAdaptSpeedMult", ListOrdering.None, true) },
            { "HdrEyeAdaptSpeedAdd", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("HdrEyeAdaptSpeedAdd", ListOrdering.None, true) },
            { "HdrBloomBlurRadiusMult", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("HdrBloomBlurRadiusMult", ListOrdering.None, true) },
            { "HdrBloomBlurRadiusAdd", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("HdrBloomBlurRadiusAdd", ListOrdering.None, true) },
            { "HdrBloomThresholdMult", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("HdrBloomThresholdMult", ListOrdering.None, true) },
            { "HdrBloomThresholdAdd", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("HdrBloomThresholdAdd", ListOrdering.None, true) },
            { "HdrBloomScaleMult", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("HdrBloomScaleMult", ListOrdering.None, true) },
            { "HdrBloomScaleAdd", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("HdrBloomScaleAdd", ListOrdering.None, true) },
            { "HdrTargetLumMinMult", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("HdrTargetLumMinMult", ListOrdering.None, true) },
            { "HdrTargetLumMinAdd", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("HdrTargetLumMinAdd", ListOrdering.None, true) },
            { "HdrTargetLumMaxMult", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("HdrTargetLumMaxMult", ListOrdering.None, true) },
            { "HdrTargetLumMaxAdd", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("HdrTargetLumMaxAdd", ListOrdering.None, true) },
            { "HdrSunlightScaleMult", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("HdrSunlightScaleMult", ListOrdering.None, true) },
            { "HdrSunlightScaleAdd", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("HdrSunlightScaleAdd", ListOrdering.None, true) },
            { "HdrSkyScaleMult", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("HdrSkyScaleMult", ListOrdering.None, true) },
            { "HdrSkyScaleAdd", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("HdrSkyScaleAdd", ListOrdering.None, true) },
            { "Unknown08", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Unknown08", ListOrdering.None, true) },
            { "Unknown48", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Unknown48", ListOrdering.None, true) },
            { "Unknown09", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Unknown09", ListOrdering.None, true) },
            { "Unknown49", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Unknown49", ListOrdering.None, true) },
            { "Unknown0A", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Unknown0A", ListOrdering.None, true) },
            { "Unknown4A", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Unknown4A", ListOrdering.None, true) },
            { "Unknown0B", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Unknown0B", ListOrdering.None, true) },
            { "Unknown4B", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Unknown4B", ListOrdering.None, true) },
            { "Unknown0C", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Unknown0C", ListOrdering.None, true) },
            { "Unknown4C", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Unknown4C", ListOrdering.None, true) },
            { "Unknown0D", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Unknown0D", ListOrdering.None, true) },
            { "Unknown4D", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Unknown4D", ListOrdering.None, true) },
            { "Unknown0E", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Unknown0E", ListOrdering.None, true) },
            { "Unknown4E", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Unknown4E", ListOrdering.None, true) },
            { "Unknown0F", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Unknown0F", ListOrdering.None, true) },
            { "Unknown4F", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Unknown4F", ListOrdering.None, true) },
            { "Unknown10", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Unknown10", ListOrdering.None, true) },
            { "Unknown50", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Unknown50", ListOrdering.None, true) },
            { "CinematicSaturationMult", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("CinematicSaturationMult", ListOrdering.None, true) },
            { "CinematicSaturationAdd", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("CinematicSaturationAdd", ListOrdering.None, true) },
            { "CinematicBrightnessMult", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("CinematicBrightnessMult", ListOrdering.None, true) },
            { "CinematicBrightnessAdd", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("CinematicBrightnessAdd", ListOrdering.None, true) },
            { "CinematicContrastMult", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("CinematicContrastMult", ListOrdering.None, true) },
            { "CinematicContrastAdd", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("CinematicContrastAdd", ListOrdering.None, true) },
            { "Unknown14", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Unknown14", ListOrdering.None, true) },
            { "Unknown54", new SimpleReflectionListPropertyHandler<IKeyFrameGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>("Unknown54", ListOrdering.None, true) }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IImageSpaceAdapterGetter imageSpaceAdapter)
            {
                throw new InvalidOperationException($"Expected IImageSpaceAdapterGetter but got {winningContext.Record.GetType()}");
            }

            return imageSpaceAdapter
                .ToLink<IImageSpaceAdapterGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IImageSpaceAdapter, IImageSpaceAdapterGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
