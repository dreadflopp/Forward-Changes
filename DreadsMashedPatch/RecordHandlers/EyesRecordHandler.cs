using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Eyes;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: Name and flags via shared handlers.
    // - Kept specialized: Icon via dedicated texture-path-normalizing handler.
    // - Rationale: icon asset path normalization follows project texture handling policy.
    public class EyesRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Name", new NameHandler() },
            { "Icon", new IconHandler() },
            { "Flags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Eyes.Flag, IEyes, IEyesGetter>("Flags") },
            { "MajorFlags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Eyes.MajorFlag, IEyes, IEyesGetter>("MajorFlags") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IEyesGetter eyes)
            {
                throw new InvalidOperationException($"Expected IEyesGetter but got {winningContext.Record.GetType()}");
            }

            return eyes
                .ToLink<IEyesGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IEyes, IEyesGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
