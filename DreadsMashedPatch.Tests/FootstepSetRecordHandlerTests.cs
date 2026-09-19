using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace ForwardChanges.Tests;

public class FootstepSetRecordHandlerTests
{
    private static readonly ModKey TestModKey = ModKey.FromNameAndExtension("FootstepTests.esp");

    [Fact]
    public void AllFiveListsUseAtomicHandlers()
    {
        var handlers = new FootstepSetRecordHandler().PropertyHandlers;
        var propertyNames = new[]
        {
            "WalkForwardFootsteps",
            "RunForwardFootsteps",
            "WalkForwardAlternateFootsteps",
            "RunForwardAlternateFootsteps",
            "WalkForwardAlternateFootsteps2"
        };

        foreach (var propertyName in propertyNames)
        {
            Assert.IsType<AtomicFormLinkListPropertyHandler<IFootstepGetter, IFootstepSet, IFootstepSetGetter>>(
                handlers[propertyName]);
        }
    }

    [Fact]
    public void AtomicHandlerPreservesOrderAndDuplicateMultiplicity()
    {
        var handler = new AtomicFormLinkListPropertyHandler<IFootstepGetter, IFootstepSet, IFootstepSetGetter>(
            "WalkForwardFootsteps");
        var first = new FormKey(TestModKey, 0x100);
        var second = new FormKey(TestModKey, 0x101);
        var source = new FootstepSet(new FormKey(TestModKey, 0x200), SkyrimRelease.SkyrimSE);
        var target = new FootstepSet(new FormKey(TestModKey, 0x201), SkyrimRelease.SkyrimSE);
        source.WalkForwardFootsteps.Add(new FormLink<IFootstepGetter>(first));
        source.WalkForwardFootsteps.Add(new FormLink<IFootstepGetter>(second));
        source.WalkForwardFootsteps.Add(new FormLink<IFootstepGetter>(first));

        var value = handler.GetValue(source);
        handler.SetValue(target, value);

        Assert.Equal([first, second, first], handler.GetValue(target));
        Assert.True(handler.AreValuesEqual([first, second, first], handler.GetValue(target)));
        Assert.False(handler.AreValuesEqual([first, first, second], handler.GetValue(target)));
        Assert.False(handler.AreValuesEqual([first, second, second], handler.GetValue(target)));
    }
}
