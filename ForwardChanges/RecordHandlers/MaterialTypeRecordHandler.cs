using System;
using System.Collections.Generic;
using System.Drawing;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.RecordHandlers;

public class MaterialTypeRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Parent", new SimpleReflectionFormLinkPropertyHandler<IMaterialTypeGetter, IMaterialType, IMaterialTypeGetter>("Parent") },
        { "Name", new NameHandler() },
        { "HavokDisplayColor", new SimpleReflectionPropertyHandler<Color?, IMaterialType, IMaterialTypeGetter>("HavokDisplayColor") },
        { "Buoyancy", new SimpleReflectionPropertyHandler<float?, IMaterialType, IMaterialTypeGetter>("Buoyancy") },
        { "Flags", new SimpleReflectionFlagPropertyHandler<MaterialType.Flag, IMaterialType, IMaterialTypeGetter>("Flags") },
        { "HavokImpactDataSet", new SimpleReflectionFormLinkPropertyHandler<IImpactDataSetGetter, IMaterialType, IMaterialTypeGetter>("HavokImpactDataSet") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IMaterialTypeGetter materialTypeRecord)
        {
            throw new InvalidOperationException($"Expected IMaterialTypeGetter but got {winningContext.Record.GetType()}");
        }

        return materialTypeRecord
            .ToLink<IMaterialTypeGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IMaterialType, IMaterialTypeGetter>(state.LinkCache)
            .ToArray();
    }
}