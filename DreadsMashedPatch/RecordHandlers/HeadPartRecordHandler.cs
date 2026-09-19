using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.HeadPart;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: scalar, flag, link, model, and ExtraParts fields use shared handlers.
    // - Kept specialized: Parts preserves ordered atomic NAM0/NAM1 pairs with Mutagen-generated deep copying.
    // - Rationale: binary-overlay filenames are getter asset links and cannot be assigned to mutable asset links by reflection.
    public class HeadPartRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Name", new NameHandler() },
            { "Model", new ModelHandler() },
            { "Flags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.HeadPart.Flag, IHeadPart, IHeadPartGetter>("Flags") },
            { "Type", new SimpleReflectionPropertyHandler<Mutagen.Bethesda.Skyrim.HeadPart.TypeEnum?, IHeadPart, IHeadPartGetter>("Type") },
            { "ExtraParts", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IHeadPartGetter>, IHeadPart, IHeadPartGetter>("ExtraParts", ListSemantics.SortedKeyed) },
            { "Parts", new PartsHandler() },
            { "TextureSet", new SimpleReflectionFormLinkPropertyHandler<ITextureSetGetter, IHeadPart, IHeadPartGetter>("TextureSet") },
            { "Color", new SimpleReflectionFormLinkPropertyHandler<IColorRecordGetter, IHeadPart, IHeadPartGetter>("Color") },
            { "ValidRaces", new SimpleReflectionFormLinkPropertyHandler<IFormListGetter, IHeadPart, IHeadPartGetter>("ValidRaces") },
            { "MajorFlags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.HeadPart.MajorFlag, IHeadPart, IHeadPartGetter>("MajorFlags") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IHeadPartGetter headPart)
            {
                throw new InvalidOperationException($"Expected IHeadPartGetter but got {winningContext.Record.GetType()}");
            }

            return headPart
                .ToLink<IHeadPartGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IHeadPart, IHeadPartGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
