using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Quest;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class VirtualMachineAdapterHandlerTests
{
    private static readonly ModKey TestModKey = new("VmadHandlerTests", ModType.Plugin);
    private readonly TestVirtualMachineAdapterHandler _handler = new();

    [Fact]
    public void ChangedScriptContentKeepsScriptIdentityButIsDetectedAsAChange()
    {
        var original = CreateMineOreScript(1);
        var changed = Clone(original);
        ((ScriptIntProperty)changed.Properties.Single(property => property.Name == "OreCount")).Data = 2;

        Assert.True(_handler.SameIdentity(original, changed));
        Assert.False(_handler.AreValuesEqual([original], [changed]));
    }

    [Fact]
    public void NewAtomicValueReplacesTheCompleteSameNameScriptAndTakesOwnership()
    {
        var original = CreateMineOreScript(1);
        var ussepVersion = Clone(original);
        ((ScriptBoolProperty)ussepVersion.Properties.Single(property => property.Name == "GiveOre")).Data = false;
        var afterUssep = _handler.ApplyAtomic(
            original,
            ussepVersion,
            original,
            forwardOwner: "Skyrim.esm",
            newOwner: "Unofficial Skyrim Special Edition Patch.esp",
            owner => string.Equals(owner, "Skyrim.esm", StringComparison.OrdinalIgnoreCase));

        // Deepborn was authored without USSEP as a master. Its complete script
        // value is nevertheless a new value because it differs from both the
        // original and the currently forwarded USSEP value.
        var deepbornVersion = Clone(original);
        ((ScriptIntProperty)deepbornVersion.Properties.Single(property => property.Name == "OreCount")).Data = 2;
        var afterDeepborn = _handler.ApplyAtomic(
            afterUssep.Value,
            deepbornVersion,
            original,
            forwardOwner: afterUssep.Owner,
            newOwner: "deepborn.esp",
            owner => string.Equals(owner, "Skyrim.esm", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(owner, "deepborn.esp", StringComparison.OrdinalIgnoreCase));

        Assert.Equal(
            2,
            ((IScriptIntPropertyGetter)afterDeepborn.Value.Properties.Single(property => property.Name == "OreCount")).Data);
        Assert.True(
            ((IScriptBoolPropertyGetter)afterDeepborn.Value.Properties.Single(property => property.Name == "GiveOre")).Data);
        Assert.Equal("deepborn.esp", afterDeepborn.Owner);
    }

    [Fact]
    public void ReversionWithoutPermissionKeepsTheForwardedAtomicValue()
    {
        var original = CreateMineOreScript(1);
        var ussepVersion = Clone(original);
        ((ScriptIntProperty)ussepVersion.Properties.Single(property => property.Name == "OreCount")).Data = 2;

        var result = _handler.ApplyAtomic(
            ussepVersion,
            original,
            original,
            forwardOwner: "Unofficial Skyrim Special Edition Patch.esp",
            newOwner: "deepborn.esp",
            _ => false);

        Assert.Equal("Unofficial Skyrim Special Edition Patch.esp", result.Owner);
        Assert.Equal(
            2,
            ((IScriptIntPropertyGetter)result.Value.Properties.Single(property => property.Name == "OreCount")).Data);
    }

    [Fact]
    public void ReversionWithPermissionRestoresOriginalAndTakesOwnership()
    {
        var original = CreateMineOreScript(1);
        var ussepVersion = Clone(original);
        ((ScriptIntProperty)ussepVersion.Properties.Single(property => property.Name == "OreCount")).Data = 2;

        var result = _handler.ApplyAtomic(
            ussepVersion,
            original,
            original,
            forwardOwner: "Unofficial Skyrim Special Edition Patch.esp",
            newOwner: "dependent.esp",
            owner => string.Equals(
                owner,
                "Unofficial Skyrim Special Edition Patch.esp",
                StringComparison.OrdinalIgnoreCase));

        Assert.Equal("dependent.esp", result.Owner);
        Assert.Equal(
            1,
            ((IScriptIntPropertyGetter)result.Value.Properties.Single(property => property.Name == "OreCount")).Data);
    }

    [Fact]
    public void DeepComparisonChecksScriptFlagsPropertyTypeFlagsValuesAndNestedValues()
    {
        var original = CreateScriptWithEveryPropertyKind();
        AssertUnchanged(original);

        AssertChanged(original, script => script.Flags = ScriptEntry.Flag.Inherited);
        AssertChanged(original, script => script.Properties[0].Flags = ScriptProperty.Flag.Removed);
        AssertChanged(original, script => script.Properties[0] = new ScriptFloatProperty
        {
            Name = script.Properties[0].Name,
            Flags = script.Properties[0].Flags,
            Data = 1f
        });
        AssertChanged(original, script => ((ScriptBoolProperty)script.Properties[0]).Data = false);
        AssertChanged(original, script => ((ScriptIntProperty)script.Properties[1]).Data++);
        AssertChanged(original, script => ((ScriptFloatProperty)script.Properties[2]).Data += 0.5f);
        AssertChanged(original, script => ((ScriptStringProperty)script.Properties[3]).Data += " changed");
        AssertChanged(original, script => ((ScriptObjectProperty)script.Properties[4]).Object.SetTo(
            new FormKey(TestModKey, 0x701)));
        AssertChanged(original, script => ((ScriptObjectProperty)script.Properties[4]).Alias++);
        AssertUnchanged(original, script => ((ScriptObjectProperty)script.Properties[4]).Unused++);
        AssertChanged(original, script => ((ScriptBoolListProperty)script.Properties[5]).Data[0] = false);
        AssertChanged(original, script => ((ScriptIntListProperty)script.Properties[6]).Data[0]++);
        AssertChanged(original, script => ((ScriptFloatListProperty)script.Properties[7]).Data[0] += 0.5f);
        AssertChanged(original, script => ((ScriptStringListProperty)script.Properties[8]).Data[0] += " changed");
        AssertChanged(original, script => ((ScriptObjectListProperty)script.Properties[9]).Objects[0].Object.SetTo(
            new FormKey(TestModKey, 0x702)));
        AssertChanged(original, script => ((ScriptObjectListProperty)script.Properties[9]).Objects[0].Alias++);
        AssertUnchanged(original, script => ((ScriptObjectListProperty)script.Properties[9]).Objects[0].Unused++);
        AssertChanged(original, script => ((ScriptObjectListProperty)script.Properties[9]).Objects[0].Name += " changed");
        AssertChanged(original, script => ((ScriptObjectListProperty)script.Properties[9]).Objects[0].Flags =
            ScriptProperty.Flag.Removed);
    }

    [Fact]
    public void AtomicSemanticChangePreservesForwardedUnusedValues()
    {
        var original = CreateScriptWithEveryPropertyKind();
        var changed = Clone(original);
        ((ScriptObjectProperty)changed.Properties[4]).Alias++;
        ((ScriptObjectProperty)changed.Properties[4]).Unused = 40;
        ((ScriptObjectListProperty)changed.Properties[9]).Objects[0].Unused = 60;

        var result = _handler.ApplyAtomic(
            original,
            changed,
            original,
            forwardOwner: "Skyrim.esm",
            newOwner: "Change.esp",
            _ => true);

        Assert.Equal(4, ((IScriptObjectPropertyGetter)result.Value.Properties[4]).Unused);
        Assert.Equal(
            6,
            ((IScriptObjectListPropertyGetter)result.Value.Properties[9]).Objects[0].Unused);
        Assert.Equal(4, ((IScriptObjectPropertyGetter)result.Value.Properties[4]).Alias);
    }

    [Fact]
    public void SetValuePreservesWinningUnusedValuesAndDefaultsNewObjectProperties()
    {
        var winning = CreateScriptWithEveryPropertyKind();
        var forwarded = Clone(winning);
        ((ScriptObjectProperty)forwarded.Properties[4]).Alias++;
        ((ScriptObjectProperty)forwarded.Properties[4]).Unused = 40;
        ((ScriptObjectListProperty)forwarded.Properties[9]).Objects[0].Unused = 60;
        forwarded.Properties.Add(new ScriptObjectProperty
        {
            Name = "NewObject",
            Object = new FormLink<ISkyrimMajorRecordGetter>(new FormKey(TestModKey, 0x703)),
            Unused = 70
        });

        var target = new Mutagen.Bethesda.Skyrim.Activator(
            new FormKey(TestModKey, 0x704),
            SkyrimRelease.SkyrimSE)
        {
            VirtualMachineAdapter = new VirtualMachineAdapter()
        };
        target.VirtualMachineAdapter.Scripts.Add(winning);

        _handler.SetValue(target, [forwarded]);

        var result = Assert.Single(target.VirtualMachineAdapter!.Scripts);
        Assert.Equal(4, ((IScriptObjectPropertyGetter)result.Properties[4]).Unused);
        Assert.Equal(6, ((IScriptObjectListPropertyGetter)result.Properties[9]).Objects[0].Unused);
        Assert.Equal(0, ((IScriptObjectPropertyGetter)result.Properties.Single(p => p.Name == "NewObject")).Unused);
    }

    [Fact]
    public void QuestFragmentAliasesIgnoreAndPreserveUnusedValues()
    {
        var destinationAlias = CreateFragmentAlias(aliasId: 12, propertyUnused: 4, scriptUnused: 6);
        var sourceAlias = CreateFragmentAlias(aliasId: 12, propertyUnused: 40, scriptUnused: 60);
        var handler = new QuestFragmentAliasHandler();

        Assert.True(handler.AreValuesEqual([destinationAlias], [sourceAlias]));

        var quest = new Quest(new FormKey(TestModKey, 0x705), SkyrimRelease.SkyrimSE)
        {
            VirtualMachineAdapter = new QuestAdapter()
        };
        quest.VirtualMachineAdapter.Aliases.Add(destinationAlias);

        handler.SetValue(quest, [sourceAlias]);

        var result = Assert.Single(quest.VirtualMachineAdapter.Aliases);
        Assert.Equal(4, result.Property.Unused);
        Assert.Equal(
            6,
            ((IScriptObjectPropertyGetter)Assert.Single(Assert.Single(result.Scripts).Properties)).Unused);
    }

    private void AssertUnchanged(ScriptEntry original)
    {
        Assert.True(_handler.AreValuesEqual([original], [Clone(original)]));
    }

    private void AssertUnchanged(ScriptEntry original, Action<ScriptEntry> mutate)
    {
        var changed = Clone(original);
        mutate(changed);
        Assert.True(_handler.AreValuesEqual([original], [changed]));
    }

    private void AssertChanged(ScriptEntry original, Action<ScriptEntry> mutate)
    {
        var changed = Clone(original);
        mutate(changed);
        Assert.False(_handler.AreValuesEqual([original], [changed]));
    }

    private static ScriptEntry CreateMineOreScript(int oreCount)
    {
        var script = new ScriptEntry
        {
            Name = "MineOreScript",
            Flags = ScriptEntry.Flag.Local
        };
        script.Properties.Add(new ScriptIntProperty
        {
            Name = "OreCount",
            Flags = ScriptProperty.Flag.Edited,
            Data = oreCount
        });
        script.Properties.Add(new ScriptBoolProperty
        {
            Name = "GiveOre",
            Flags = ScriptProperty.Flag.Edited,
            Data = true
        });
        return script;
    }

    private static ScriptEntry CreateScriptWithEveryPropertyKind()
    {
        var objectKey = new FormKey(TestModKey, 0x700);
        var script = new ScriptEntry { Name = "AllValues", Flags = ScriptEntry.Flag.Local };
        script.Properties.Add(new ScriptBoolProperty { Name = "Bool", Flags = ScriptProperty.Flag.Edited, Data = true });
        script.Properties.Add(new ScriptIntProperty { Name = "Int", Flags = ScriptProperty.Flag.Edited, Data = 10 });
        script.Properties.Add(new ScriptFloatProperty { Name = "Float", Flags = ScriptProperty.Flag.Edited, Data = 1.25f });
        script.Properties.Add(new ScriptStringProperty { Name = "String", Flags = ScriptProperty.Flag.Edited, Data = "value" });
        script.Properties.Add(new ScriptObjectProperty
        {
            Name = "Object",
            Flags = ScriptProperty.Flag.Edited,
            Object = new FormLink<ISkyrimMajorRecordGetter>(objectKey),
            Alias = 3,
            Unused = 4
        });

        var boolList = new ScriptBoolListProperty { Name = "BoolList", Flags = ScriptProperty.Flag.Edited };
        boolList.Data.Add(true);
        script.Properties.Add(boolList);
        var intList = new ScriptIntListProperty { Name = "IntList", Flags = ScriptProperty.Flag.Edited };
        intList.Data.Add(20);
        script.Properties.Add(intList);
        var floatList = new ScriptFloatListProperty { Name = "FloatList", Flags = ScriptProperty.Flag.Edited };
        floatList.Data.Add(2.5f);
        script.Properties.Add(floatList);
        var stringList = new ScriptStringListProperty { Name = "StringList", Flags = ScriptProperty.Flag.Edited };
        stringList.Data.Add("entry");
        script.Properties.Add(stringList);
        var objectList = new ScriptObjectListProperty { Name = "ObjectList", Flags = ScriptProperty.Flag.Edited };
        objectList.Objects.Add(new ScriptObjectProperty
        {
            Name = "NestedObject",
            Flags = ScriptProperty.Flag.Edited,
            Object = new FormLink<ISkyrimMajorRecordGetter>(objectKey),
            Alias = 5,
            Unused = 6
        });
        script.Properties.Add(objectList);
        return script;
    }

    private static QuestFragmentAlias CreateFragmentAlias(
        short aliasId,
        ushort propertyUnused,
        ushort scriptUnused)
    {
        var script = new ScriptEntry { Name = "FragmentAliasScript" };
        script.Properties.Add(new ScriptObjectProperty
        {
            Name = "Object",
            Object = new FormLink<ISkyrimMajorRecordGetter>(new FormKey(TestModKey, 0x706)),
            Alias = 2,
            Unused = scriptUnused
        });

        var alias = new QuestFragmentAlias
        {
            Version = 5,
            ObjectFormat = 2,
            Property = new ScriptObjectProperty
            {
                Name = "AliasProperty",
                Alias = aliasId,
                Unused = propertyUnused
            }
        };
        alias.Scripts.Add(script);
        return alias;
    }

    private static ScriptEntry Clone(IScriptEntryGetter source)
    {
        var clone = new ScriptEntry();
        clone.DeepCopyIn(source);
        return clone;
    }

    private sealed class TestVirtualMachineAdapterHandler
        : SimpleReflectionVirtualMachineAdapterHandler<IActivator, IActivatorGetter>
    {
        public bool SameIdentity(IScriptEntryGetter script1, IScriptEntryGetter script2)
        {
            return IsItemEqual(script1, script2);
        }

        public (IScriptEntryGetter Value, string Owner) ApplyAtomic(
            IScriptEntryGetter forwardScript,
            IScriptEntryGetter recordScript,
            IScriptEntryGetter? originalScript,
            string forwardOwner,
            string newOwner,
            Func<string?, bool> hasPermission)
        {
            var forwardContext = new ForwardChanges.Contexts.ListPropertyValueContext<IScriptEntryGetter>(
                forwardScript,
                forwardOwner);
            ApplyAtomicScriptChange(
                forwardContext,
                recordScript,
                originalScript,
                newOwner,
                hasPermission);
            return (forwardContext.Value, forwardContext.OwnerMod);
        }
    }
}
