using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.FormList;
using ForwardChanges.RecordHandlers;
using Xunit;

namespace ForwardChanges.Tests;

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
