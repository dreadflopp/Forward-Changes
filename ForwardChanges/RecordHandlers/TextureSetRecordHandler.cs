using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.TextureSet;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.RecordHandlers
{
    public class TextureSetRecordHandler : AbstractRecordHandler
    {
        private readonly Dictionary<string, IPropertyHandler> _propertyHandlers;

        public TextureSetRecordHandler()
        {
            _propertyHandlers = new Dictionary<string, IPropertyHandler>
            {
                { "EditorID", new EditorIDHandler() },
                { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
                { "ObjectBounds", new ObjectBoundsHandler() },
                { "Diffuse", new DiffuseHandler() },
                { "NormalOrGloss", new NormalOrGlossHandler() },
                { "EnvironmentMaskOrSubsurfaceTint", new EnvironmentMaskOrSubsurfaceTintHandler() },
                { "GlowOrDetailMap", new GlowOrDetailMapHandler() },
                { "Height", new HeightHandler() },
                { "Environment", new EnvironmentHandler() },
                { "Multilayer", new MultilayerHandler() },
                { "BacklightMaskOrSpecular", new BacklightMaskOrSpecularHandler() },
                { "Decal", new DecalHandler() },
                { "Flags", new FlagsHandler() }
            };
        }

        public override Dictionary<string, IPropertyHandler> PropertyHandlers => _propertyHandlers;

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not ITextureSetGetter textureSetRecord)
                throw new InvalidOperationException($"Expected ITextureSetGetter but got {winningContext.Record.GetType()}");

            return textureSetRecord
                .ToLink<ITextureSetGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, ITextureSet, ITextureSetGetter>(state.LinkCache)
                .ToArray();
        }

        public override IMajorRecord GetOverrideRecord(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            return winningContext.GetOrAddAsOverride(state.PatchMod);
        }

        public override void ApplyForwardedProperties(IMajorRecord record, Dictionary<string, object?> propertiesToForward)
        {
            foreach (var (propertyName, value) in propertiesToForward)
            {
                if (PropertyHandlers.TryGetValue(propertyName, out var handler))
                {
                    try
                    {
                        Console.WriteLine($"[{propertyName}] Applying value: {handler.FormatValue(value)}, Type: {value?.GetType()}");
                        handler.SetValue(record, value);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Property {propertyName} not available on texture set {record.FormKey}: {ex.Message}");
                    }
                }
            }
        }
    }
}
