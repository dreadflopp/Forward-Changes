using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Light;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.RecordHandlers
{
    public class LightRecordHandler : AbstractRecordHandler
    {
        private readonly Dictionary<string, IPropertyHandler> _propertyHandlers;

        public override Dictionary<string, IPropertyHandler> PropertyHandlers => _propertyHandlers;

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

        public LightRecordHandler()
        {
            _propertyHandlers = new Dictionary<string, IPropertyHandler>
            {
                // Base properties
                { "EditorID", new EditorIDHandler() },
                { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
                { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
                { "MajorFlags", new MajorFlagsHandler() },
                
                // Light-specific properties
                { "Name", new NameHandler() },
                { "Time", new TimeHandler() },
                { "Radius", new RadiusHandler() },
                { "Color", new ColorHandler() },
                { "Flags", new FlagsHandler() },
                { "FalloffExponent", new FalloffExponentHandler() },
                { "FOV", new FOVHandler() },
                { "NearClip", new NearClipHandler() },
                { "FlickerPeriod", new FlickerPeriodHandler() },
                { "FlickerIntensityAmplitude", new FlickerIntensityAmplitudeHandler() },
                { "FlickerMovementAmplitude", new FlickerMovementAmplitudeHandler() },
                { "Value", new ValueHandler() },
                { "Weight", new WeightHandler() },
                { "FadeValue", new FadeValueHandler() },
                { "Sound", new SoundHandler() },
                { "Lens", new LensHandler() },
                
                // Complex properties
                { "VirtualMachineAdapter", new VirtualMachineAdapterHandler() },
                { "ObjectBounds", new PropertyHandlers.Light.ObjectBoundsHandler() },
                { "Model", new PropertyHandlers.Light.ModelHandler() },
                { "Destructible", new DestructibleHandler() },
                { "Icons", new IconsHandler() }
            };
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}
