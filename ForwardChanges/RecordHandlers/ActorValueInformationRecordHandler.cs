using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.ActorValueInformation;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: Description, Abbreviation, Skill via reflection handlers; CNAM via the shared binary-data handler.
    // - Kept specialized: Name via the shared name handler; PerkTree via a record-specific structural handler.
    // - Rationale: PerkTree is a get-only mutable collection whose nested binary and list data require Mutagen's
    //   generated deep-copy and equality semantics.
    public class ActorValueInformationRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Name", new NameHandler() },
            { "Description", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IActorValueInformation, IActorValueInformationGetter>("Description") },
            { "Abbreviation", new SimpleReflectionPropertyHandler<string, IActorValueInformation, IActorValueInformationGetter>("Abbreviation") },
            { "CNAM", new SimpleReflectionBinaryDataPropertyHandler<IActorValueInformation, IActorValueInformationGetter>("CNAM") },
            { "Skill", new ComplexReflectionPropertyHandler<IActorValueSkillGetter, IActorValueInformation, IActorValueInformationGetter>("Skill") },
            { "PerkTree", new PerkTreeHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IActorValueInformationGetter actorValueInformation)
            {
                throw new InvalidOperationException($"Expected IActorValueInformationGetter but got {winningContext.Record.GetType()}");
            }

            return actorValueInformation
                .ToLink<IActorValueInformationGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IActorValueInformation, IActorValueInformationGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
