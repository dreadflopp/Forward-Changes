using DreadsMashedPatch.PropertyHandlers.ConstructibleObject;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class ConditionHandlerTests
{
    private static readonly ModKey SourceModKey = ModKey.FromNameAndExtension("ConditionSource.esp");

    [Fact]
    public void GetStageReferencesIgnorePluginNameCasing()
    {
        var upperCaseReference = new FormKey(
            new ModKey("ccASVSSE001-ALMSIVI", ModType.Master),
            0x000CC3);
        var lowerCaseReference = new FormKey(
            new ModKey("ccasvsse001-almsivi", ModType.Master),
            0x000CC3);
        var handler = new ConditionsHandler();

        var upperCaseCondition = CreateGetStageCondition(upperCaseReference);
        var lowerCaseCondition = CreateGetStageCondition(lowerCaseReference);

        Assert.True(upperCaseReference.Equals(lowerCaseReference));
        Assert.True(handler.AreValuesEqual([upperCaseCondition], [lowerCaseCondition]));
    }

    [Fact]
    public void GetStageReferencesStillDistinguishDifferentFormIds()
    {
        var modKey = new ModKey("ccasvsse001-almsivi", ModType.Master);
        var handler = new ConditionsHandler();

        var firstCondition = CreateGetStageCondition(new FormKey(modKey, 0x000CC3));
        var secondCondition = CreateGetStageCondition(new FormKey(modKey, 0x000CC4));

        Assert.False(handler.AreValuesEqual([firstCondition], [secondCondition]));
    }

    [Fact]
    public void CtdaPaddingDoesNotChangeConditionIdentity()
    {
        var handler = new ConditionsHandler();
        var firstCondition = CreateFurnitureCondition(FurnitureAnimType.Sit);
        var secondCondition = CreateFurnitureCondition(FurnitureAnimType.Sit);

        firstCondition.Unknown1 = new byte[] { 1, 2, 3 };
        firstCondition.Unknown2 = 9704;
        secondCondition.Unknown1 = new byte[] { 4, 5, 6 };
        secondCondition.Unknown2 = 0;

        Assert.True(handler.AreValuesEqual([firstCondition], [secondCondition]));
    }

    [Fact]
    public void FunctionSpecificParametersChangeConditionIdentity()
    {
        var handler = new ConditionsHandler();
        var sittingCondition = CreateFurnitureCondition(FurnitureAnimType.Sit);
        var leaningCondition = CreateFurnitureCondition(FurnitureAnimType.Lean);

        Assert.False(handler.AreValuesEqual([sittingCondition], [leaningCondition]));
    }

    [Fact]
    public void UnusedFunctionParameterDoesNotChangeConditionIdentity()
    {
        var handler = new ConditionsHandler();
        var firstCondition = CreateGraphVariableCondition(28795872);
        var secondCondition = CreateGraphVariableCondition(0);

        Assert.True(handler.AreValuesEqual([firstCondition], [secondCondition]));
    }

    [Fact]
    public void ComparisonValueChangesConditionIdentity()
    {
        var handler = new ConditionsHandler();
        var firstCondition = CreateFurnitureCondition(FurnitureAnimType.Sit);
        var secondCondition = CreateFurnitureCondition(FurnitureAnimType.Sit);
        secondCondition.ComparisonValue = 2;

        Assert.False(handler.AreValuesEqual([firstCondition], [secondCondition]));
    }

    [Fact]
    public void XEditAlignmentIgnoresOperatorAndComparisonValue()
    {
        var handler = new InspectableConditionsHandler();
        var firstCondition = CreateRandomPercentCondition(
            CompareOperator.LessThanOrEqualTo,
            5);
        var secondCondition = CreateRandomPercentCondition(
            CompareOperator.EqualTo,
            -1);

        Assert.False(handler.AreValuesEqual([firstCondition], [secondCondition]));
        Assert.True(handler.AlignmentEquals(firstCondition, secondCondition));
    }

    [Fact]
    public void XEditAlignmentIncludesFunctionParameters()
    {
        var handler = new InspectableConditionsHandler();
        var firstCondition = CreateGetStageCondition(
            new FormKey(SourceModKey, 0x1704));
        var secondCondition = CreateGetStageCondition(
            new FormKey(SourceModKey, 0x1705));

        Assert.False(handler.AlignmentEquals(firstCondition, secondCondition));
    }

    [Fact]
    public void XEditAlignmentUsesCaseSensitiveStringParameters()
    {
        var handler = new InspectableConditionsHandler();
        var firstCondition = CreateGraphVariableCondition(0, "IsEquipping");
        var secondCondition = CreateGraphVariableCondition(0, "isequipping");

        Assert.False(handler.AlignmentEquals(firstCondition, secondCondition));
    }

    [Fact]
    public void DiagnosticIncludesFunctionSpecificParameters()
    {
        var handler = new InspectableConditionsHandler();

        var formatted = handler.Format(CreateFurnitureCondition(FurnitureAnimType.Lean));

        Assert.Contains("Param1:Lean", formatted, StringComparison.Ordinal);
        Assert.DoesNotContain("Unused", formatted, StringComparison.Ordinal);
        Assert.DoesNotContain("U2:", formatted, StringComparison.Ordinal);
    }

    [Fact]
    public void BinaryOverlayPaddingDoesNotChangeConditionIdentity()
    {
        var source = new SkyrimMod(SourceModKey, SkyrimRelease.SkyrimSE);
        var idle = new IdleAnimation(new FormKey(SourceModKey, 0x1700), SkyrimRelease.SkyrimSE);
        var sourceCondition = CreateFurnitureCondition(FurnitureAnimType.Sit);
        sourceCondition.Unknown1 = new byte[] { 1, 2, 3 };
        sourceCondition.Unknown2 = 9704;
        idle.Conditions.Add(sourceCondition);
        source.IdleAnimations.Add(idle);

        using var stream = new MemoryStream();
        source.WriteToBinary(stream);
        stream.Position = 0;
        using var overlay = SkyrimMod.CreateFromBinaryOverlay(stream, SkyrimRelease.SkyrimSE, SourceModKey);
        var overlayCondition = Assert.Single(Assert.Single(overlay.IdleAnimations).Conditions);

        var handler = new ConditionsHandler();
        Assert.True(handler.AreValuesEqual(
            [overlayCondition],
            [CreateFurnitureCondition(FurnitureAnimType.Sit)]));
    }

    [Fact]
    public void BinaryOverlayUnusedFunctionParameterDoesNotChangeConditionIdentity()
    {
        var source = new SkyrimMod(SourceModKey, SkyrimRelease.SkyrimSE);
        var idle = new IdleAnimation(new FormKey(SourceModKey, 0x1701), SkyrimRelease.SkyrimSE);
        idle.Conditions.Add(CreateGraphVariableCondition(28795872));
        source.IdleAnimations.Add(idle);

        using var stream = new MemoryStream();
        source.WriteToBinary(stream);
        stream.Position = 0;
        using var overlay = SkyrimMod.CreateFromBinaryOverlay(stream, SkyrimRelease.SkyrimSE, SourceModKey);
        var overlayCondition = Assert.Single(Assert.Single(overlay.IdleAnimations).Conditions);

        var handler = new ConditionsHandler();
        Assert.True(handler.AreValuesEqual([overlayCondition], [CreateGraphVariableCondition(0)]));
    }

    [Fact]
    public void BinaryOverlayFormLinkParameterMatchesMutableCondition()
    {
        var quest = new FormKey(SourceModKey, 0x1703);
        var source = new SkyrimMod(SourceModKey, SkyrimRelease.SkyrimSE);
        var idle = new IdleAnimation(new FormKey(SourceModKey, 0x1702), SkyrimRelease.SkyrimSE);
        idle.Conditions.Add(CreateGetStageCondition(quest));
        source.IdleAnimations.Add(idle);

        using var stream = new MemoryStream();
        source.WriteToBinary(stream);
        stream.Position = 0;
        using var overlay = SkyrimMod.CreateFromBinaryOverlay(stream, SkyrimRelease.SkyrimSE, SourceModKey);
        var overlayCondition = Assert.Single(Assert.Single(overlay.IdleAnimations).Conditions);

        var handler = new ConditionsHandler();
        Assert.True(handler.AreValuesEqual([overlayCondition], [CreateGetStageCondition(quest)]));
    }

    [Fact]
    public void GlobalComparisonValueChangesConditionIdentity()
    {
        var handler = new ConditionsHandler();
        var firstCondition = CreateGlobalCondition(new FormKey(SourceModKey, 0x701));
        var secondCondition = CreateGlobalCondition(new FormKey(SourceModKey, 0x702));

        Assert.False(handler.AreValuesEqual([firstCondition], [secondCondition]));
    }

    private static ConditionFloat CreateGetStageCondition(FormKey quest)
    {
        var data = new GetStageConditionData();
        data.Quest.Link.SetTo(quest);

        return new ConditionFloat
        {
            CompareOperator = CompareOperator.GreaterThanOrEqualTo,
            ComparisonValue = 30,
            Data = data
        };
    }

    private static ConditionFloat CreateFurnitureCondition(FurnitureAnimType furnitureAnimType)
    {
        return new ConditionFloat
        {
            CompareOperator = CompareOperator.EqualTo,
            ComparisonValue = 1,
            Data = new IsFurnitureAnimTypeConditionData
            {
                FurnitureAnimType = furnitureAnimType
            }
        };
    }

    private static ConditionFloat CreateRandomPercentCondition(
        CompareOperator compareOperator,
        float comparisonValue)
    {
        return new ConditionFloat
        {
            CompareOperator = compareOperator,
            ComparisonValue = comparisonValue,
            Data = new GetRandomPercentConditionData()
        };
    }

    private static ConditionFloat CreateGraphVariableCondition(
        int unusedParameter,
        string graphVariable = "IsEquipping")
    {
        return new ConditionFloat
        {
            CompareOperator = CompareOperator.NotEqualTo,
            ComparisonValue = 1,
            Data = new GetGraphVariableIntConditionData
            {
                FirstUnusedIntParameter = unusedParameter,
                GraphVariable = graphVariable
            }
        };
    }

    private static ConditionGlobal CreateGlobalCondition(FormKey global)
    {
        var condition = new ConditionGlobal
        {
            CompareOperator = CompareOperator.EqualTo,
            Data = new IsPS3ConditionData()
        };
        condition.ComparisonValue.SetTo(global);
        return condition;
    }

    private sealed class InspectableConditionsHandler :
        AbstractConditionsHandler<IConstructibleObjectGetter, IConstructibleObject>
    {
        public string Format(IConditionGetter condition) => FormatItem(condition);
        public bool AlignmentEquals(IConditionGetter condition1, IConditionGetter condition2) =>
            IsAlignmentEqual(condition1, condition2);

        protected override void UpdateConditionsCollection(
            IConstructibleObject record,
            List<IConditionGetter> conditions)
        {
            record.Conditions.Clear();
            foreach (var condition in conditions)
            {
                record.Conditions.Add(condition.DeepCopy());
            }
        }

        protected override IEnumerable<IConditionGetter>? GetConditions(IConstructibleObjectGetter record) =>
            record.Conditions;

        protected override IEnumerable<IConditionGetter>? GetConditions(IConstructibleObject record) =>
            record.Conditions;
    }
}
