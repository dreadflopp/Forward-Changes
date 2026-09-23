using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace DreadsMashedPatch.Tests;

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
    [InlineData(typeof(MagicEffectRecordHandler), "Sounds", ListSemantics.SortedKeyed)]
    [InlineData(typeof(LandscapeTextureRecordHandler), "Grasses", ListSemantics.SortedKeyed)]
    [InlineData(typeof(PlacedObjectRecordHandler), "LinkedReferences", ListSemantics.ExactOrdered)]
    [InlineData(typeof(PlacedNpcRecordHandler), "LinkedReferences", ListSemantics.SortedKeyed)]
    [InlineData(typeof(QuestRecordHandler), "TextDisplayGlobals", ListSemantics.AlignedOrdered)]
    [InlineData(typeof(WeatherRecordHandler), "Sounds", ListSemantics.SortedKeyed)]
    public void AuditedListRegistrationsUseTheirExplicitSemantics(
        Type recordHandlerType,
        string propertyName,
        ListSemantics expected)
    {
        var recordHandler = Assert.IsAssignableFrom<DreadsMashedPatch.RecordHandlers.Abstracts.AbstractRecordHandler>(
            System.Activator.CreateInstance(recordHandlerType));
        var propertyHandler = recordHandler.PropertyHandlers[propertyName];
        var semantics = propertyHandler.GetType().GetProperty("Semantics")
            ?.GetValue(propertyHandler);

        Assert.Equal(expected, semantics);
    }

    [Fact]
    public void PlacedObjectLinkedReferencesTreatReversedRowsAsDifferent()
    {
        var handler = new PlacedObjectRecordHandler().PropertyHandlers["LinkedReferences"];
        var first = LinkedReference(0x0E8500, 0x000B0D);
        var second = LinkedReference(0x0D5B87, 0x000AB6);

        Assert.True(handler.AreValuesEqual(
            new List<ILinkedReferencesGetter> { first, second },
            new List<ILinkedReferencesGetter> { first.DeepCopy(), second.DeepCopy() }));
        Assert.False(handler.AreValuesEqual(
            new List<ILinkedReferencesGetter> { first, second },
            new List<ILinkedReferencesGetter> { second.DeepCopy(), first.DeepCopy() }));
    }

    [Fact]
    public void PlacedNpcLinkedReferencesRemainSortedAndOrderIndependent()
    {
        var handler = new PlacedNpcRecordHandler().PropertyHandlers["LinkedReferences"];
        var first = LinkedReference(0x0E8500, 0x000B0D);
        var second = LinkedReference(0x0D5B87, 0x000AB6);

        Assert.True(handler.AreValuesEqual(
            new List<ILinkedReferencesGetter> { first, second },
            new List<ILinkedReferencesGetter> { second.DeepCopy(), first.DeepCopy() }));
    }

    [Fact]
    public void PlacedObjectLinkedReferencesWriterPreservesDeclaredOrder()
    {
        var handler = new PlacedObjectRecordHandler().PropertyHandlers["LinkedReferences"];
        var first = LinkedReference(0x0E8500, 0x000B0D);
        var second = LinkedReference(0x0D5B87, 0x000AB6);
        var target = new PlacedObject(
            new FormKey(ModKey.FromNameAndExtension("Patch.esp"), 0x800),
            SkyrimRelease.SkyrimSE);

        handler.SetValue(
            target,
            new List<ILinkedReferencesGetter> { first, second });

        Assert.Equal(
            new[] { first.KeywordOrReference.FormKey, second.KeywordOrReference.FormKey },
            target.LinkedReferences.Select(entry => entry.KeywordOrReference.FormKey));
    }

    private static LinkedReferences LinkedReference(uint keywordId, uint referenceId)
    {
        var skyrim = ModKey.FromNameAndExtension("Skyrim.esm");
        return new LinkedReferences
        {
            KeywordOrReference = new FormLink<IKeywordLinkedReferenceGetter>(new FormKey(skyrim, keywordId)),
            Reference = new FormLink<IPlacedGetter>(new FormKey(skyrim, referenceId))
        };
    }

    [Theory]
    [InlineData(typeof(FurnitureRecordHandler), "Markers")]
    [InlineData(typeof(IdleMarkerRecordHandler), "Animations")]
    [InlineData(typeof(MusicTypeRecordHandler), "Tracks")]
    [InlineData(typeof(MusicTrackRecordHandler), "Tracks")]
    public void StructuralListsUseAtomicPropertyContexts(Type recordHandlerType, string propertyName)
    {
        var recordHandler = Assert.IsAssignableFrom<DreadsMashedPatch.RecordHandlers.Abstracts.AbstractRecordHandler>(
            System.Activator.CreateInstance(recordHandlerType));
        var propertyHandler = recordHandler.PropertyHandlers[propertyName];

        Assert.StartsWith("AtomicReflectionListPropertyHandler", propertyHandler.GetType().Name);
        Assert.StartsWith("SimplePropertyContext", propertyHandler.CreatePropertyContext().GetType().Name);
    }

    [Fact]
    public void ArmorArmatureUsesAtomicFormLinkListContext()
    {
        var propertyHandler = new ArmorRecordHandler().PropertyHandlers["Armature"];

        Assert.IsType<AtomicFormLinkListPropertyHandler<
            Mutagen.Bethesda.Skyrim.IArmorAddonGetter,
            Mutagen.Bethesda.Skyrim.IArmor,
            Mutagen.Bethesda.Skyrim.IArmorGetter>>(propertyHandler);
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
