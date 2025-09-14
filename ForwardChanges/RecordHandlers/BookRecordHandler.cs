using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Book;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    public class BookRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Name", new NameHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "Model", new ModelHandler() },
            { "Value", new ValueHandler() },
            { "Weight", new WeightHandler() },
            { "Description", new DescriptionHandler() },
            { "PickUpSound", new PickUpSoundHandler() },
            { "PutDownSound", new PutDownSoundHandler() },
            { "Keywords", new KeywordListHandler() },
            { "BookText", new BookTextHandler() },
            { "Destructible", new DestructibleHandler() },
            { "Flags", new FlagsHandler() },
            { "Type", new TypeHandler() },
            { "Unused", new UnusedHandler() },
            { "Teaches", new TeachesHandler() },
            { "InventoryArt", new InventoryArtHandler() },
            { "VirtualMachineAdapter", new VirtualMachineAdapterHandler() },
            { "Icons", new IconsHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IBookGetter bookRecord)
            {
                throw new InvalidOperationException($"Expected IBookGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = bookRecord
                .ToLink<IBookGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IBook, IBookGetter>(state.LinkCache)
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
                        // Property doesn't exist on this book type - just continue
                        Console.WriteLine($"Warning: Property {propertyName} not available on book {record.FormKey}: {ex.Message}");
                    }
                }
            }
        }
    }
}