using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Static;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    public class StaticRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "Model", new PropertyHandlers.Static.ModelHandler() },
            { "MaxAngle", new MaxAngleHandler() },
            { "Material", new MaterialHandler() },
            { "Flags", new FlagsHandler() },
            { "Lod", new LodHandler() },
            { "MajorFlags", new MajorFlagsHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IStaticGetter staticRecord)
            {
                throw new InvalidOperationException($"Expected IStaticGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = staticRecord
                .ToLink<IStaticGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IStatic, IStaticGetter>(state.LinkCache)
                .ToArray();

            return contexts;
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
                        Console.WriteLine($"Warning: Property {propertyName} not available on static {record.FormKey}: {ex.Message}");
                    }
                }
            }
        }
    }
}
