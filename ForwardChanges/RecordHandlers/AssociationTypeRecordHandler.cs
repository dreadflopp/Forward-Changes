using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: ParentTitle, Title, IsFamily via reflection handlers.
    // - Kept specialized: none.
    // - Rationale: small surface with reflection-safe properties.
    public class AssociationTypeRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "ParentTitle", new ComplexReflectionPropertyHandler<IGenderedItemGetter<string?>, IAssociationType, IAssociationTypeGetter>("ParentTitle") },
            { "Title", new ComplexReflectionPropertyHandler<IGenderedItemGetter<string?>, IAssociationType, IAssociationTypeGetter>("Title") },
            { "IsFamily", new SimpleReflectionPropertyHandler<bool?, IAssociationType, IAssociationTypeGetter>("IsFamily") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IAssociationTypeGetter associationType)
            {
                throw new InvalidOperationException($"Expected IAssociationTypeGetter but got {winningContext.Record.GetType()}");
            }

            return associationType
                .ToLink<IAssociationTypeGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IAssociationType, IAssociationTypeGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
