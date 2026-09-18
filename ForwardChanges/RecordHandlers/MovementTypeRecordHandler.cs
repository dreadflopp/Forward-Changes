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
// - Generalized: all MOVT scalar and enum fields via reflection handlers.
// - Kept specialized: none.
// - Intentionally excluded: SPEDDataTypeState is Mutagen serialization state, not an xEdit field.
// - Rationale: semantic fields are forwarded while the winning record retains its binary SPED layout.
public class MovementTypeRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Name", new SimpleReflectionPropertyHandler<string?, IMovementType, IMovementTypeGetter>("Name") },
        { "LeftWalk", new SimpleReflectionPropertyHandler<float, IMovementType, IMovementTypeGetter>("LeftWalk") },
        { "LeftRun", new SimpleReflectionPropertyHandler<float, IMovementType, IMovementTypeGetter>("LeftRun") },
        { "RightWalk", new SimpleReflectionPropertyHandler<float, IMovementType, IMovementTypeGetter>("RightWalk") },
        { "RightRun", new SimpleReflectionPropertyHandler<float, IMovementType, IMovementTypeGetter>("RightRun") },
        { "ForwardWalk", new SimpleReflectionPropertyHandler<float, IMovementType, IMovementTypeGetter>("ForwardWalk") },
        { "ForwardRun", new SimpleReflectionPropertyHandler<float, IMovementType, IMovementTypeGetter>("ForwardRun") },
        { "BackWalk", new SimpleReflectionPropertyHandler<float, IMovementType, IMovementTypeGetter>("BackWalk") },
        { "BackRun", new SimpleReflectionPropertyHandler<float, IMovementType, IMovementTypeGetter>("BackRun") },
        { "RotateInPlaceWalk", new SimpleReflectionPropertyHandler<float, IMovementType, IMovementTypeGetter>("RotateInPlaceWalk") },
        { "RotateInPlaceRun", new SimpleReflectionPropertyHandler<float, IMovementType, IMovementTypeGetter>("RotateInPlaceRun") },
        { "RotateWhileMovingRun", new SimpleReflectionPropertyHandler<float, IMovementType, IMovementTypeGetter>("RotateWhileMovingRun") },
        { "AnimationChangeThresholds", new ComplexReflectionPropertyHandler<IAnimationChangeThresholdsGetter, IMovementType, IMovementTypeGetter>("AnimationChangeThresholds") },
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IMovementTypeGetter movementTypeRecord)
        {
            throw new InvalidOperationException($"Expected IMovementTypeGetter but got {winningContext.Record.GetType()}");
        }

        return movementTypeRecord
            .ToLink<IMovementTypeGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IMovementType, IMovementTypeGetter>(state.LinkCache)
            .ToArray();
    }
}
