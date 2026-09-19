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
    // - Generalized: all properties via shared reflection and common handlers.
    // - Kept specialized: none.
    // - Intentionally excluded: DATADataTypeState is Mutagen serialization state, not an xEdit field.
    // - Rationale: semantic fields are forwarded while the winning record retains its binary DATA layout.
    public class ExplosionRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "VirtualMachineAdapter", new SimpleReflectionVirtualMachineAdapterHandler<IExplosion, IExplosionGetter>() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "Name", new NameHandler() },
            { "Model", new ModelHandler() },
            { "ObjectEffect", new SimpleReflectionFormLinkPropertyHandler<IEffectRecordGetter, IExplosion, IExplosionGetter>("ObjectEffect") },
            { "ImageSpaceModifier", new SimpleReflectionFormLinkPropertyHandler<IImageSpaceAdapterGetter, IExplosion, IExplosionGetter>("ImageSpaceModifier") },
            { "Light", new SimpleReflectionFormLinkPropertyHandler<ILightGetter, IExplosion, IExplosionGetter>("Light") },
            { "Sound1", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IExplosion, IExplosionGetter>("Sound1") },
            { "Sound2", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IExplosion, IExplosionGetter>("Sound2") },
            { "ImpactDataSet", new SimpleReflectionFormLinkPropertyHandler<IImpactDataSetGetter, IExplosion, IExplosionGetter>("ImpactDataSet") },
            { "PlacedObject", new SimpleReflectionFormLinkPropertyHandler<IExplodeSpawnGetter, IExplosion, IExplosionGetter>("PlacedObject") },
            { "SpawnProjectile", new SimpleReflectionFormLinkPropertyHandler<IProjectileGetter, IExplosion, IExplosionGetter>("SpawnProjectile") },
            { "Force", new SimpleReflectionPropertyHandler<float, IExplosion, IExplosionGetter>("Force", 0.001f) },
            { "Damage", new SimpleReflectionPropertyHandler<float, IExplosion, IExplosionGetter>("Damage", 0.001f) },
            { "Radius", new SimpleReflectionPropertyHandler<float, IExplosion, IExplosionGetter>("Radius", 0.001f) },
            { "ISRadius", new SimpleReflectionPropertyHandler<float, IExplosion, IExplosionGetter>("ISRadius", 0.001f) },
            { "VerticalOffsetMult", new SimpleReflectionPropertyHandler<float, IExplosion, IExplosionGetter>("VerticalOffsetMult", 0.001f) },
            { "Flags", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Explosion.Flag, IExplosion, IExplosionGetter>("Flags") },
            { "SoundLevel", new SimpleReflectionPropertyHandler<SoundLevel, IExplosion, IExplosionGetter>("SoundLevel") },
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IExplosionGetter explosion)
            {
                throw new InvalidOperationException($"Expected IExplosionGetter but got {winningContext.Record.GetType()}");
            }

            return explosion
                .ToLink<IExplosionGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IExplosion, IExplosionGetter>(state.LinkCache)
                .ToArray();
        }
    }
}
