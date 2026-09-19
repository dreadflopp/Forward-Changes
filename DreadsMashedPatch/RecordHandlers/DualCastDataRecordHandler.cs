using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.RecordHandlers.Abstracts;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: all properties via shared handlers.
    // - Kept specialized: none.
    // - Rationale: interface is straightforward links/enums with stable shared coverage.
    public class DualCastDataRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "Projectile", new SimpleReflectionFormLinkPropertyHandler<IProjectileGetter, IDualCastData, IDualCastDataGetter>("Projectile") },
            { "Explosion", new SimpleReflectionFormLinkPropertyHandler<IExplosionGetter, IDualCastData, IDualCastDataGetter>("Explosion") },
            { "EffectShader", new SimpleReflectionFormLinkPropertyHandler<IEffectShaderGetter, IDualCastData, IDualCastDataGetter>("EffectShader") },
            { "HitEffectArt", new SimpleReflectionFormLinkPropertyHandler<IArtObjectGetter, IDualCastData, IDualCastDataGetter>("HitEffectArt") },
            { "ImpactDataSet", new SimpleReflectionFormLinkPropertyHandler<IImpactDataSetGetter, IDualCastData, IDualCastDataGetter>("ImpactDataSet") },
            { "InheritScale", new SimpleReflectionPropertyHandler<Mutagen.Bethesda.Skyrim.DualCastData.InheritScaleType, IDualCastData, IDualCastDataGetter>("InheritScale") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IDualCastDataGetter dualCastData)
            {
                throw new InvalidOperationException($"Expected IDualCastDataGetter but got {winningContext.Record.GetType()}");
            }

            return dualCastData
                .ToLink<IDualCastDataGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IDualCastData, IDualCastDataGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
