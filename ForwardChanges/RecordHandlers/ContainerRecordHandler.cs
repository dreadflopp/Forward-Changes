using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.Container;
using ForwardChanges.PropertyHandlers.General;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: independent CONT scalar, link, model, VMAD, and header fields use shared handlers.
    // - Kept specialized: Items keeps FormID-keyed entry ownership; destructible data and flags retain semantic handlers.
    // - Rationale: item count/metadata changes must replace the entry under the same item key.
    public class ContainerRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "Name", new NameHandler() },
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "Model", new ModelHandler() },
            { "Weight", new SimpleReflectionPropertyHandler<float, IContainer, IContainerGetter>("Weight") },
            { "Items", new ItemHandler() },
            { "VirtualMachineAdapter", new SimpleReflectionVirtualMachineAdapterHandler<IContainer, IContainerGetter>() },
            { "Destructible", new DestructibleHandler() },
            { "Flags", new FlagsHandler() },
            { "OpenSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IContainer, IContainerGetter>("OpenSound") },
            { "CloseSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IContainer, IContainerGetter>("CloseSound") },
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

        // ApplyForwardedProperties is now handled by the base class
        // The base class automatically handles flag property coordination
    }
}
