using DreadsMashedPatch.PropertyHandlers.DialogResponse;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class DialogResponseVirtualMachineAdapterHandlerTests
{
    private static readonly ModKey TestModKey = new("DialogVmadTests", ModType.Plugin);
    private readonly VirtualMachineAdapterHandler _handler = new();

    [Fact]
    public void SetValueCopiesCompleteAdapterWithoutDefaultingTypedDataOrFragments()
    {
        var source = CreateAdapter(objectUnused: 40);
        var target = new DialogResponses(
            new FormKey(TestModKey, 0x801),
            SkyrimRelease.SkyrimSE)
        {
            VirtualMachineAdapter = CreateAdapter(objectUnused: 4)
        };

        _handler.SetValue(target, source);

        var result = Assert.IsType<DialogResponsesAdapter>(target.VirtualMachineAdapter);
        Assert.Equal(5, result.Version);
        Assert.Equal((ushort)2, result.ObjectFormat);
        Assert.Equal("TIF__0002B8B2", result.ScriptFragments?.FileName);
        Assert.Equal("Fragment_0", result.ScriptFragments?.OnEnd?.FragmentName);
        Assert.Equal("TIF__0002B8B2", result.ScriptFragments?.OnEnd?.ScriptName);

        var script = Assert.Single(result.Scripts);
        Assert.Equal(ScriptEntry.Flag.Local, script.Flags);
        Assert.Equal(37, Assert.IsType<ScriptIntProperty>(script.Properties[0]).Data);
        var objectProperty = Assert.IsType<ScriptObjectProperty>(script.Properties[1]);
        Assert.Equal(new FormKey(TestModKey, 0x800), objectProperty.Object.FormKey);
        Assert.Equal((short)3, objectProperty.Alias);
        Assert.Equal((ushort)4, objectProperty.Unused);
    }

    [Fact]
    public void EqualityUsesCompleteSemanticVmadAndIgnoresOpaqueObjectPadding()
    {
        var baseline = CreateAdapter(objectUnused: 4);
        var sameSemantics = CreateAdapter(objectUnused: 40);

        Assert.True(_handler.AreValuesEqual(baseline, sameSemantics));

        Assert.IsType<ScriptIntProperty>(sameSemantics.Scripts[0].Properties[0]).Data++;
        Assert.False(_handler.AreValuesEqual(baseline, sameSemantics));

        sameSemantics = CreateAdapter(objectUnused: 40);
        sameSemantics.ScriptFragments!.OnEnd!.FragmentName = "Fragment_1";
        Assert.False(_handler.AreValuesEqual(baseline, sameSemantics));

        sameSemantics = CreateAdapter(objectUnused: 40);
        sameSemantics.ObjectFormat = 1;
        Assert.False(_handler.AreValuesEqual(baseline, sameSemantics));
    }

    private static DialogResponsesAdapter CreateAdapter(ushort objectUnused)
    {
        var adapter = new DialogResponsesAdapter
        {
            Version = 5,
            ObjectFormat = 2,
            ScriptFragments = new ScriptFragments
            {
                ExtraBindDataVersion = 2,
                FileName = "TIF__0002B8B2",
                OnEnd = new ScriptFragment
                {
                    ExtraBindDataVersion = 1,
                    FragmentName = "Fragment_0",
                    ScriptName = "TIF__0002B8B2"
                }
            }
        };

        var script = new ScriptEntry
        {
            Flags = ScriptEntry.Flag.Local,
            Name = "TIF__0002B8B2"
        };
        script.Properties.Add(new ScriptIntProperty
        {
            Name = "StageToSet",
            Flags = ScriptProperty.Flag.Edited,
            Data = 37
        });
        script.Properties.Add(new ScriptObjectProperty
        {
            Name = "QuestToSet",
            Flags = ScriptProperty.Flag.Edited,
            Object = new FormLink<ISkyrimMajorRecordGetter>(new FormKey(TestModKey, 0x800)),
            Alias = 3,
            Unused = objectUnused
        });
        adapter.Scripts.Add(script);
        return adapter;
    }
}
