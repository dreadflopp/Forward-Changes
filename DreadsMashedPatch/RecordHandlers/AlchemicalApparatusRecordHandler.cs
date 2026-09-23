using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.AlchemicalApparatus;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.RecordHandlers.Abstracts;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: Quality, Description, PickUpSound, PutDownSound, and VM via reflection handlers.
    // - Kept specialized: Name/ObjectBounds/Model/Icons/Destructible/Value/Weight via existing shared handlers.
    // - Rationale: follows established item handler pattern used by adjacent record handlers.
    public class AlchemicalApparatusRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "VirtualMachineAdapter", new SimpleReflectionVirtualMachineAdapterHandler<IAlchemicalApparatus, IAlchemicalApparatusGetter>() },
            { "Name", new NameHandler() },
            { "ModelAndBounds", new ModelBoundsHandler() },
            { "Icons", new IconsHandler() },
            { "Destructible", new DestructibleHandler() },
            { "PickUpSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IAlchemicalApparatus, IAlchemicalApparatusGetter>("PickUpSound") },
            { "PutDownSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IAlchemicalApparatus, IAlchemicalApparatusGetter>("PutDownSound") },
            { "Quality", new SimpleReflectionPropertyHandler<QualityLevel?, IAlchemicalApparatus, IAlchemicalApparatusGetter>("Quality") },
            { "Description", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IAlchemicalApparatus, IAlchemicalApparatusGetter>("Description") },
            { "Value", new ValueHandler() },
            { "Weight", new WeightHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IAlchemicalApparatusGetter apparatus)
            {
                throw new InvalidOperationException($"Expected IAlchemicalApparatusGetter but got {winningContext.Record.GetType()}");
            }

            return apparatus
                .ToLink<IAlchemicalApparatusGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IAlchemicalApparatus, IAlchemicalApparatusGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
