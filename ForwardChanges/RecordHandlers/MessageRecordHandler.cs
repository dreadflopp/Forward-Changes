using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Strings;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Message;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.RecordHandlers;

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