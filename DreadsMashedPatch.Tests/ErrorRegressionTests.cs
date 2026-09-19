using ForwardChanges.PropertyHandlers.Class;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Landscape;
using ForwardChanges.PropertyHandlers.Perk;
using ForwardChanges.RecordHandlers;
using ForwardChanges.RecordHandlers.Abstracts;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Xunit;
using SkyrimClass = Mutagen.Bethesda.Skyrim.Class;

namespace ForwardChanges.Tests;

public sealed class ErrorRegressionTests
{
    private static readonly ModKey TestModKey = new("ErrorRegressionTests", ModType.Plugin);

    [Fact]
    public void EveryRecordHandlerCanBeConstructedWithoutLoadOrderRecords()
    {
        var failures = typeof(AbstractRecordHandler).Assembly
            .GetTypes()
            .Where(type =>
                !type.IsAbstract
                && typeof(AbstractRecordHandler).IsAssignableFrom(type)
                && type.GetConstructor(Type.EmptyTypes) != null)
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .Select(type =>
            {
                try
                {
                    _ = System.Activator.CreateInstance(type);
                    return null;
                }
                catch (Exception ex)
                {
                    var actual = ex is System.Reflection.TargetInvocationException { InnerException: not null }
                        ? ex.InnerException
                        : ex;
                    return $"{type.Name}: {actual!.GetType().Name}: {actual.Message}";
                }
            })
            .Where(failure => failure != null)
            .ToList();

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void GenericListHandlersHaveSafeMutableItemStrategies()
    {
        var failures = new List<string>();
        foreach (var recordHandler in CreateAllRecordHandlers())
        {
            foreach (var (propertyName, handler) in recordHandler.PropertyHandlers)
            {
                var handlerType = handler.GetType();
                if (!handlerType.IsGenericType
                    || handlerType.GetGenericTypeDefinition() != typeof(SimpleReflectionListPropertyHandler<,,>))
                {
                    continue;
                }

                var mutableType = handlerType
                    .GetField("_mutableItemType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    ?.GetValue(handler) as Type;
                var isFormLinkList = (bool)(handlerType
                    .GetField("_isFormLinkList", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    ?.GetValue(handler) ?? false);
                var itemType = handlerType.GetGenericArguments()[0];
                if (mutableType == null && itemType != typeof(string) && !isFormLinkList)
                {
                    failures.Add($"{recordHandler.GetType().Name}.{propertyName} -> no mutable type for {itemType.FullName}");
                }
                else if (mutableType is { IsAbstract: true } || mutableType is { IsInterface: true })
                {
                    failures.Add($"{recordHandler.GetType().Name}.{propertyName} -> {mutableType.FullName}");
                }
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void GenericComplexHandlersDoNotTreatGenderedAggregatesAsLists()
    {
        var failures = CreateAllRecordHandlers()
            .SelectMany(recordHandler => recordHandler.PropertyHandlers.Select(pair => (recordHandler, pair)))
            .Where(item =>
            {
                var handlerType = item.pair.Value.GetType();
                if (!handlerType.IsGenericType
                    || handlerType.GetGenericTypeDefinition() != typeof(ComplexReflectionPropertyHandler<,,>))
                {
                    return false;
                }

                var valueType = handlerType.GetGenericArguments()[0];
                return valueType.IsGenericType
                    && valueType.GetGenericTypeDefinition() == typeof(IGenderedItemGetter<>);
            })
            .Select(item => $"{item.recordHandler.GetType().Name}.{item.pair.Key}")
            .ToList();

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void SharedRegistrationContextsAreFilteredBeforeSubtypeCasting()
    {
        var intRecord = new GlobalInt(new FormKey(TestModKey, 0x615), SkyrimRelease.SkyrimSE);
        var floatRecord = new GlobalFloat(new FormKey(TestModKey, 0x616), SkyrimRelease.SkyrimSE);
        IModContext<ISkyrimMod, ISkyrimModGetter, IGlobal, IGlobalGetter>[] baseContexts =
        [
            new ModContext<ISkyrimMod, ISkyrimModGetter, IGlobal, IGlobalGetter>(
                TestModKey,
                intRecord,
                (_, _) => throw new NotSupportedException(),
                (_, _, _, _) => throw new NotSupportedException()),
            new ModContext<ISkyrimMod, ISkyrimModGetter, IGlobal, IGlobalGetter>(
                TestModKey,
                floatRecord,
                (_, _) => throw new NotSupportedException(),
                (_, _, _, _) => throw new NotSupportedException())
        ];
        var method = typeof(Program)
            .GetMethod("NarrowContexts", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
            .MakeGenericMethod(typeof(IGlobal), typeof(IGlobalGetter), typeof(IGlobalInt), typeof(IGlobalIntGetter));

        var narrowed = Assert.IsType<IModContext<ISkyrimMod, ISkyrimModGetter, IGlobalInt, IGlobalIntGetter>[]>(
            method.Invoke(null, [baseContexts]));

        Assert.Same(intRecord, Assert.Single(narrowed).Record);
    }

    [Fact]
    public void DialogLinkToUsesDialogTopicLinks()
    {
        var handler = Assert.IsType<SimpleReflectionListPropertyHandler<IFormLinkGetter<IDialogTopicGetter>, IDialogResponses, IDialogResponsesGetter>>(
            new DialogResponseRecordHandler().PropertyHandlers["LinkTo"]);
        var target = new DialogResponses(new FormKey(TestModKey, 0x600), SkyrimRelease.SkyrimSE);
        var topicKey = new FormKey(TestModKey, 0x601);

        handler.SetValue(target,
        [
            new FormLink<IDialogTopicGetter>(topicKey)
        ]);

        Assert.Equal(topicKey, Assert.Single(target.LinkTo).FormKey);
    }

    [Fact]
    public void StringListComparisonUsesTypedEquality()
    {
        var handler = Assert.IsType<SimpleReflectionListPropertyHandler<string, IRace, IRaceGetter>>(
            new RaceRecordHandler().PropertyHandlers["MovementTypeNames"]);

        Assert.True(handler.AreValuesEqual(["Walk", "Run"], ["Walk", "Run"]));
        Assert.False(handler.AreValuesEqual(["Walk", "Run"], ["Walk", "Sprint"]));
    }

    [Fact]
    public void StringListsAreCopiedWithoutMutableTypeConstruction()
    {
        var handler = Assert.IsType<SimpleReflectionListPropertyHandler<string, IRace, IRaceGetter>>(
            new RaceRecordHandler().PropertyHandlers["MovementTypeNames"]);
        var target = new Race(new FormKey(TestModKey, 0x611), SkyrimRelease.SkyrimSE);

        handler.SetValue(target, ["Walk", "Run"]);

        Assert.Equal(["Walk", "Run"], target.MovementTypeNames);
    }

    [Fact]
    public void RaceGenderedAggregatesCopyMaleAndFemaleSlotsWithoutEnumerableConversion()
    {
        var handlers = new RaceRecordHandler().PropertyHandlers;
        var heightHandler = Assert.IsType<GenderedItemHandler<float, float, IRace, IRaceGetter>>(handlers["Height"]);
        var voicesHandler = Assert.IsType<GenderedItemHandler<IFormLinkGetter<IVoiceTypeGetter>, IFormLinkGetter<IVoiceTypeGetter>, IRace, IRaceGetter>>(handlers["Voices"]);
        var bodyDataHandler = Assert.IsType<GenderedItemHandler<IBodyDataGetter?, BodyData?, IRace, IRaceGetter>>(handlers["BodyData"]);
        var target = new Race(new FormKey(TestModKey, 0x612), SkyrimRelease.SkyrimSE);
        var maleVoice = new FormKey(TestModKey, 0x613);
        var femaleVoice = new FormKey(TestModKey, 0x614);
        var maleBodyData = new BodyData();

        heightHandler.SetValue(target, new GenderedItem<float>(1.05f, 0.95f));
        voicesHandler.SetValue(target, new GenderedItem<IFormLinkGetter<IVoiceTypeGetter>>(
            new FormLink<IVoiceTypeGetter>(maleVoice),
            new FormLink<IVoiceTypeGetter>(femaleVoice)));
        bodyDataHandler.SetValue(target, new GenderedItem<IBodyDataGetter?>(maleBodyData, null));

        Assert.Equal(1.05f, target.Height.Male);
        Assert.Equal(0.95f, target.Height.Female);
        Assert.Equal(maleVoice, target.Voices.Male.FormKey);
        Assert.Equal(femaleVoice, target.Voices.Female.FormKey);
        Assert.NotSame(maleBodyData, target.BodyData.Male);
        Assert.Null(target.BodyData.Female);
    }

    [Fact]
    public void RequiredNamesAreHandledWithoutOptionalNamedAspect()
    {
        var handler = new NameHandler();
        var key = new Key(new FormKey(TestModKey, 0x602), SkyrimRelease.SkyrimSE);
        var classRecord = new SkyrimClass(new FormKey(TestModKey, 0x603), SkyrimRelease.SkyrimSE);

        handler.SetValue(key, "Test Key");
        handler.SetValue(classRecord, "Test Class");

        Assert.Equal("Test Key", handler.GetValue(key));
        Assert.Equal("Test Class", handler.GetValue(classRecord));
    }

    [Fact]
    public void OptionalObjectBoundsAreCopiedForSoulGems()
    {
        var handler = new ObjectBoundsHandler();
        var soulGem = new SoulGem(new FormKey(TestModKey, 0x604), SkyrimRelease.SkyrimSE);
        var source = new ObjectBounds();

        handler.SetValue(soulGem, source);

        Assert.NotNull(soulGem.ObjectBounds);
        Assert.NotSame(source, soulGem.ObjectBounds);
        Assert.True(handler.AreValuesEqual(source, handler.GetValue(soulGem)));

        handler.SetValue(soulGem, null);
        Assert.Null(soulGem.ObjectBounds);
    }

    [Fact]
    public void InheritedGlobalAndStoryPropertiesResolve()
    {
        var globalHandler = Assert.IsType<SimpleReflectionFlagPropertyHandler<Global.MajorFlag, IGlobalInt, IGlobalIntGetter>>(
            new GlobalIntRecordHandler().PropertyHandlers["MajorFlags"]);
        var global = new GlobalInt(new FormKey(TestModKey, 0x605), SkyrimRelease.SkyrimSE);
        globalHandler.SetValue(global, Global.MajorFlag.Constant);
        Assert.Equal(Global.MajorFlag.Constant, global.MajorFlags);

        var parentHandler = Assert.IsType<SimpleReflectionFormLinkPropertyHandler<IAStoryManagerNodeGetter, IStoryManagerBranchNode, IStoryManagerBranchNodeGetter>>(
            new StoryManagerBranchNodeRecordHandler().PropertyHandlers["Parent"]);
        var branch = new StoryManagerBranchNode(new FormKey(TestModKey, 0x606), SkyrimRelease.SkyrimSE);
        var parentKey = new FormKey(TestModKey, 0x607);
        parentHandler.SetValue(branch, new FormLinkNullable<IAStoryManagerNodeGetter>(parentKey));
        Assert.Equal(parentKey, parentHandler.GetValue(branch)!.FormKey);
    }

    [Fact]
    public void ClassWeightsCopyIntoGetterOnlyDictionariesAndCompareByKey()
    {
        var handlers = new ClassRecordHandler().PropertyHandlers;
        var skillHandler = Assert.IsType<ClassWeightsHandler<Skill>>(handlers["SkillWeights"]);
        var statHandler = Assert.IsType<ClassWeightsHandler<BasicStat>>(handlers["StatWeights"]);
        var target = new SkyrimClass(new FormKey(TestModKey, 0x608), SkyrimRelease.SkyrimSE);

        skillHandler.SetValue(target, new Dictionary<Skill, byte>
        {
            [Skill.OneHanded] = 4,
            [Skill.Sneak] = 2
        });
        statHandler.SetValue(target, new Dictionary<BasicStat, byte>
        {
            [BasicStat.Health] = 3,
            [BasicStat.Magicka] = 1
        });

        Assert.Equal(4, target.SkillWeights[Skill.OneHanded]);
        Assert.Equal(2, target.SkillWeights[Skill.Sneak]);
        Assert.Equal(3, target.StatWeights[BasicStat.Health]);
        Assert.Equal(1, target.StatWeights[BasicStat.Magicka]);
        Assert.True(skillHandler.AreValuesEqual(
            new Dictionary<Skill, byte> { [Skill.Sneak] = 2, [Skill.OneHanded] = 4 },
            skillHandler.GetValue(target)));
    }

    [Fact]
    public void LandscapeArraysAreCopiedIntoMutableArray2dValues()
    {
        var handler = new LandscapeArray2dHandler(vertexNormals: true);
        var target = new Landscape(new FormKey(TestModKey, 0x609), SkyrimRelease.SkyrimSE);
        var source = new Array2d<P3UInt8>(2, 2, new P3UInt8(1, 2, 3));

        handler.SetValue(target, source);

        Assert.NotNull(target.VertexNormals);
        Assert.NotSame(source, target.VertexNormals);
        Assert.True(handler.AreValuesEqual(source, target.VertexNormals));
        target.VertexNormals![0, 0] = new P3UInt8(9, 9, 9);
        Assert.False(handler.AreValuesEqual(source, target.VertexNormals));
    }

    [Fact]
    public void PerkEffectsUseGeneratedPolymorphicDeepCopy()
    {
        var handler = Assert.IsType<EffectsHandler>(new PerkRecordHandler().PropertyHandlers["Effects"]);
        var target = new Perk(new FormKey(TestModKey, 0x610), SkyrimRelease.SkyrimSE);
        var source = new PerkQuestEffect { Stage = 10, Rank = 2 };

        handler.SetValue(target, [source]);

        var copied = Assert.IsType<PerkQuestEffect>(Assert.Single(target.Effects));
        Assert.NotSame(source, copied);
        Assert.Equal(source.Stage, copied.Stage);
        Assert.Equal(source.Rank, copied.Rank);
        Assert.True(handler.AreValuesEqual([source], [copied]));
    }

    private static IEnumerable<AbstractRecordHandler> CreateAllRecordHandlers()
    {
        return typeof(AbstractRecordHandler).Assembly
            .GetTypes()
            .Where(type =>
                !type.IsAbstract
                && typeof(AbstractRecordHandler).IsAssignableFrom(type)
                && type.GetConstructor(Type.EmptyTypes) != null)
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .Select(type => (AbstractRecordHandler)System.Activator.CreateInstance(type)!);
    }
}
