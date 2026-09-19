using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class ListSemanticsTests
{
    [Fact]
    public void XEditSortKeyComparerUsesCompositeKeysAndLoadOrderFormIds()
    {
        var early = ModKey.FromNameAndExtension("Early.esm");
        var late = ModKey.FromNameAndExtension("Late.esp");
        var comparer = new XEditSortKeyComparer(new Dictionary<ModKey, int>
        {
            [early] = 0,
            [late] = 1
        });

        Assert.True(comparer.Compare([1, "a"], [1, "B"]) < 0);
        Assert.True(comparer.Compare(
            [new FormKey(early, 0xFFFFFF)],
            [new FormKey(late, 1)]) < 0);
        Assert.True(comparer.Compare([1], [1, 0]) < 0);
    }

    [Theory]
    [InlineData(typeof(ArmorRecordHandler), "Armature", ListSemantics.AlignedOrdered)]
    [InlineData(typeof(MagicEffectRecordHandler), "Sounds", ListSemantics.SortedKeyed)]
    [InlineData(typeof(LandscapeTextureRecordHandler), "Grasses", ListSemantics.SortedKeyed)]
    [InlineData(typeof(PlacedObjectRecordHandler), "LinkedReferences", ListSemantics.SortedKeyed)]
    [InlineData(typeof(PlacedNpcRecordHandler), "LinkedReferences", ListSemantics.SortedKeyed)]
    [InlineData(typeof(QuestRecordHandler), "TextDisplayGlobals", ListSemantics.AlignedOrdered)]
    [InlineData(typeof(WeatherRecordHandler), "Sounds", ListSemantics.SortedKeyed)]
    public void AuditedListRegistrationsUseTheirExplicitSemantics(
        Type recordHandlerType,
        string propertyName,
        ListSemantics expected)
    {
        var recordHandler = Assert.IsAssignableFrom<ForwardChanges.RecordHandlers.Abstracts.AbstractRecordHandler>(
            Activator.CreateInstance(recordHandlerType));
        var propertyHandler = recordHandler.PropertyHandlers[propertyName];
        var semantics = propertyHandler.GetType().GetProperty("Semantics")
            ?.GetValue(propertyHandler);

        Assert.Equal(expected, semantics);
    }

    [Theory]
    [InlineData(typeof(FurnitureRecordHandler), "Markers")]
    [InlineData(typeof(IdleMarkerRecordHandler), "Animations")]
    [InlineData(typeof(MusicTypeRecordHandler), "Tracks")]
    [InlineData(typeof(MusicTrackRecordHandler), "Tracks")]
    public void StructuralListsUseAtomicPropertyContexts(Type recordHandlerType, string propertyName)
    {
        var recordHandler = Assert.IsAssignableFrom<ForwardChanges.RecordHandlers.Abstracts.AbstractRecordHandler>(
            Activator.CreateInstance(recordHandlerType));
        var propertyHandler = recordHandler.PropertyHandlers[propertyName];

        Assert.StartsWith("AtomicReflectionListPropertyHandler", propertyHandler.GetType().Name);
        Assert.StartsWith("SimplePropertyContext", propertyHandler.CreatePropertyContext().GetType().Name);
    }

    [Theory]
    [InlineData("Phases", "ScenePhasesHandler")]
    [InlineData("Actors", "SceneActorsHandler")]
    [InlineData("Actions", "SceneActionsHandler")]
    public void SceneStructuralListsUseTypedAtomicPropertyContexts(
        string propertyName,
        string handlerTypeName)
    {
        var propertyHandler = new SceneRecordHandler().PropertyHandlers[propertyName];

        Assert.Equal(handlerTypeName, propertyHandler.GetType().Name);
        Assert.StartsWith("SimplePropertyContext", propertyHandler.CreatePropertyContext().GetType().Name);
    }

    [Fact]
    public void RegionAreasUseTypedAtomicPropertyContext()
    {
        var propertyHandler = new RegionRecordHandler().PropertyHandlers["RegionAreas"];

        Assert.Equal("RegionAreasHandler", propertyHandler.GetType().Name);
        Assert.StartsWith("SimplePropertyContext", propertyHandler.CreatePropertyContext().GetType().Name);
    }
}
