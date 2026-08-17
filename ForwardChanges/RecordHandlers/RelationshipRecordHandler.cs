using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers;

public class RelationshipRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Parent", new SimpleReflectionFormLinkPropertyHandler<INpcGetter, IRelationship, IRelationshipGetter>("Parent") },
        { "Child", new SimpleReflectionFormLinkPropertyHandler<INpcGetter, IRelationship, IRelationshipGetter>("Child") },
        { "Rank", new SimpleReflectionPropertyHandler<Relationship.RankType, IRelationship, IRelationshipGetter>("Rank") },
        { "Unknown", new SimpleReflectionPropertyHandler<byte, IRelationship, IRelationshipGetter>("Unknown") },
        { "Flags", new SimpleReflectionFlagPropertyHandler<Relationship.Flag, IRelationship, IRelationshipGetter>("Flags") },
        { "AssociationType", new SimpleReflectionFormLinkPropertyHandler<IAssociationTypeGetter, IRelationship, IRelationshipGetter>("AssociationType") },
        { "MajorFlags", new SimpleReflectionFlagPropertyHandler<Relationship.MajorFlag, IRelationship, IRelationshipGetter>("MajorFlags") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IRelationshipGetter relationshipRecord)
        {
            throw new InvalidOperationException($"Expected IRelationshipGetter but got {winningContext.Record.GetType()}");
        }

        return relationshipRecord
            .ToLink<IRelationshipGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IRelationship, IRelationshipGetter>(state.LinkCache)
            .ToArray();
    }
}