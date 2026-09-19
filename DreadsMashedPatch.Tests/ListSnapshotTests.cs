using DreadsMashedPatch.Contexts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Noggog;
using Xunit;
using ContainerItemHandler = DreadsMashedPatch.PropertyHandlers.Container.ItemHandler;

namespace DreadsMashedPatch.Tests;

public sealed class ListSnapshotTests
{
    [Fact]
    public void SortedKeyedInitializationKeepsOriginalAndForwardItemsIndependent()
    {
        var record = new Container(new FormKey(OriginalModKey, 0x100), SkyrimRelease.SkyrimSE)
        {
            Items = new ExtendedList<ContainerEntry>
            {
                new()
                {
                    Item = new ContainerItem
                    {
                        Item = new FormLink<IItemGetter>(new FormKey(OriginalModKey, 0x101)),
                        Count = 1
                    }
                }
            }
        };
        IPropertyHandler handler = new ContainerItemHandler();
        var propertyContext = Assert.IsType<ListPropertyContext<ContainerEntry>>(handler.CreatePropertyContext());
        var recordContext = CreateContext(record);

        handler.InitializeContext(recordContext, recordContext, propertyContext);

        var original = Assert.Single(propertyContext.OriginalValueContexts!);
        var forward = Assert.Single(propertyContext.ForwardValueContexts!);
        Assert.NotSame(propertyContext.OriginalValueContexts, propertyContext.ForwardValueContexts);
        Assert.NotSame(original, forward);
        Assert.NotSame(original.Value, forward.Value);

        forward.Value.Item.Count = 2;
        forward.OwnerMod = "Changing.esp";
        forward.IsRemoved = true;

        Assert.Equal(1, original.Value.Item.Count);
        Assert.Equal(OriginalModKey.ToString(), original.OwnerMod);
        Assert.False(original.IsRemoved);
    }

    private static IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> CreateContext(
        IMajorRecord record)
        => new ModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>(
            OriginalModKey,
            record,
            (_, _) => throw new NotSupportedException(),
            (_, _, _, _) => throw new NotSupportedException());

    private static readonly ModKey OriginalModKey = new("Skyrim.esm", ModType.Master);
}
