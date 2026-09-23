using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Noggog;

namespace DreadsMashedPatch.PropertyHandlers.Projectile;

public readonly record struct ProjectileTrajectoryGroup(
    Mutagen.Bethesda.Skyrim.Projectile.TypeEnum Type,
    float Gravity,
    float Speed,
    float Range,
    float ConeSpread,
    float Lifetime,
    float RelaunchInterval);

public readonly record struct ProjectileExplosionGroup(
    bool ExplosionEnabled,
    bool AlternateTriggerEnabled,
    FormKey Explosion,
    float Proximity,
    float Timer,
    FormKey CountdownSound);

public sealed record ProjectileMuzzleFlashGroup(
    bool Enabled,
    FormKey Light,
    float Duration,
    string ModelPath,
    byte[]? TextureHashes);

public readonly record struct ProjectilePickupGroup(bool Enabled, FormKey WeaponSource);

public readonly record struct ProjectileDisableGroup(bool Enabled, FormKey Sound);

public readonly record struct ProjectileCollisionGroup(float Radius, FormKey Layer);

public sealed class ProjectileTrajectoryHandler : AbstractPropertyHandler<ProjectileTrajectoryGroup>
{
    public override string PropertyName => "Trajectory";

    public override ProjectileTrajectoryGroup GetValue(IMajorRecordGetter record)
    {
        var projectile = RequireGetter(record);
        return new(
            projectile.Type,
            projectile.Gravity,
            projectile.Speed,
            projectile.Range,
            projectile.ConeSpread,
            projectile.Lifetime,
            projectile.RelaunchInterval);
    }

    public override void SetValue(IMajorRecord record, ProjectileTrajectoryGroup value)
    {
        var projectile = RequireSetter(record);
        projectile.Type = value.Type;
        projectile.Gravity = value.Gravity;
        projectile.Speed = value.Speed;
        projectile.Range = value.Range;
        projectile.ConeSpread = value.ConeSpread;
        projectile.Lifetime = value.Lifetime;
        projectile.RelaunchInterval = value.RelaunchInterval;
    }

    public override string FormatValue(object? value) => value is ProjectileTrajectoryGroup group
        ? $"Type={group.Type}, Gravity={group.Gravity:G9}, Speed={group.Speed:G9}, Range={group.Range:G9}, " +
          $"Cone={group.ConeSpread:G9}, Lifetime={group.Lifetime:G9}, Relaunch={group.RelaunchInterval:G9}"
        : "null";

    private static IProjectileGetter RequireGetter(IMajorRecordGetter record) =>
        record as IProjectileGetter
        ?? throw new InvalidOperationException($"Expected IProjectileGetter but got {record.GetType()}");

    private static IProjectile RequireSetter(IMajorRecord record) =>
        record as IProjectile
        ?? throw new InvalidOperationException($"Expected IProjectile but got {record.GetType()}");
}

public sealed class ProjectileExplosionHandler : AbstractPropertyHandler<ProjectileExplosionGroup>
{
    private const Mutagen.Bethesda.Skyrim.Projectile.Flag OwnedFlags =
        Mutagen.Bethesda.Skyrim.Projectile.Flag.Explosion |
        Mutagen.Bethesda.Skyrim.Projectile.Flag.AltTrigger;

    public override string PropertyName => "ExplosionBehavior";

    public override ProjectileExplosionGroup GetValue(IMajorRecordGetter record)
    {
        var projectile = RequireGetter(record);
        return new(
            ProjectileAtomicFlagUtility.HasFlag(projectile.Flags, Mutagen.Bethesda.Skyrim.Projectile.Flag.Explosion),
            ProjectileAtomicFlagUtility.HasFlag(projectile.Flags, Mutagen.Bethesda.Skyrim.Projectile.Flag.AltTrigger),
            projectile.Explosion.FormKey,
            projectile.ExplosionAltTriggerProximity,
            projectile.ExplosionAltTriggerTimer,
            projectile.CountdownSound.FormKey);
    }

    public override void SetValue(IMajorRecord record, ProjectileExplosionGroup value)
    {
        var projectile = RequireSetter(record);
        var requested = (value.ExplosionEnabled ? Mutagen.Bethesda.Skyrim.Projectile.Flag.Explosion : (Mutagen.Bethesda.Skyrim.Projectile.Flag)0)
            | (value.AlternateTriggerEnabled ? Mutagen.Bethesda.Skyrim.Projectile.Flag.AltTrigger : (Mutagen.Bethesda.Skyrim.Projectile.Flag)0);
        projectile.Flags = ProjectileAtomicFlagUtility.ReplaceFlags(projectile.Flags, OwnedFlags, requested);
        projectile.Explosion.SetTo(value.Explosion);
        projectile.ExplosionAltTriggerProximity = value.Proximity;
        projectile.ExplosionAltTriggerTimer = value.Timer;
        projectile.CountdownSound.SetTo(value.CountdownSound);
    }

    private static IProjectileGetter RequireGetter(IMajorRecordGetter record) =>
        record as IProjectileGetter
        ?? throw new InvalidOperationException($"Expected IProjectileGetter but got {record.GetType()}");

    private static IProjectile RequireSetter(IMajorRecord record) =>
        record as IProjectile
        ?? throw new InvalidOperationException($"Expected IProjectile but got {record.GetType()}");
}

public sealed class ProjectileMuzzleFlashHandler : AbstractPropertyHandler<ProjectileMuzzleFlashGroup>
{
    private const Mutagen.Bethesda.Skyrim.Projectile.Flag OwnedFlag =
        Mutagen.Bethesda.Skyrim.Projectile.Flag.MuzzleFlash;

    public override string PropertyName => "MuzzleFlashBehavior";

    public override ProjectileMuzzleFlashGroup GetValue(IMajorRecordGetter record)
    {
        var projectile = RequireGetter(record);
        return new(
            ProjectileAtomicFlagUtility.HasFlag(projectile.Flags, OwnedFlag),
            projectile.MuzzleFlash.FormKey,
            projectile.MuzzleFlashDuration,
            projectile.MuzzleFlashModel.GivenPath,
            projectile.TextureFilesHashes?.ToArray());
    }

    public override void SetValue(IMajorRecord record, ProjectileMuzzleFlashGroup? value)
    {
        if (value == null) return;
        var projectile = RequireSetter(record);
        projectile.Flags = ProjectileAtomicFlagUtility.ReplaceFlags(
            projectile.Flags,
            OwnedFlag,
            value.Enabled ? OwnedFlag : (Mutagen.Bethesda.Skyrim.Projectile.Flag)0);
        projectile.MuzzleFlash.SetTo(value.Light);
        projectile.MuzzleFlashDuration = value.Duration;
        projectile.MuzzleFlashModel = new AssetLink<SkyrimModelAssetType>(value.ModelPath);
        projectile.TextureFilesHashes = value.TextureHashes == null
            ? (MemorySlice<byte>?)null
            : new MemorySlice<byte>(value.TextureHashes.ToArray());
    }

    public override bool AreValuesEqual(
        ProjectileMuzzleFlashGroup? value1,
        ProjectileMuzzleFlashGroup? value2)
    {
        if (value1 == null || value2 == null) return value1 == null && value2 == null;
        return value1.Enabled == value2.Enabled
            && value1.Light == value2.Light
            && value1.Duration.Equals(value2.Duration)
            && string.Equals(
                AssetPathHelper.NormalizeForComparison(value1.ModelPath),
                AssetPathHelper.NormalizeForComparison(value2.ModelPath),
                StringComparison.OrdinalIgnoreCase)
            && BinaryEqual(value1.TextureHashes, value2.TextureHashes);
    }

    private static bool BinaryEqual(byte[]? left, byte[]? right) =>
        left == null || right == null
            ? left == null && right == null
            : left.AsSpan().SequenceEqual(right);

    private static IProjectileGetter RequireGetter(IMajorRecordGetter record) =>
        record as IProjectileGetter
        ?? throw new InvalidOperationException($"Expected IProjectileGetter but got {record.GetType()}");

    private static IProjectile RequireSetter(IMajorRecord record) =>
        record as IProjectile
        ?? throw new InvalidOperationException($"Expected IProjectile but got {record.GetType()}");
}

public sealed class ProjectilePickupHandler : AbstractPropertyHandler<ProjectilePickupGroup>
{
    private const Mutagen.Bethesda.Skyrim.Projectile.Flag OwnedFlag =
        Mutagen.Bethesda.Skyrim.Projectile.Flag.CanBePickedUp;

    public override string PropertyName => "PickupBehavior";

    public override ProjectilePickupGroup GetValue(IMajorRecordGetter record)
    {
        var projectile = (IProjectileGetter)record;
        return new(ProjectileAtomicFlagUtility.HasFlag(projectile.Flags, OwnedFlag), projectile.DefaultWeaponSource.FormKey);
    }

    public override void SetValue(IMajorRecord record, ProjectilePickupGroup value)
    {
        var projectile = (IProjectile)record;
        projectile.Flags = ProjectileAtomicFlagUtility.ReplaceFlags(
            projectile.Flags,
            OwnedFlag,
            value.Enabled ? OwnedFlag : (Mutagen.Bethesda.Skyrim.Projectile.Flag)0);
        projectile.DefaultWeaponSource.SetTo(value.WeaponSource);
    }
}

public sealed class ProjectileDisableHandler : AbstractPropertyHandler<ProjectileDisableGroup>
{
    private const Mutagen.Bethesda.Skyrim.Projectile.Flag OwnedFlag =
        Mutagen.Bethesda.Skyrim.Projectile.Flag.CanBeDisabled;

    public override string PropertyName => "DisableBehavior";

    public override ProjectileDisableGroup GetValue(IMajorRecordGetter record)
    {
        var projectile = (IProjectileGetter)record;
        return new(ProjectileAtomicFlagUtility.HasFlag(projectile.Flags, OwnedFlag), projectile.DisaleSound.FormKey);
    }

    public override void SetValue(IMajorRecord record, ProjectileDisableGroup value)
    {
        var projectile = (IProjectile)record;
        projectile.Flags = ProjectileAtomicFlagUtility.ReplaceFlags(
            projectile.Flags,
            OwnedFlag,
            value.Enabled ? OwnedFlag : (Mutagen.Bethesda.Skyrim.Projectile.Flag)0);
        projectile.DisaleSound.SetTo(value.Sound);
    }
}

public sealed class ProjectileCollisionHandler : AbstractPropertyHandler<ProjectileCollisionGroup>
{
    public override string PropertyName => "Collision";

    public override ProjectileCollisionGroup GetValue(IMajorRecordGetter record)
    {
        var projectile = (IProjectileGetter)record;
        return new(projectile.CollisionRadius, projectile.CollisionLayer.FormKey);
    }

    public override void SetValue(IMajorRecord record, ProjectileCollisionGroup value)
    {
        var projectile = (IProjectile)record;
        projectile.CollisionRadius = value.Radius;
        projectile.CollisionLayer.SetTo(value.Layer);
    }
}

internal static class ProjectileAtomicFlagUtility
{
    public static bool HasFlag(
        Mutagen.Bethesda.Skyrim.Projectile.Flag value,
        Mutagen.Bethesda.Skyrim.Projectile.Flag flag) => (value & flag) == flag;

    public static Mutagen.Bethesda.Skyrim.Projectile.Flag ReplaceFlags(
        Mutagen.Bethesda.Skyrim.Projectile.Flag current,
        Mutagen.Bethesda.Skyrim.Projectile.Flag owned,
        Mutagen.Bethesda.Skyrim.Projectile.Flag requested) =>
        (current & ~owned) | (requested & owned);
}
