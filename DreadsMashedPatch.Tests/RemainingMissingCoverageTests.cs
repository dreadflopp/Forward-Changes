using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class RemainingMissingCoverageTests
{
    [Fact]
    public void LowRiskAndFormListFieldsUseEstablishedHandlers()
    {
        Assert.IsType<ModelBoundsHandler>(new IngestibleRecordHandler().PropertyHandlers["ModelAndBounds"]);
        Assert.IsType<EditorIDHandler>(new WeaponRecordHandler().PropertyHandlers["EditorID"]);

        var formListHandlers = new FormIdRecordHandler().PropertyHandlers;
        Assert.IsType<MajorRecordFlagsRawHandler>(formListHandlers["MajorRecordFlagsRaw"]);
        Assert.IsType<SkyrimMajorRecordFlagsHandler>(formListHandlers["SkyrimMajorRecordFlags"]);
    }

    [Theory]
    [InlineData("RagdollData")]
    [InlineData("RagdollBipedData")]
    public void PlacedNpcRagdollHandlersDeepCopyBytes(string propertyName)
    {
        var handler = Assert.IsType<SimpleReflectionBinaryDataPropertyHandler<IPlacedNpc, IPlacedNpcGetter>>(
            new PlacedNpcRecordHandler().PropertyHandlers[propertyName]);
        var sourceBytes = new byte[] { 1, 2, 3, 4 };
        var source = CreatePlacedNpc(0x500);
        var target = CreatePlacedNpc(0x501);
        if (propertyName == "RagdollData")
        {
            source.RagdollData = new MemorySlice<byte>(sourceBytes);
        }
        else
        {
            source.RagdollBipedData = new MemorySlice<byte>(sourceBytes);
        }

        handler.SetValue(target, handler.GetValue(source));
        sourceBytes[0] = 9;

        var copied = propertyName == "RagdollData" ? target.RagdollData : target.RagdollBipedData;
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, copied!.Value.ToArray());
    }

    [Fact]
    public void DialogResponseDataIsDeepCopiedAndOpaquePayloadIsExcluded()
    {
        var handlers = new DialogResponseRecordHandler().PropertyHandlers;
        var dataHandler = Assert.IsType<SimpleReflectionBinaryDataPropertyHandler<IDialogResponses, IDialogResponsesGetter>>(
            handlers["DATA"]);
        var sourceBytes = new byte[] { 5, 6, 7 };
        var source = CreateDialogResponses(0x502);
        var target = CreateDialogResponses(0x503);
        source.DATA = new MemorySlice<byte>(sourceBytes);

        dataHandler.SetValue(target, dataHandler.GetValue(source));
        sourceBytes[0] = 0;

        Assert.Equal(new byte[] { 5, 6, 7 }, target.DATA!.Value.ToArray());
        Assert.DoesNotContain("PreviousDialog", handlers.Keys);
        Assert.DoesNotContain("UnknownData", handlers.Keys);
    }

    [Fact]
    public void NpcAggregateHandlersCoverLeavesAndCreateOptionalPlayerSkills()
    {
        var handlers = new NpcRecordHandler().PropertyHandlers;
        var expectedKeys = new[]
        {
            "AIData.Aggression", "AIData.Attack", "Configuration.Level",
            "Configuration.TemplateFlags", "PlayerSkills.Health", "PlayerSkills.SkillValues"
        };
        foreach (var key in expectedKeys)
        {
            Assert.Contains(key, handlers.Keys);
        }

        var npc = new Npc(new FormKey(TestModKey, 0x504), SkyrimRelease.SkyrimSE);
        Assert.Null(npc.PlayerSkills);
        handlers["AIData.Aggression"].SetValue(npc, Aggression.VeryAggressive);
        handlers["Configuration.StaminaOffset"].SetValue(npc, (short)23);
        handlers["Configuration.Level"].SetValue(npc, new NpcLevel { Level = 31 });
        handlers["PlayerSkills.Health"].SetValue(npc, (ushort)125);

        Assert.Equal(Aggression.VeryAggressive, npc.AIData.Aggression);
        Assert.Equal(23, npc.Configuration.StaminaOffset);
        Assert.Equal(31, Assert.IsType<NpcLevel>(npc.Configuration.Level).Level);
        Assert.Equal(125, npc.PlayerSkills!.Health);
        Assert.DoesNotContain("AIData.Unused", handlers.Keys);
        Assert.DoesNotContain("PlayerSkills.Unused", handlers.Keys);
        Assert.DoesNotContain("PlayerSkills.Unused2", handlers.Keys);
    }

    [Fact]
    public void NpcTemplateFlagHandlerPreservesUnknownBits()
    {
        const int unknownBit = 0x4000;
        var npc = new Npc(new FormKey(TestModKey, 0x505), SkyrimRelease.SkyrimSE);
        npc.Configuration.TemplateFlags = (NpcConfiguration.TemplateFlag)unknownBit;
        var handler = Assert.IsType<SimpleReflectionFlagPropertyHandler<NpcConfiguration.TemplateFlag, INpc, INpcGetter>>(
            new NpcRecordHandler().PropertyHandlers["Configuration.TemplateFlags"]);

        handler.SetValue(npc, NpcConfiguration.TemplateFlag.Traits);

        Assert.Equal(unknownBit | (int)NpcConfiguration.TemplateFlag.Traits, (int)npc.Configuration.TemplateFlags);
    }

    [Fact]
    public void QuestAdapterUsesExactSemanticLeavesAndCreatesMissingAdapter()
    {
        var handlers = new QuestRecordHandler().PropertyHandlers;
        var expectedKeys = new[]
        {
            "VirtualMachineAdapter.Presence",
            "VirtualMachineAdapter.Version", "VirtualMachineAdapter.ObjectFormat",
            "VirtualMachineAdapter.Scripts", "VirtualMachineAdapter.ExtraBindDataVersion",
            "VirtualMachineAdapter.FileName", "VirtualMachineAdapter.Fragments",
            "VirtualMachineAdapter.Aliases"
        };
        foreach (var key in expectedKeys)
        {
            Assert.Contains(key, handlers.Keys);
        }

        Assert.DoesNotContain("VirtualMachineAdapter.Versioning", handlers.Keys);
        Assert.DoesNotContain("QuestScripts", handlers.Keys);
        Assert.DoesNotContain("QuestScriptFragments", handlers.Keys);
        Assert.DoesNotContain("QuestFragmentAliases", handlers.Keys);

        var quest = new Quest(new FormKey(TestModKey, 0x506), SkyrimRelease.SkyrimSE);
        Assert.Null(quest.VirtualMachineAdapter);
        handlers["VirtualMachineAdapter.Version"].SetValue(quest, (short)5);
        handlers["VirtualMachineAdapter.ObjectFormat"].SetValue(quest, (ushort)2);
        handlers["VirtualMachineAdapter.FileName"].SetValue(quest, "Fragments.pex");

        Assert.NotNull(quest.VirtualMachineAdapter);
        Assert.Equal(5, quest.VirtualMachineAdapter.Version);
        Assert.Equal(2, quest.VirtualMachineAdapter.ObjectFormat);
        Assert.Equal("Fragments.pex", quest.VirtualMachineAdapter.FileName);

        var questWithScripts = new Quest(new FormKey(TestModKey, 0x507), SkyrimRelease.SkyrimSE);
        handlers["VirtualMachineAdapter.Scripts"].SetValue(
            questWithScripts,
            new List<IScriptEntryGetter> { new ScriptEntry { Name = "CoverageScript" } });
        Assert.Equal("CoverageScript", Assert.Single(questWithScripts.VirtualMachineAdapter!.Scripts).Name);
    }

    [Fact]
    public void WeaponAggregateHandlersUseExactLeavesAndCreateMissingAggregates()
    {
        var handlers = new WeaponRecordHandler().PropertyHandlers;
        var expectedKeys = new[]
        {
            "BasicStats.Value", "BasicStats.Weight", "BasicStats.Damage",
            "Data.AnimationType", "Data.Flags",
            "Critical.Damage", "Critical.Flags", "Critical.Effect"
        };
        foreach (var key in expectedKeys)
        {
            Assert.Contains(key, handlers.Keys);
        }

        Assert.DoesNotContain("Value", handlers.Keys);
        Assert.DoesNotContain("Damage", handlers.Keys);
        Assert.DoesNotContain("Flags", handlers.Keys);
        Assert.DoesNotContain("Versioning", handlers.Keys);
        Assert.DoesNotContain("Critical.Versioning", handlers.Keys);

        var weapon = new Weapon(new FormKey(TestModKey, 0x508), SkyrimRelease.SkyrimSE);
        weapon.BasicStats = null;
        weapon.Data = null;
        weapon.Critical = null;
        handlers["BasicStats.Damage"].SetValue(weapon, (ushort)42);
        handlers["Data.Speed"].SetValue(weapon, 1.25f);
        handlers["Critical.Damage"].SetValue(weapon, (ushort)17);
        var effectKey = new FormKey(TestModKey, 0x509);
        handlers["Critical.Effect"].SetValue(weapon, new FormLink<ISpellGetter>(effectKey));

        Assert.Equal(42, weapon.BasicStats!.Damage);
        Assert.Equal(1.25f, weapon.Data!.Speed);
        Assert.Equal(17, weapon.Critical!.Damage);
        Assert.Equal(effectKey, weapon.Critical.Effect.FormKey);

        var unusedKeys = new[]
        {
            "Unused", "Data.Unused", "Data.Unused2", "Critical.Unused",
            "Critical.Unused2", "Critical.Unused3", "Critical.Unused4"
        };
        foreach (var key in unusedKeys)
        {
            Assert.DoesNotContain(key, handlers.Keys);
        }
    }

    [Fact]
    public void WeaponAggregateFlagHandlersPreserveUnknownBits()
    {
        const int dataUnknownBit = 0x40000000;
        const int criticalUnknownBit = 0x4000;
        var weapon = new Weapon(new FormKey(TestModKey, 0x510), SkyrimRelease.SkyrimSE)
        {
            Data = new WeaponData { Flags = (WeaponData.Flag)dataUnknownBit },
            Critical = new CriticalData { Flags = (CriticalData.Flag)criticalUnknownBit }
        };
        var handlers = new WeaponRecordHandler().PropertyHandlers;

        handlers["Data.Flags"].SetValue(weapon, WeaponData.Flag.Automatic);
        handlers["Critical.Flags"].SetValue(weapon, CriticalData.Flag.OnDeath);

        Assert.Equal(dataUnknownBit | (int)WeaponData.Flag.Automatic, (int)weapon.Data.Flags);
        Assert.Equal(criticalUnknownBit | (int)CriticalData.Flag.OnDeath, (int)weapon.Critical.Flags);
    }

    private static readonly ModKey TestModKey = new("RemainingCoverageTests", ModType.Plugin);

    private static PlacedNpc CreatePlacedNpc(uint id) =>
        new(new FormKey(TestModKey, id), SkyrimRelease.SkyrimSE);

    private static DialogResponses CreateDialogResponses(uint id) =>
        new(new FormKey(TestModKey, id), SkyrimRelease.SkyrimSE);
}
