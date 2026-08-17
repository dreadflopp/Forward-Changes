using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
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
            { "Unused", new SimpleReflectionPropertyHandler<ushort, IBook, IBookGetter>("Unused") },
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