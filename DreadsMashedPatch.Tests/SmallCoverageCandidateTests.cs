using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers;
using ForwardChanges.RecordHandlers.Abstracts;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class SmallCoverageCandidateTests
{
    [Fact]
    public void AddonNodeUsesSerializedFlagsInsteadOfNonexistentAlwaysLoadedProperty()
    {
        var handlers = new AddonNodeRecordHandler().PropertyHandlers;
        Assert.Contains("Flags", handlers.Keys);
        Assert.DoesNotContain("AlwaysLoaded", handlers.Keys);
        var handler = Assert.IsType<SimpleReflectionFlagPropertyHandler<AddonNode.Flag, IAddonNode, IAddonNodeGetter>>(
            handlers["Flags"]);
        var record = new AddonNode(new FormKey(TestModKey, 0x400), SkyrimRelease.SkyrimSE)
        {
            Flags = (AddonNode.Flag)0x4
        };

        handler.SetValue(record, AddonNode.Flag.AlwaysLoaded);

        Assert.Equal(0x4 | (int)AddonNode.Flag.AlwaysLoaded, (int)record.Flags);
    }

    [Fact]
    public void PackageUsesCorrectInterruptFlagsPathAndPreservesUnknownBits()
    {
        var handlers = new PackageRecordHandler().PropertyHandlers;
        Assert.Contains("InterruptFlags", handlers.Keys);
        Assert.DoesNotContain("InteruptFlags", handlers.Keys);
        var handler = Assert.IsType<SimpleReflectionFlagPropertyHandler<Package.InterruptFlag, IPackage, IPackageGetter>>(
            handlers["InterruptFlags"]);
        var record = new Package(new FormKey(TestModKey, 0x401), SkyrimRelease.SkyrimSE)
        {
            InterruptFlags = (Package.InterruptFlag)0x100
        };

        handler.SetValue(record, Package.InterruptFlag.WorldInteractions);

        Assert.Equal(0x100 | (int)Package.InterruptFlag.WorldInteractions, (int)record.InterruptFlags);
    }

    [Fact]
    public void SerializationOnlyUnusedPropertiesAreNotRegistered()
    {
        var excludedProperties = new (AbstractRecordHandler Handler, string[] Properties)[]
        {
            (new BookRecordHandler(), ["Unused"]),
            (new NpcRecordHandler(), ["AIData.Unused", "PlayerSkills.Unused", "PlayerSkills.Unused2"]),
            (new SceneRecordHandler(), ["Unused", "Unused2"]),
            (new StaticRecordHandler(), ["Unused"]),
            (new WaterRecordHandler(), ["UnusedNoisemaps"]),
            (new WeaponRecordHandler(),
            [
                "Unused", "Data.Unused", "Data.Unused2", "Critical.Unused",
                "Critical.Unused2", "Critical.Unused3", "Critical.Unused4"
            ])
        };

        foreach (var (handler, properties) in excludedProperties)
        {
            foreach (var property in properties)
            {
                Assert.DoesNotContain(property, handler.PropertyHandlers.Keys);
            }
        }
    }

    private static readonly ModKey TestModKey = new("SmallCoverageTests", ModType.Plugin);
}
