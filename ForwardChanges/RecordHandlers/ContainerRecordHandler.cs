using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.Container;
using ForwardChanges.PropertyHandlers.General;
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
            { "Name", new NameHandler() },
            { "EditorID", new EditorIDHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "Model", new ModelHandler() },
            { "Weight", new WeightHandler() },
            { "Items", new ItemHandler() },
            { "VirtualMachineAdapter", new VirtualMachineAdapterHandler() },
            { "Destructible", new DestructibleHandler() },
            { "Flags", new FlagsHandler() },
            { "OpenSound", new OpenSoundHandler() },
            { "CloseSound", new CloseSoundHandler() },
            { "MajorFlags", new MajorFlagsHandler() }
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
                    handler.SetValue(record, value);
                }
            }
        }
    }
}