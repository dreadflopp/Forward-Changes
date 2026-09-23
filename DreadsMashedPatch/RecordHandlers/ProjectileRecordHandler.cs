using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.PropertyHandlers.Projectile;
using DreadsMashedPatch.RecordHandlers.Abstracts;

namespace DreadsMashedPatch.RecordHandlers;

// Migration note:
// - Generalized: independent PROJ scalar/form-link fields and unrelated flag bits.
// - Kept specialized: trajectory, explosion, muzzle-flash, pickup, disable, and collision groups.
// - Intentionally excluded: DATADataTypeState is Mutagen serialization state, not an xEdit field.
// - Rationale: fields that jointly define one behavior are decided atomically, while unrelated DATA
//   fields and flag bits remain independently mergeable and the winning binary layout is retained.
public class ProjectileRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Name", new NameHandler() },
        { "ModelAndBounds", new ModelBoundsHandler() },
        { "Destructible", new GeneratedCopyReflectionPropertyHandler<IDestructibleGetter, Destructible, IProjectile, IProjectileGetter>("Destructible", value => value.DeepCopy(), DestructibleMixIn.Equals) },
        { "Flags", new SimpleReflectionFlagPropertyHandler<Projectile.Flag, IProjectile, IProjectileGetter>(
            "Flags",
            preserveUnknownBits: true,
            includedFlags:
            [
                Projectile.Flag.Hitscan,
                Projectile.Flag.Supersonic,
                Projectile.Flag.PinsLimbs,
                Projectile.Flag.PassThroughSmallTransparent,
                Projectile.Flag.DisableCombatAimCorrection,
                Projectile.Flag.Rotation
            ]) },
        { "Trajectory", new ProjectileTrajectoryHandler() },
        { "Light", new SimpleReflectionFormLinkPropertyHandler<ILightGetter, IProjectile, IProjectileGetter>("Light") },
        { "MuzzleFlashBehavior", new ProjectileMuzzleFlashHandler() },
        { "TracerChance", new SimpleReflectionPropertyHandler<float, IProjectile, IProjectileGetter>("TracerChance") },
        { "ExplosionBehavior", new ProjectileExplosionHandler() },
        { "Sound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IProjectile, IProjectileGetter>("Sound") },
        { "FadeDuration", new SimpleReflectionPropertyHandler<float, IProjectile, IProjectileGetter>("FadeDuration") },
        { "ImpactForce", new SimpleReflectionPropertyHandler<float, IProjectile, IProjectileGetter>("ImpactForce") },
        { "PickupBehavior", new ProjectilePickupHandler() },
        { "DisableBehavior", new ProjectileDisableHandler() },
        { "Collision", new ProjectileCollisionHandler() },
        { "DecalData", new SimpleReflectionFormLinkPropertyHandler<ITextureSetGetter, IProjectile, IProjectileGetter>("DecalData") },
        { "SoundLevel", new SimpleReflectionPropertyHandler<uint, IProjectile, IProjectileGetter>("SoundLevel") },
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
