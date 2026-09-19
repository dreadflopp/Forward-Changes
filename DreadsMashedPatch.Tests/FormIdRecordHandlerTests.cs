using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.FormList;
using DreadsMashedPatch.RecordHandlers;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class FormIdRecordHandlerTests
{
    [Fact]
    public void ItemsUsesProgressiveOrderedListMode()
    {
        var recordHandler = new FormIdRecordHandler();
        Assert.IsType<FormIdsHandler>(recordHandler.PropertyHandlers["Items"]);

        var handler = new InspectableFormIdsHandler();
        Assert.Equal(ListSemantics.AlignedOrdered, handler.ExposedSemantics);
    }

    private sealed class InspectableFormIdsHandler : FormIdsHandler
    {
        public ListSemantics ExposedSemantics => Semantics;
    }
}
