using System;
using Noggog;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.RecordHandlers.Abstracts;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: semantic properties via shared handlers.
    // - Kept specialized: none.
    // - Intentionally excluded: Unknown* fields are outside the semantic conflict surface.
    // - Rationale: surface is scalar/formlink/binary data suitable for reflection handlers.
    public class GrassRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "Model", new ModelHandler() },
            { "Density", new SimpleReflectionPropertyHandler<byte, IGrass, IGrassGetter>("Density") },
            { "MinSlope", new SimpleReflectionPropertyHandler<byte, IGrass, IGrassGetter>("MinSlope") },
            { "MaxSlope", new SimpleReflectionPropertyHandler<byte, IGrass, IGrassGetter>("MaxSlope") },
            { "UnitsFromWater", new SimpleReflectionPropertyHandler<ushort, IGrass, IGrassGetter>("UnitsFromWater") },
            { "UnitsFromWaterType", new SimpleReflectionPropertyHandler<Mutagen.Bethesda.Skyrim.Grass.UnitsFromWaterTypeEnum, IGrass, IGrassGetter>("UnitsFromWaterType") },
            { "PositionRange", new SimpleReflectionPropertyHandler<float, IGrass, IGrassGetter>("PositionRange", 0.001f) },
            { "HeightRange", new SimpleReflectionPropertyHandler<float, IGrass, IGrassGetter>("HeightRange", 0.001f) },
            { "ColorRange", new SimpleReflectionPropertyHandler<float, IGrass, IGrassGetter>("ColorRange", 0.001f) },
            { "WavePeriod", new SimpleReflectionPropertyHandler<float, IGrass, IGrassGetter>("WavePeriod", 0.001f) },
            { "Flags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Grass.Flag, IGrass, IGrassGetter>("Flags") },
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IGrassGetter grass)
            {
                throw new InvalidOperationException($"Expected IGrassGetter but got {winningContext.Record.GetType()}");
            }

            return grass
                .ToLink<IGrassGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IGrass, IGrassGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
