using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class ListPropertyNullabilityTests
{
    [Fact]
    public void PlacedObjectAbsentPortalsRemainNullThroughInferredNullability()
    {
        var record = new PlacedObject(new FormKey(TestModKey, 0x97), SkyrimRelease.SkyrimSE);
        var propertyHandler = new PlacedObjectRecordHandler().PropertyHandlers["Portals"];
        var propertyContext = propertyHandler.CreatePropertyContext();
        var recordContext = CreateContext(record);

        propertyHandler.InitializeContext(recordContext, recordContext, propertyContext);

        Assert.Null(propertyHandler.GetValue(record));
        Assert.Null(propertyContext.GetForwardValue());
        Assert.True(propertyHandler.AreValuesEqual(
            propertyContext.GetForwardValue(),
            propertyHandler.GetValue(record)));
    }

    [Fact]
    public void PlacedObjectAbsentVirtualMachineAdapterRemainsNull()
    {
        var record = new PlacedObject(new FormKey(TestModKey, 0x98), SkyrimRelease.SkyrimSE);
        var propertyHandler = new PlacedObjectRecordHandler().PropertyHandlers["VirtualMachineAdapter"];
        var propertyContext = propertyHandler.CreatePropertyContext();
        var recordContext = CreateContext(record);

        propertyHandler.InitializeContext(recordContext, recordContext, propertyContext);

        Assert.Null(propertyHandler.GetValue(record));
        Assert.Null(propertyContext.GetForwardValue());
        Assert.True(propertyHandler.AreValuesEqual(
            propertyContext.GetForwardValue(),
            propertyHandler.GetValue(record)));
    }

    [Fact]
    public void PlacedObjectPresentEmptyVirtualMachineAdapterRemainsEmpty()
    {
        var record = new PlacedObject(new FormKey(TestModKey, 0x96), SkyrimRelease.SkyrimSE)
        {
            VirtualMachineAdapter = new VirtualMachineAdapter()
        };
        var propertyHandler = new PlacedObjectRecordHandler().PropertyHandlers["VirtualMachineAdapter"];
        var propertyContext = propertyHandler.CreatePropertyContext();
        var recordContext = CreateContext(record);

        propertyHandler.InitializeContext(recordContext, recordContext, propertyContext);

        var computed = Assert.IsType<List<object>>(propertyContext.GetForwardValue());
        Assert.Empty(computed);
        Assert.True(propertyHandler.AreValuesEqual(computed, propertyHandler.GetValue(record)));
    }

    [Fact]
    public void QuestAbsentVirtualMachineAdapterScriptsRemainNull()
    {
        var record = new Quest(new FormKey(TestModKey, 0x107), SkyrimRelease.SkyrimSE);
        var propertyHandler = new QuestRecordHandler().PropertyHandlers["VirtualMachineAdapter.Scripts"];
        var propertyContext = propertyHandler.CreatePropertyContext();
        var recordContext = CreateContext(record);

        propertyHandler.InitializeContext(recordContext, recordContext, propertyContext);

        Assert.Null(propertyHandler.GetValue(record));
        Assert.Null(propertyContext.GetForwardValue());
        Assert.True(propertyHandler.AreValuesEqual(
            propertyContext.GetForwardValue(),
            propertyHandler.GetValue(record)));
    }

    [Fact]
    public void QuestPresentEmptyVirtualMachineAdapterScriptsRemainEmpty()
    {
        var record = new Quest(new FormKey(TestModKey, 0x108), SkyrimRelease.SkyrimSE)
        {
            VirtualMachineAdapter = new QuestAdapter()
        };
        var propertyHandler = new QuestRecordHandler().PropertyHandlers["VirtualMachineAdapter.Scripts"];
        var propertyContext = propertyHandler.CreatePropertyContext();
        var recordContext = CreateContext(record);

        propertyHandler.InitializeContext(recordContext, recordContext, propertyContext);

        var computed = Assert.IsType<List<object>>(propertyContext.GetForwardValue());
        Assert.Empty(computed);
        Assert.True(propertyHandler.AreValuesEqual(computed, propertyHandler.GetValue(record)));
    }

    [Fact]
    public void PlacedObjectEmptyLinkedReferencesDoNotProduceNullForwardValue()
    {
        var record = new PlacedObject(new FormKey(TestModKey, 0x99), SkyrimRelease.SkyrimSE);
        var propertyHandler = new PlacedObjectRecordHandler().PropertyHandlers["LinkedReferences"];
        var propertyContext = propertyHandler.CreatePropertyContext();
        var recordContext = CreateContext(record);

        propertyHandler.InitializeContext(recordContext, recordContext, propertyContext);

        var computed = Assert.IsType<List<object>>(propertyContext.GetForwardValue());
        Assert.Empty(computed);
        Assert.True(propertyHandler.AreValuesEqual(computed, propertyHandler.GetValue(record)));
    }

    [Fact]
    public void NonNullableEmptyListRemainsEmptyAndEqualsWinningValue()
    {
        var record = new Race(new FormKey(TestModKey, 0x100), SkyrimRelease.SkyrimSE);
        var handler = new SimpleReflectionListPropertyHandler<string, IRace, IRaceGetter>(
            "MovementTypeNames",
            ListSemantics.Unordered);
        var propertyHandler = (IPropertyHandler)handler;
        var propertyContext = propertyHandler.CreatePropertyContext();
        var recordContext = CreateContext(record);

        propertyHandler.InitializeContext(recordContext, recordContext, propertyContext);

        var computed = Assert.IsType<List<object>>(propertyContext.GetForwardValue());
        Assert.Empty(computed);
        Assert.True(propertyHandler.AreValuesEqual(computed, propertyHandler.GetValue(record)));
        Assert.Equal("Empty", propertyHandler.FormatValue(computed));
    }

    [Fact]
    public void NullableNullListRemainsNull()
    {
        var record = new MusicType(new FormKey(TestModKey, 0x101), SkyrimRelease.SkyrimSE);
        var handler = CreateNullableTracksHandler();
        var propertyHandler = (IPropertyHandler)handler;
        var propertyContext = propertyHandler.CreatePropertyContext();
        var recordContext = CreateContext(record);

        propertyHandler.InitializeContext(recordContext, recordContext, propertyContext);

        Assert.Null(propertyContext.GetForwardValue());
        Assert.True(propertyHandler.AreValuesEqual(
            propertyContext.GetForwardValue(),
            propertyHandler.GetValue(record)));
    }

    [Fact]
    public void NullablePresentEmptyListRemainsEmpty()
    {
        var record = new MusicType(new FormKey(TestModKey, 0x102), SkyrimRelease.SkyrimSE)
        {
            Tracks = []
        };
        var handler = CreateNullableTracksHandler();
        var propertyHandler = (IPropertyHandler)handler;
        var propertyContext = propertyHandler.CreatePropertyContext();
        var recordContext = CreateContext(record);

        propertyHandler.InitializeContext(recordContext, recordContext, propertyContext);

        var computed = Assert.IsType<List<object>>(propertyContext.GetForwardValue());
        Assert.Empty(computed);
        Assert.True(propertyHandler.AreValuesEqual(computed, propertyHandler.GetValue(record)));
    }

    [Fact]
    public void NullableContextDistinguishesNullFromPresentEmpty()
    {
        var context = new ListPropertyContext<string>
        {
            CanBeNull = true,
            ForwardValueContexts = [],
            ForwardIsNull = true
        };

        Assert.Null(context.GetForwardValue());

        context.ForwardIsNull = false;

        Assert.Empty(Assert.IsType<List<object>>(context.GetForwardValue()));
    }

    [Fact]
    public void ActiveItemsAlwaysProducePresentList()
    {
        var context = new ListPropertyContext<string>
        {
            CanBeNull = true,
            ForwardValueContexts = [new ListPropertyValueContext<string>("value", TestModKey.ToString())],
            ForwardIsNull = true
        };

        var result = Assert.IsType<List<object>>(context.GetForwardValue());

        Assert.Equal("value", Assert.Single(result));
    }

    [Fact]
    public void FactionAbsentConditionsRemainNull()
    {
        var record = new Faction(new FormKey(TestModKey, 0x103), SkyrimRelease.SkyrimSE);
        var propertyHandler = new FactionRecordHandler().PropertyHandlers["Conditions"];
        var propertyContext = propertyHandler.CreatePropertyContext();
        var recordContext = CreateContext(record);

        propertyHandler.InitializeContext(recordContext, recordContext, propertyContext);

        Assert.Null(propertyHandler.GetValue(record));
        Assert.Null(propertyContext.GetForwardValue());
        Assert.True(propertyHandler.AreValuesEqual(
            propertyContext.GetForwardValue(),
            propertyHandler.GetValue(record)));
    }

    [Fact]
    public void FactionConditionsSetterPreservesNullAndEmptyAsDistinctStates()
    {
        var record = new Faction(new FormKey(TestModKey, 0x104), SkyrimRelease.SkyrimSE)
        {
            Conditions = []
        };
        var propertyHandler = new FactionRecordHandler().PropertyHandlers["Conditions"];

        propertyHandler.SetValue(record, null);
        Assert.Null(record.Conditions);

        propertyHandler.SetValue(record, new List<IConditionGetter>());
        Assert.NotNull(record.Conditions);
        Assert.Empty(record.Conditions);
    }

    [Fact]
    public void MusicTrackConditionsSetterPreservesNullAndEmptyAsDistinctStates()
    {
        var record = new MusicTrack(new FormKey(TestModKey, 0x106), SkyrimRelease.SkyrimSE)
        {
            Conditions = []
        };
        var propertyHandler = new MusicTrackRecordHandler().PropertyHandlers["Conditions"];

        propertyHandler.SetValue(record, null);
        Assert.Null(record.Conditions);

        propertyHandler.SetValue(record, new List<IConditionGetter>());
        Assert.NotNull(record.Conditions);
        Assert.Empty(record.Conditions);
    }

    [Fact]
    public void ObservedNullSurvivesMissingSpecializedNullabilityDeclaration()
    {
        var record = new Race(new FormKey(TestModKey, 0x105), SkyrimRelease.SkyrimSE);
        IPropertyHandler propertyHandler = new NullReturningListHandler();
        var propertyContext = propertyHandler.CreatePropertyContext();
        var recordContext = CreateContext(record);

        propertyHandler.InitializeContext(recordContext, recordContext, propertyContext);

        Assert.Null(propertyContext.GetForwardValue());
        Assert.True(propertyHandler.AreValuesEqual(
            propertyContext.GetForwardValue(),
            propertyHandler.GetValue(record)));
    }

    private static SimpleReflectionListPropertyHandler<IFormLinkGetter<IMusicTrackGetter>, IMusicType, IMusicTypeGetter>
        CreateNullableTracksHandler()
        => new("Tracks", ListSemantics.ExactOrdered, canBeNull: true);

    private static IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> CreateContext(
        IMajorRecord record)
        => new ModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>(
            TestModKey,
            record,
            (_, _) => throw new NotSupportedException(),
            (_, _, _, _) => throw new NotSupportedException());

    private static readonly ModKey TestModKey = new("ListNullabilityTests", ModType.Plugin);

    private sealed class NullReturningListHandler : AbstractListPropertyHandler<string>
    {
        public override string PropertyName => "TestList";

        public override List<string>? GetValue(IMajorRecordGetter record) => null;

        public override void SetValue(IMajorRecord record, List<string>? value)
        {
        }
    }
}
