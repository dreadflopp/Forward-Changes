using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Projectile;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Xunit;
using SkyrimProjectile = Mutagen.Bethesda.Skyrim.Projectile;

namespace DreadsMashedPatch.Tests;

public sealed class ProjectileAtomicGroupHandlerTests
{
    private static readonly ModKey ModKey = new("ProjectileTests.esp", ModType.Plugin);

    [Fact]
    public void RecordHandlerUsesOneOwnerForEveryGroupedField()
    {
        var handlers = new ProjectileRecordHandler().PropertyHandlers;

        Assert.IsType<ProjectileTrajectoryHandler>(handlers["Trajectory"]);
        Assert.IsType<ProjectileExplosionHandler>(handlers["ExplosionBehavior"]);
        Assert.IsType<ProjectileMuzzleFlashHandler>(handlers["MuzzleFlashBehavior"]);
        Assert.IsType<ProjectilePickupHandler>(handlers["PickupBehavior"]);
        Assert.IsType<ProjectileDisableHandler>(handlers["DisableBehavior"]);
        Assert.IsType<ProjectileCollisionHandler>(handlers["Collision"]);
        Assert.IsType<SimpleReflectionFlagPropertyHandler<SkyrimProjectile.Flag, IProjectile, IProjectileGetter>>(
            handlers["Flags"]);

        var replacedProperties = new[]
        {
            "Type", "Gravity", "Speed", "Range", "ConeSpread", "Lifetime", "RelaunchInterval",
            "Explosion", "ExplosionAltTriggerProximity", "ExplosionAltTriggerTimer", "CountdownSound",
            "MuzzleFlash", "MuzzleFlashDuration", "MuzzleFlashModel", "TextureFilesHashes",
            "DefaultWeaponSource", "DisaleSound", "CollisionRadius", "CollisionLayer"
        };

        foreach (var property in replacedProperties)
        {
            Assert.DoesNotContain(property, handlers.Keys);
        }
    }

    [Fact]
    public void ResidualFlagHandlerPreservesBitsOwnedByAtomicGroups()
    {
        var projectile = CreateProjectile();
        projectile.Flags = SkyrimProjectile.Flag.Explosion
            | SkyrimProjectile.Flag.MuzzleFlash
            | SkyrimProjectile.Flag.CanBePickedUp;
        var handler = Assert.IsType<SimpleReflectionFlagPropertyHandler<SkyrimProjectile.Flag, IProjectile, IProjectileGetter>>(
            new ProjectileRecordHandler().PropertyHandlers["Flags"]);

        Assert.True(handler.AreValuesEqual(
            SkyrimProjectile.Flag.Explosion,
            SkyrimProjectile.Flag.MuzzleFlash | SkyrimProjectile.Flag.CanBeDisabled));
        Assert.False(handler.AreValuesEqual(0, SkyrimProjectile.Flag.Hitscan));

        handler.SetValue(projectile, SkyrimProjectile.Flag.Hitscan);

        Assert.Equal(
            SkyrimProjectile.Flag.Explosion
            | SkyrimProjectile.Flag.MuzzleFlash
            | SkyrimProjectile.Flag.CanBePickedUp
            | SkyrimProjectile.Flag.Hitscan,
            projectile.Flags);
    }

    [Fact]
    public void MuzzleFlashGroupIsAppliedAtomicallyAndPreservesOtherFlags()
    {
        var projectile = CreateProjectile();
        projectile.Flags = SkyrimProjectile.Flag.Hitscan | SkyrimProjectile.Flag.Explosion;
        var light = new FormKey(ModKey, 0x200);
        var handler = new ProjectileMuzzleFlashHandler();

        handler.SetValue(
            projectile,
            new ProjectileMuzzleFlashGroup(true, light, 0.75f, "Meshes/Effects/Flash.nif", [1, 2, 3]));

        Assert.True(projectile.Flags.HasFlag(SkyrimProjectile.Flag.MuzzleFlash));
        Assert.True(projectile.Flags.HasFlag(SkyrimProjectile.Flag.Hitscan));
        Assert.True(projectile.Flags.HasFlag(SkyrimProjectile.Flag.Explosion));
        Assert.Equal(light, projectile.MuzzleFlash.FormKey);
        Assert.Equal(0.75f, projectile.MuzzleFlashDuration);
        Assert.Equal("Meshes/Effects/Flash.nif", projectile.MuzzleFlashModel.GivenPath);
        Assert.Equal(new byte[] { 1, 2, 3 }, projectile.TextureFilesHashes!.Value.ToArray());
    }

    [Fact]
    public void ExplosionGroupUpdatesOnlyItsOwnedFlagBits()
    {
        var projectile = CreateProjectile();
        projectile.Flags = SkyrimProjectile.Flag.Hitscan
            | SkyrimProjectile.Flag.MuzzleFlash
            | SkyrimProjectile.Flag.AltTrigger;
        var explosion = new FormKey(ModKey, 0x300);
        var countdownSound = new FormKey(ModKey, 0x301);
        var handler = new ProjectileExplosionHandler();

        handler.SetValue(
            projectile,
            new ProjectileExplosionGroup(true, false, explosion, 128, 1.5f, countdownSound));

        Assert.True(projectile.Flags.HasFlag(SkyrimProjectile.Flag.Explosion));
        Assert.False(projectile.Flags.HasFlag(SkyrimProjectile.Flag.AltTrigger));
        Assert.True(projectile.Flags.HasFlag(SkyrimProjectile.Flag.Hitscan));
        Assert.True(projectile.Flags.HasFlag(SkyrimProjectile.Flag.MuzzleFlash));
        Assert.Equal(explosion, projectile.Explosion.FormKey);
        Assert.Equal(128, projectile.ExplosionAltTriggerProximity);
        Assert.Equal(1.5f, projectile.ExplosionAltTriggerTimer);
        Assert.Equal(countdownSound, projectile.CountdownSound.FormKey);
    }

    [Fact]
    public void TrajectoryComparisonTreatsAnyMemberChangeAsOneAtomicChange()
    {
        var handler = new ProjectileTrajectoryHandler();
        var baseline = new ProjectileTrajectoryGroup(
            SkyrimProjectile.TypeEnum.Missile, 1, 2, 3, 4, 5, 6);
        var changed = baseline with { Speed = 20 };

        Assert.True(handler.AreValuesEqual(baseline, baseline));
        Assert.False(handler.AreValuesEqual(baseline, changed));
    }

    private static SkyrimProjectile CreateProjectile() =>
        new(new FormKey(ModKey, 0x100), SkyrimRelease.SkyrimSE);
}
