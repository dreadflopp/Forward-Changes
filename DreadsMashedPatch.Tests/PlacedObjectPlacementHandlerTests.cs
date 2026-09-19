using DreadsMashedPatch.PropertyHandlers.PlacedObject;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class PlacedObjectPlacementHandlerTests
{
    [Fact]
    public void RecordHandlerUsesReadablePlacementFormatter()
    {
        var handler = Assert.IsType<PlacementHandler>(
            new PlacedObjectRecordHandler().PropertyHandlers["Placement"]);
        var placement = new Placement
        {
            Position = new P3Float(1.25f, -2.5f, -0f),
            Rotation = new P3Float(0.1f, 0.2f, 0.3f)
        };

        var formatted = handler.FormatValue(placement);

        Assert.Equal(
            "Position=(1.2500, -2.5000, 0.0000), Rotation=(0.1000, 0.2000, 0.3000)",
            formatted);
    }

    [Fact]
    public void FormatterPreservesNullOutput()
    {
        Assert.Equal("null", new PlacementHandler().FormatValue(null));
    }
}
