using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Door;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: VM adapter, bounds, name, model, sounds, and major flags via shared handlers.
    // - Kept specialized: Destructible via dedicated destructible handler.
    // - Rationale: preserve destructible deep-copy semantics while reusing stable scalar/link handlers.
    public class DoorRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "VirtualMachineAdapter", new SimpleReflectionVirtualMachineAdapterHandler<IDoor, IDoorGetter>() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "Name", new NameHandler() },
            { "Model", new ModelHandler() },
            { "Destructible", new DestructibleHandler() },
            { "OpenSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IDoor, IDoorGetter>("OpenSound") },
            { "CloseSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IDoor, IDoorGetter>("CloseSound") },
            { "LoopSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IDoor, IDoorGetter>("LoopSound") },
            { "Flags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Door.Flag, IDoor, IDoorGetter>("Flags") },
            { "MajorFlags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Door.MajorFlag, IDoor, IDoorGetter>("MajorFlags") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IDoorGetter door)
            {
                throw new InvalidOperationException($"Expected IDoorGetter but got {winningContext.Record.GetType()}");
            }

            return door
                .ToLink<IDoorGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IDoor, IDoorGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
