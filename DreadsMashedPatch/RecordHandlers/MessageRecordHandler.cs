using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Strings;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Message;
using DreadsMashedPatch.PropertyHandlers.Interfaces;

namespace DreadsMashedPatch.RecordHandlers;

// Migration note:
// - Generalized: MESG text, binary data, links, flags, and scalars use shared semantic handlers.
// - Kept specialized: MenuButtons retains aligned ordered rows and generated item copying.
// - Rationale: button declaration order is meaningful and must be reconciled independently from scalar fields.
public class MessageRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Description", new ComplexReflectionPropertyHandler<TranslatedString, IMessage, IMessageGetter>("Description") },
        { "Name", new ComplexReflectionPropertyHandler<TranslatedString, IMessage, IMessageGetter>("Name") },
        { "INAM", new SimpleReflectionBinaryDataPropertyHandler<IMessage, IMessageGetter>("INAM") },
        { "Quest", new SimpleReflectionFormLinkPropertyHandler<IQuestGetter, IMessage, IMessageGetter>("Quest") },
        { "Flags", new SimpleReflectionFlagPropertyHandler<Message.Flag, IMessage, IMessageGetter>("Flags") },
        { "DisplayTime", new SimpleReflectionPropertyHandler<uint?, IMessage, IMessageGetter>("DisplayTime") },
        { "MenuButtons", new MenuButtonsHandler() }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IMessageGetter messageRecord)
        {
            throw new InvalidOperationException($"Expected IMessageGetter but got {winningContext.Record.GetType()}");
        }

        return messageRecord
            .ToLink<IMessageGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IMessage, IMessageGetter>(state.LinkCache)
            .ToArray();
    }
}
