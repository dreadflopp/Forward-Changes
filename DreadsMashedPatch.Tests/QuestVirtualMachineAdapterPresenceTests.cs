using ForwardChanges.PropertyHandlers.Quest;
using ForwardChanges.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class QuestVirtualMachineAdapterPresenceTests
{
    private static readonly ModKey PatchModKey = ModKey.FromNameAndExtension("QuestVmadPatch.esp");

    [Fact]
    public void PresenceHandlerDistinguishesMissingAndPresentAdapters()
    {
        var handler = new QuestVirtualMachineAdapterPresenceHandler();
        var quest = CreateQuest(0x900);

        Assert.False(handler.GetValue(quest));

        handler.SetValue(quest, true);
        Assert.True(handler.GetValue(quest));
        Assert.NotNull(quest.VirtualMachineAdapter);

        handler.SetValue(quest, false);
        Assert.False(handler.GetValue(quest));
        Assert.Null(quest.VirtualMachineAdapter);
    }

    [Fact]
    public void AbsentPresenceRemovesWholeAdapterAndSuppressesChildValues()
    {
        var quest = CreateQuestWithObjectScript(0x901);
        var recordHandler = new QuestRecordHandler();
        var attemptedScript = CreateObjectScript(quest.FormKey, "ShouldNotSurvive");
        var properties = new Dictionary<string, object?>
        {
            ["VirtualMachineAdapter.Scripts"] = new List<IScriptEntryGetter> { attemptedScript },
            ["VirtualMachineAdapter.ObjectFormat"] = (ushort)0,
            ["VirtualMachineAdapter.Version"] = (short)0,
            ["VirtualMachineAdapter.Presence"] = false
        };

        recordHandler.ApplyForwardedProperties(quest, properties);

        Assert.Null(quest.VirtualMachineAdapter);
        AssertSerializes(quest);
    }

    [Fact]
    public void PresentPresenceIsAppliedBeforeChildrenRegardlessOfInputOrder()
    {
        var quest = CreateQuest(0x902);
        var recordHandler = new QuestRecordHandler();
        var script = CreateObjectScript(quest.FormKey, "RestoredScript");
        var properties = new Dictionary<string, object?>
        {
            ["VirtualMachineAdapter.Scripts"] = new List<IScriptEntryGetter> { script },
            ["VirtualMachineAdapter.ObjectFormat"] = (ushort)2,
            ["VirtualMachineAdapter.Version"] = (short)5,
            ["VirtualMachineAdapter.ExtraBindDataVersion"] = (byte)2,
            ["VirtualMachineAdapter.FileName"] = "QF_RestoredQuest",
            ["VirtualMachineAdapter.Presence"] = true
        };

        recordHandler.ApplyForwardedProperties(quest, properties);

        var adapter = Assert.IsType<QuestAdapter>(quest.VirtualMachineAdapter);
        Assert.Equal(5, adapter.Version);
        Assert.Equal(2, adapter.ObjectFormat);
        Assert.Equal(2, adapter.ExtraBindDataVersion);
        Assert.Equal("QF_RestoredQuest", adapter.FileName);
        Assert.Equal("RestoredScript", Assert.Single(adapter.Scripts).Name);
        AssertSerializes(quest);
    }

    [Fact]
    public void MissingWinningAdapterCannotBeRecreatedByAnOrphanedChildDecision()
    {
        var quest = CreateQuest(0x903);
        var recordHandler = new QuestRecordHandler();

        recordHandler.ApplyForwardedProperties(
            quest,
            new Dictionary<string, object?>
            {
                ["VirtualMachineAdapter.Scripts"] = new List<IScriptEntryGetter>
                {
                    CreateObjectScript(quest.FormKey, "OrphanedScript")
                }
            });

        Assert.Null(quest.VirtualMachineAdapter);
        AssertSerializes(quest);
    }

    [Fact]
    public void PresentAdapterWithObjectPropertiesRejectsUnsupportedObjectFormatBeforeMutation()
    {
        var quest = CreateQuest(0x904);
        var recordHandler = new QuestRecordHandler();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            recordHandler.ApplyForwardedProperties(
                quest,
                new Dictionary<string, object?>
                {
                    ["VirtualMachineAdapter.Presence"] = true,
                    ["VirtualMachineAdapter.ObjectFormat"] = (ushort)0,
                    ["VirtualMachineAdapter.Scripts"] = new List<IScriptEntryGetter>
                    {
                        CreateObjectScript(quest.FormKey, "InvalidScript")
                    }
                }));

        Assert.Contains("unsupported ObjectFormat 0", exception.Message);
        Assert.Null(quest.VirtualMachineAdapter);
        AssertSerializes(quest);
    }

    private static Quest CreateQuest(uint id) =>
        new(new FormKey(PatchModKey, id), SkyrimRelease.SkyrimSE);

    private static Quest CreateQuestWithObjectScript(uint id)
    {
        var quest = CreateQuest(id);
        quest.VirtualMachineAdapter = new QuestAdapter
        {
            Version = 5,
            ObjectFormat = 2,
            ExtraBindDataVersion = 2,
            FileName = "QF_ExistingQuest"
        };
        quest.VirtualMachineAdapter.Scripts.Add(CreateObjectScript(quest.FormKey, "ExistingScript"));
        return quest;
    }

    private static ScriptEntry CreateObjectScript(FormKey target, string name)
    {
        var script = new ScriptEntry { Name = name, Flags = ScriptEntry.Flag.Local };
        script.Properties.Add(new ScriptObjectProperty
        {
            Name = "Target",
            Flags = ScriptProperty.Flag.Edited,
            Object = new FormLink<ISkyrimMajorRecordGetter>(target)
        });
        return script;
    }

    private static void AssertSerializes(Quest quest)
    {
        var patch = new SkyrimMod(PatchModKey, SkyrimRelease.SkyrimSE);
        patch.Quests.Add(quest);
        using var stream = new MemoryStream();
        patch.WriteToBinary(stream);
        Assert.True(stream.Length > 0);
    }
}
