using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Noggog;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.ImageSpaceAdapter;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: record metadata via shared handlers.
    // - Kept specialized: ordered curves, radial blur, depth of field, and Mult/Add pairs are atomic units.
    // - Intentionally excluded: Unknown* keyframe collections are outside the semantic conflict surface.
    // - Rationale: merging individual keyframes can invent curves and invalidates the DNAM-derived counts.
    public class ImageSpaceAdapterRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "AnimationSettings", new AnimationSettingsHandler() },
            { "BlurRadius", KeyFrameCurve("BlurRadius", a => a.BlurRadius, (a, v) => a.BlurRadius = v) },
            { "DoubleVisionStrength", KeyFrameCurve("DoubleVisionStrength", a => a.DoubleVisionStrength, (a, v) => a.DoubleVisionStrength = v) },
            { "TintColor", ColorFrameCurve("TintColor", a => a.TintColor, (a, v) => a.TintColor = v) },
            { "FadeColor", ColorFrameCurve("FadeColor", a => a.FadeColor, (a, v) => a.FadeColor = v) },
            { "RadialBlur", new RadialBlurHandler() },
            { "DepthOfField", new DepthOfFieldHandler() },
            { "MotionBlurStrength", KeyFrameCurve("MotionBlurStrength", a => a.MotionBlurStrength, (a, v) => a.MotionBlurStrength = v) },
            { "HdrEyeAdaptSpeed", KeyFramePair("HdrEyeAdaptSpeed", a => a.HdrEyeAdaptSpeedMult, a => a.HdrEyeAdaptSpeedAdd, (a, v) => a.HdrEyeAdaptSpeedMult = v, (a, v) => a.HdrEyeAdaptSpeedAdd = v) },
            { "HdrBloomBlurRadius", KeyFramePair("HdrBloomBlurRadius", a => a.HdrBloomBlurRadiusMult, a => a.HdrBloomBlurRadiusAdd, (a, v) => a.HdrBloomBlurRadiusMult = v, (a, v) => a.HdrBloomBlurRadiusAdd = v) },
            { "HdrBloomThreshold", KeyFramePair("HdrBloomThreshold", a => a.HdrBloomThresholdMult, a => a.HdrBloomThresholdAdd, (a, v) => a.HdrBloomThresholdMult = v, (a, v) => a.HdrBloomThresholdAdd = v) },
            { "HdrBloomScale", KeyFramePair("HdrBloomScale", a => a.HdrBloomScaleMult, a => a.HdrBloomScaleAdd, (a, v) => a.HdrBloomScaleMult = v, (a, v) => a.HdrBloomScaleAdd = v) },
            { "HdrTargetLumMin", KeyFramePair("HdrTargetLumMin", a => a.HdrTargetLumMinMult, a => a.HdrTargetLumMinAdd, (a, v) => a.HdrTargetLumMinMult = v, (a, v) => a.HdrTargetLumMinAdd = v) },
            { "HdrTargetLumMax", KeyFramePair("HdrTargetLumMax", a => a.HdrTargetLumMaxMult, a => a.HdrTargetLumMaxAdd, (a, v) => a.HdrTargetLumMaxMult = v, (a, v) => a.HdrTargetLumMaxAdd = v) },
            { "HdrSunlightScale", KeyFramePair("HdrSunlightScale", a => a.HdrSunlightScaleMult, a => a.HdrSunlightScaleAdd, (a, v) => a.HdrSunlightScaleMult = v, (a, v) => a.HdrSunlightScaleAdd = v) },
            { "HdrSkyScale", KeyFramePair("HdrSkyScale", a => a.HdrSkyScaleMult, a => a.HdrSkyScaleAdd, (a, v) => a.HdrSkyScaleMult = v, (a, v) => a.HdrSkyScaleAdd = v) },
            { "CinematicSaturation", KeyFramePair("CinematicSaturation", a => a.CinematicSaturationMult, a => a.CinematicSaturationAdd, (a, v) => a.CinematicSaturationMult = v, (a, v) => a.CinematicSaturationAdd = v) },
            { "CinematicBrightness", KeyFramePair("CinematicBrightness", a => a.CinematicBrightnessMult, a => a.CinematicBrightnessAdd, (a, v) => a.CinematicBrightnessMult = v, (a, v) => a.CinematicBrightnessAdd = v) },
            { "CinematicContrast", KeyFramePair("CinematicContrast", a => a.CinematicContrastMult, a => a.CinematicContrastAdd, (a, v) => a.CinematicContrastMult = v, (a, v) => a.CinematicContrastAdd = v) },
        };

        private static AtomicKeyFrameCurveHandler KeyFrameCurve(
            string propertyName,
            Func<IImageSpaceAdapterGetter, IReadOnlyList<IKeyFrameGetter>?> getter,
            Action<IImageSpaceAdapter, ExtendedList<KeyFrame>?> setter) =>
            new(propertyName, getter, setter);

        private static AtomicColorFrameCurveHandler ColorFrameCurve(
            string propertyName,
            Func<IImageSpaceAdapterGetter, IReadOnlyList<IColorFrameGetter>?> getter,
            Action<IImageSpaceAdapter, ExtendedList<ColorFrame>?> setter) =>
            new(propertyName, getter, setter);

        private static KeyFrameCurvePairHandler KeyFramePair(
            string propertyName,
            Func<IImageSpaceAdapterGetter, IReadOnlyList<IKeyFrameGetter>?> multGetter,
            Func<IImageSpaceAdapterGetter, IReadOnlyList<IKeyFrameGetter>?> addGetter,
            Action<IImageSpaceAdapter, ExtendedList<KeyFrame>?> multSetter,
            Action<IImageSpaceAdapter, ExtendedList<KeyFrame>?> addSetter) =>
            new(propertyName, multGetter, addGetter, multSetter, addSetter);

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
