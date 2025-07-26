using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers;
using ForwardChanges.Contexts;
using ForwardChanges.Contexts.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ForwardChanges.RecordHandlers
{
    public class ContainerRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "Name", new NamePropertyHandler() },
            { "EditorID", new EditorIDPropertyHandler() },
            { "Items", new ContainerItemListPropertyHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IContainerGetter containerRecord)
            {
                throw new InvalidOperationException($"Expected IContainerGetter but got {winningContext.Record.GetType()}");
            }

            return containerRecord
                .ToLink<IContainerGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IContainer, IContainerGetter>(state.LinkCache)
                .ToArray();
        }

        public override IMajorRecord GetOverrideRecord(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            return state.PatchMod.Containers.GetOrAddAsOverride(winningContext.Record);
        }

        public override void ApplyForwardedProperties(IMajorRecord record, Dictionary<string, object?> propertiesToForward)
        {
            foreach (var (propertyName, value) in propertiesToForward)
            {
                if (PropertyHandlers.TryGetValue(propertyName, out var handler))
                {
                    Console.WriteLine($"[{propertyName}] Applying value: {handler.FormatValue(value)}, Type: {value?.GetType()}");
                    if (value != null)
                    {
                        handler.SetValue(record, value);
                    }
                }
            }
        }
    }
}