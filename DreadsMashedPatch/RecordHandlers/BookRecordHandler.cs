using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.Book;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using System;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: BOOK text, links, flags, and scalar fields use shared semantic handlers.
    // - Kept specialized: Teaches and Icons retain their typed aggregate handlers.
    // - Intentionally excluded: Unused is non-semantic storage; the winning value is preserved.
    public class BookRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Name", new NameHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "Model", new ModelHandler() },
            { "Value", new ValueHandler() },
            { "Weight", new WeightHandler() },
            { "Description", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IBook, IBookGetter>("Description") },
            { "PickUpSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IBook, IBookGetter>("PickUpSound") },
            { "PutDownSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IBook, IBookGetter>("PutDownSound") },
            { "Keywords", new KeywordListHandler() },
            { "BookText", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IBook, IBookGetter>("BookText") },
            { "Destructible", new DestructibleHandler() },
            { "Flags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Book.Flag, IBook, IBookGetter>("Flags") },
            { "Type", new SimpleReflectionPropertyHandler<Mutagen.Bethesda.Skyrim.Book.BookType, IBook, IBookGetter>("Type") },
            { "Teaches", new TeachesHandler() },
            { "InventoryArt", new SimpleReflectionFormLinkPropertyHandler<IStaticGetter, IBook, IBookGetter>("InventoryArt") },
            { "VirtualMachineAdapter", new SimpleReflectionVirtualMachineAdapterHandler<IBook, IBookGetter>() },
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

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}
