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

// Migration note:
// - Generalized: NAVM binary payloads and nested data via generic handlers.
// - Kept specialized: none.
// - Rationale: compact interface and existing binary/complex handler coverage.
public class NavigationMeshRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Data", new ComplexReflectionPropertyHandler<INavigationMeshDataGetter, INavigationMesh, INavigationMeshGetter>("Data") },
        { "ONAM", new SimpleReflectionBinaryDataPropertyHandler<INavigationMesh, INavigationMeshGetter>("ONAM") },
        { "PNAM", new SimpleReflectionBinaryDataPropertyHandler<INavigationMesh, INavigationMeshGetter>("PNAM") },
        { "NNAM", new SimpleReflectionBinaryDataPropertyHandler<INavigationMesh, INavigationMeshGetter>("NNAM") },
        { "MajorFlags", new SimpleReflectionFlagPropertyHandler<NavigationMesh.MajorFlag, INavigationMesh, INavigationMeshGetter>("MajorFlags") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not INavigationMeshGetter navigationMeshRecord)
        {
            throw new InvalidOperationException($"Expected INavigationMeshGetter but got {winningContext.Record.GetType()}");
        }

        return navigationMeshRecord
            .ToLink<INavigationMeshGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, INavigationMesh, INavigationMeshGetter>(state.LinkCache)
            .ToArray();
    }
}