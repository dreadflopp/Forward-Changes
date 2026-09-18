using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Strings;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.Shout;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers;

// Migration note:
// - Generalized: SHOU text, links, and metadata use shared semantic handlers.
// - Kept specialized: WordsOfPower is an atomic non-alignable sequence; MajorFlags retains the approved flag handler.
// - Rationale: xEdit marks the word sequence as declaration-ordered without a safe row identity.
public class ShoutRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Name", new NameHandler() },
        { "MenuDisplayObject", new SimpleReflectionFormLinkPropertyHandler<IStaticGetter, IShout, IShoutGetter>("MenuDisplayObject") },
        { "Description", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IShout, IShoutGetter>("Description") },
        { "WordsOfPower", new WordsOfPowerHandler() },
        { "MajorFlags", new SimpleReflectionFlagPropertyHandler<Shout.MajorFlag, IShout, IShoutGetter>("MajorFlags") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IShoutGetter shoutRecord)
        {
            throw new InvalidOperationException($"Expected IShoutGetter but got {winningContext.Record.GetType()}");
        }

        return shoutRecord
            .ToLink<IShoutGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IShout, IShoutGetter>(state.LinkCache)
            .ToArray();
    }
}
