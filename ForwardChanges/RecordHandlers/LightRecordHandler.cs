using System;
using System.Drawing;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Light;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.RecordHandlers
{
    public class LightRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "MajorFlags", new MajorFlagsHandler() },
            { "Name", new NameHandler() },
            { "VirtualMachineAdapter", new VirtualMachineAdapterHandler() },
            { "ObjectBounds", new PropertyHandlers.Light.ObjectBoundsHandler() },
            { "Model", new PropertyHandlers.Light.ModelHandler() },
            { "Icons", new IconsHandler() },
            { "Destructible", new DestructibleHandler() },
            { "Time", new SimpleReflectionPropertyHandler<int, ILight, ILightGetter>("Time") },
            { "Radius", new SimpleReflectionPropertyHandler<uint, ILight, ILightGetter>("Radius") },
            { "Color", new SimpleReflectionPropertyHandler<Color, ILight, ILightGetter>("Color") },
            { "Flags", new FlagsHandler() },
            { "FalloffExponent", new SimpleReflectionPropertyHandler<float, ILight, ILightGetter>("FalloffExponent") },
            { "FOV", new SimpleReflectionPropertyHandler<float, ILight, ILightGetter>("FOV") },
            { "NearClip", new SimpleReflectionPropertyHandler<float, ILight, ILightGetter>("NearClip") },
            { "FlickerPeriod", new SimpleReflectionPropertyHandler<float, ILight, ILightGetter>("FlickerPeriod") },
            { "FlickerIntensityAmplitude", new SimpleReflectionPropertyHandler<float, ILight, ILightGetter>("FlickerIntensityAmplitude") },
            { "FlickerMovementAmplitude", new SimpleReflectionPropertyHandler<float, ILight, ILightGetter>("FlickerMovementAmplitude") },
            { "Value", new ValueHandler() },
            { "Weight", new WeightHandler() },
            { "FadeValue", new SimpleReflectionPropertyHandler<float, ILight, ILightGetter>("FadeValue") },
            { "Sound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, ILight, ILightGetter>("Sound") },
            { "Lens", new SimpleReflectionFormLinkPropertyHandler<ILensFlareGetter, ILight, ILightGetter>("Lens") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not ILightGetter lightRecord)
            {
                throw new InvalidOperationException($"Expected ILightGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = lightRecord
                .ToLink<ILightGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ILight, ILightGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }
    }
}
