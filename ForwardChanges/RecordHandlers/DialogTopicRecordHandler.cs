using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    public class DialogTopicRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDPropertyHandler() },
            { "Name", new NamePropertyHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IDialogTopicGetter dialogTopicRecord)
            {
                throw new InvalidOperationException($"Expected IDialogTopicGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = dialogTopicRecord
                .ToLink<IDialogTopicGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IDialogTopic, IDialogTopicGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        public override IMajorRecord GetOverrideRecord(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            // Use the context's GetOrAddAsOverride method like Fusion does
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
                        if (value != null)
                        {
                            handler.SetValue(record, value);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Property doesn't exist on this dialog topic type - just continue
                        Console.WriteLine($"Warning: Property {propertyName} not available on dialog topic {record.FormKey}: {ex.Message}");
                    }
                }
            }
        }
    }
}