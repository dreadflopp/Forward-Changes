using ForwardChanges.PropertyHandlers.Activator;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class DestructibleHandlerTests
{
    private static readonly ModKey TestModKey = new("DestructibleHandlerTests", ModType.Plugin);
    private readonly DestructibleHandler _handler = new();

    [Fact]
    public void EqualityIncludesDestructionStages()
    {
        var original = CreateDestructible();
        var changed = original.DeepCopy();
        changed.Stages[0].Data!.HealthPercent++;

        Assert.True(_handler.AreValuesEqual(original, original.DeepCopy()));
        Assert.False(_handler.AreValuesEqual(original, changed));
    }

    [Fact]
    public void SetValueDeepCopiesTheCompleteDestructible()
    {
        var source = CreateDestructible();
        var target = new Mutagen.Bethesda.Skyrim.Activator(
            new FormKey(TestModKey, 0x800),
            SkyrimRelease.SkyrimSE);

        _handler.SetValue(target, source);

        Assert.NotNull(target.Destructible);
        Assert.True(_handler.AreValuesEqual(source, target.Destructible));
        Assert.NotSame(source, target.Destructible);
        Assert.NotSame(source.Stages[0], target.Destructible!.Stages[0]);

        source.Stages[0].Data!.HealthPercent = 1;
        Assert.Equal(75, target.Destructible.Stages[0].Data!.HealthPercent);
    }

    [Fact]
    public void FormatValueShowsDataAndStageContent()
    {
        var formatted = _handler.FormatValue(CreateDestructible());

        Assert.Contains("Health=100", formatted);
        Assert.Contains("Stages[1]", formatted);
        Assert.Contains("HealthPercent=75", formatted);
        Assert.Contains("DebrisCount=3", formatted);
        Assert.DoesNotContain("DestructibleBinaryOverlay", formatted);
    }

    private static Destructible CreateDestructible()
    {
        var destructible = new Destructible
        {
            Data = new DestructableData
            {
                Health = 100,
                DESTCount = 1,
                VATSTargetable = true,
                Unknown = 7
            }
        };
        destructible.Stages.Add(new DestructionStage
        {
            Data = new DestructionStageData
            {
                HealthPercent = 75,
                Index = 1,
                ModelDamageStage = 2,
                Flags = DestructionStageData.Flag.CapDamage,
                SelfDamagePerSecond = 4,
                Explosion = new FormLink<IExplosionGetter>(new FormKey(TestModKey, 0x801)),
                Debris = new FormLink<IDebrisGetter>(new FormKey(TestModKey, 0x802)),
                DebrisCount = 3
            }
        });
        return destructible;
    }
}
