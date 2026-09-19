using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Binary.Streams;
using Noggog;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.MaterialObject;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.RecordHandlers;

// Migration note:
// - Generalized: MATO scalar, vector, color, model, and list fields use existing handlers.
// - Kept specialized: none.
// - Intentionally excluded: DATADataTypeState is Mutagen serialization state, not an xEdit field.
// - Rationale: semantic fields are forwarded while the winning record retains its binary DATA layout.
public class MaterialObjectRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Model", new ModelHandler() },
        { "DNAMs", new DNAMsHandler() },
        { "FalloffScale", new SimpleReflectionPropertyHandler<float, IMaterialObject, IMaterialObjectGetter>("FalloffScale") },
        { "FalloffBias", new SimpleReflectionPropertyHandler<float, IMaterialObject, IMaterialObjectGetter>("FalloffBias") },
        { "NoiseUvScale", new SimpleReflectionPropertyHandler<float, IMaterialObject, IMaterialObjectGetter>("NoiseUvScale") },
        { "MaterialUvScale", new SimpleReflectionPropertyHandler<float, IMaterialObject, IMaterialObjectGetter>("MaterialUvScale") },
        { "ProjectionVector", new SimpleReflectionPropertyHandler<P3Float, IMaterialObject, IMaterialObjectGetter>("ProjectionVector") },
        { "NormalDampener", new SimpleReflectionPropertyHandler<float, IMaterialObject, IMaterialObjectGetter>("NormalDampener") },
        { "SinglePassColor", new SimpleReflectionPropertyHandler<System.Drawing.Color, IMaterialObject, IMaterialObjectGetter>("SinglePassColor") },
        { "Flags", new SimpleReflectionFlagPropertyHandler<MaterialObject.Flag, IMaterialObject, IMaterialObjectGetter>("Flags") },
        { "HasSnow", new SimpleReflectionPropertyHandler<bool, IMaterialObject, IMaterialObjectGetter>("HasSnow") },
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IMaterialObjectGetter materialObjectRecord)
        {
            throw new InvalidOperationException($"Expected IMaterialObjectGetter but got {winningContext.Record.GetType()}");
        }

        return materialObjectRecord
            .ToLink<IMaterialObjectGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IMaterialObject, IMaterialObjectGetter>(state.LinkCache)
            .ToArray();
    }
}
