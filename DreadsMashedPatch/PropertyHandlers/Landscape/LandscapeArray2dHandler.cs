using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace ForwardChanges.PropertyHandlers.Landscape;

public sealed class LandscapeArray2dHandler : AbstractPropertyHandler<IReadOnlyArray2d<P3UInt8>>
{
    private readonly bool _vertexNormals;

    public LandscapeArray2dHandler(bool vertexNormals)
    {
        _vertexNormals = vertexNormals;
    }

    public override string PropertyName => _vertexNormals ? "VertexNormals" : "VertexColors";

    public override IReadOnlyArray2d<P3UInt8>? GetValue(IMajorRecordGetter record)
    {
        if (record is not ILandscapeGetter landscape)
        {
            Console.WriteLine($"Error: Record does not implement ILandscapeGetter for {PropertyName}");
            return null;
        }

        return _vertexNormals ? landscape.VertexNormals : landscape.VertexColors;
    }

    public override void SetValue(IMajorRecord record, IReadOnlyArray2d<P3UInt8>? value)
    {
        if (record is not ILandscape landscape)
        {
            Console.WriteLine($"Error: Record does not implement ILandscape for {PropertyName}");
            return;
        }

        var copy = value == null ? null : new Array2d<P3UInt8>(value);
        if (_vertexNormals)
        {
            landscape.VertexNormals = copy;
        }
        else
        {
            landscape.VertexColors = copy;
        }
    }

    public override bool AreValuesEqual(
        IReadOnlyArray2d<P3UInt8>? value1,
        IReadOnlyArray2d<P3UInt8>? value2)
    {
        if (ReferenceEquals(value1, value2)) return true;
        if (value1 == null || value2 == null) return false;
        if (value1.Width != value2.Width || value1.Height != value2.Height) return false;

        for (var x = 0; x < value1.Width; x++)
        {
            for (var y = 0; y < value1.Height; y++)
            {
                if (!value1[x, y].Equals(value2[x, y]))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
