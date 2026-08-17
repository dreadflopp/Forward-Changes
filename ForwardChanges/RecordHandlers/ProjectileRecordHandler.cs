using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers;

// Migration note:
// - Generalized: PROJ scalar/form-link/asset-link fields via reflection handlers.
// - Kept specialized: none.
// - Rationale: interface surface is direct and covered by existing generic/property handlers.
public class ProjectileRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "ObjectBounds", new ObjectBoundsHandler() },
        { "Name", new NameHandler() },
        { "Model", new ModelHandler() },
        { "Destructible", new ComplexReflectionPropertyHandler<IDestructibleGetter, IProjectile, IProjectileGetter>("Destructible") },
        { "Flags", new SimpleReflectionFlagPropertyHandler<Projectile.Flag, IProjectile, IProjectileGetter>("Flags") },
        { "Type", new SimpleReflectionPropertyHandler<Projectile.TypeEnum, IProjectile, IProjectileGetter>("Type") },
        { "Gravity", new SimpleReflectionPropertyHandler<float, IProjectile, IProjectileGetter>("Gravity") },
        { "Speed", new SimpleReflectionPropertyHandler<float, IProjectile, IProjectileGetter>("Speed") },
        { "Range", new SimpleReflectionPropertyHandler<float, IProjectile, IProjectileGetter>("Range") },
        { "Light", new SimpleReflectionFormLinkPropertyHandler<ILightGetter, IProjectile, IProjectileGetter>("Light") },
        { "MuzzleFlash", new SimpleReflectionFormLinkPropertyHandler<ILightGetter, IProjectile, IProjectileGetter>("MuzzleFlash") },
        { "TracerChance", new SimpleReflectionPropertyHandler<float, IProjectile, IProjectileGetter>("TracerChance") },
        { "ExplosionAltTriggerProximity", new SimpleReflectionPropertyHandler<float, IProjectile, IProjectileGetter>("ExplosionAltTriggerProximity") },
        { "ExplosionAltTriggerTimer", new SimpleReflectionPropertyHandler<float, IProjectile, IProjectileGetter>("ExplosionAltTriggerTimer") },
        { "Explosion", new SimpleReflectionFormLinkPropertyHandler<IExplosionGetter, IProjectile, IProjectileGetter>("Explosion") },
        { "Sound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IProjectile, IProjectileGetter>("Sound") },
        { "MuzzleFlashDuration", new SimpleReflectionPropertyHandler<float, IProjectile, IProjectileGetter>("MuzzleFlashDuration") },
        { "FadeDuration", new SimpleReflectionPropertyHandler<float, IProjectile, IProjectileGetter>("FadeDuration") },
        { "ImpactForce", new SimpleReflectionPropertyHandler<float, IProjectile, IProjectileGetter>("ImpactForce") },
        { "CountdownSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IProjectile, IProjectileGetter>("CountdownSound") },
        { "DisaleSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IProjectile, IProjectileGetter>("DisaleSound") },
        { "DefaultWeaponSource", new SimpleReflectionFormLinkPropertyHandler<IWeaponGetter, IProjectile, IProjectileGetter>("DefaultWeaponSource") },
        { "ConeSpread", new SimpleReflectionPropertyHandler<float, IProjectile, IProjectileGetter>("ConeSpread") },
        { "CollisionRadius", new SimpleReflectionPropertyHandler<float, IProjectile, IProjectileGetter>("CollisionRadius") },
        { "Lifetime", new SimpleReflectionPropertyHandler<float, IProjectile, IProjectileGetter>("Lifetime") },
        { "RelaunchInterval", new SimpleReflectionPropertyHandler<float, IProjectile, IProjectileGetter>("RelaunchInterval") },
        { "DecalData", new SimpleReflectionFormLinkPropertyHandler<ITextureSetGetter, IProjectile, IProjectileGetter>("DecalData") },
        { "CollisionLayer", new SimpleReflectionFormLinkPropertyHandler<ICollisionLayerGetter, IProjectile, IProjectileGetter>("CollisionLayer") },
        { "MuzzleFlashModel", new SimpleReflectionPropertyHandler<AssetLinkGetter<SkyrimModelAssetType>, IProjectile, IProjectileGetter>("MuzzleFlashModel") },
        { "TextureFilesHashes", new SimpleReflectionBinaryDataPropertyHandler<IProjectile, IProjectileGetter>("TextureFilesHashes") },
        { "SoundLevel", new SimpleReflectionPropertyHandler<uint, IProjectile, IProjectileGetter>("SoundLevel") },
        { "DATADataTypeState", new SimpleReflectionPropertyHandler<Projectile.DATADataType, IProjectile, IProjectileGetter>("DATADataTypeState") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IProjectileGetter projectileRecord)
        {
            throw new InvalidOperationException($"Expected IProjectileGetter but got {winningContext.Record.GetType()}");
        }

        return projectileRecord
            .ToLink<IProjectileGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IProjectile, IProjectileGetter>(state.LinkCache)
            .ToArray();
    }
}