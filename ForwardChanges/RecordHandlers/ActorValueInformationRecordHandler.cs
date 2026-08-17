using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;
using Noggog;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: Description, Abbreviation, CNAM, Skill, PerkTree via reflection handlers.
    // - Kept specialized: Name via existing shared name handler.
    // - Rationale: aligns with existing translated-string handling while keeping AVIF fields centralized.
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
            { "CNAM", new SimpleReflectionPropertyHandler<ReadOnlyMemorySlice<byte>?, IActorValueInformation, IActorValueInformationGetter>("CNAM") },
            { "Skill", new ComplexReflectionPropertyHandler<IActorValueSkillGetter, IActorValueInformation, IActorValueInformationGetter>("Skill") },
            { "PerkTree", new ComplexReflectionPropertyHandler<IReadOnlyList<IActorValuePerkNodeGetter>, IActorValueInformation, IActorValueInformationGetter>("PerkTree") }
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
