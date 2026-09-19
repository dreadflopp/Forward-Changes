using DreadsMashedPatch.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace DreadsMashedPatch.PropertyHandlers.Region;

/// <summary>
/// Treats the complete REGN Region Areas array atomically while comparing and
/// copying its nested point lists structurally. xEdit normalizes each polygon's
/// direction after load by making its first point lexicographically no greater
/// than its last point; equality mirrors that behavior.
/// </summary>
public sealed class RegionAreasHandler : AbstractPropertyHandler<List<IRegionAreaGetter>>
{
    public override string PropertyName => "RegionAreas";

    public override List<IRegionAreaGetter>? GetValue(IMajorRecordGetter record) =>
        record is IRegionGetter region ? region.RegionAreas.ToList() : null;

    public override void SetValue(IMajorRecord record, List<IRegionAreaGetter>? value)
    {
        if (record is not IRegion region)
        {
            return;
        }

        region.RegionAreas.Clear();
        if (value == null)
        {
            return;
        }

        foreach (var area in value)
        {
            // Mutagen's generated DeepCopy converts IReadOnlyList<P2Float> from
            // overlays into the mutable ExtendedList<P2Float> required for output.
            region.RegionAreas.Add(area.DeepCopy());
        }
    }

    public override bool AreValuesEqual(
        List<IRegionAreaGetter>? value1,
        List<IRegionAreaGetter>? value2)
    {
        if (ReferenceEquals(value1, value2)) return true;
        if (value1 == null || value2 == null || value1.Count != value2.Count) return false;

        // xEdit declares Region Areas with wbRArray, not wbRArrayS, so outer area
        // order remains significant. Only polygon direction is normalized.
        for (var index = 0; index < value1.Count; index++)
        {
            if (!AreAreasEqual(value1[index], value2[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AreAreasEqual(IRegionAreaGetter left, IRegionAreaGetter right)
    {
        if (left.EdgeFallOff != right.EdgeFallOff)
        {
            return false;
        }

        return ArePointListsEqual(left.RegionPointListData, right.RegionPointListData);
    }

    private static bool ArePointListsEqual(
        IReadOnlyList<P2Float>? left,
        IReadOnlyList<P2Float>? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left == null || right == null || left.Count != right.Count) return false;

        var reverseLeft = NeedsXEditDirectionFlip(left);
        var reverseRight = NeedsXEditDirectionFlip(right);
        for (var index = 0; index < left.Count; index++)
        {
            var leftPoint = left[reverseLeft ? left.Count - 1 - index : index];
            var rightPoint = right[reverseRight ? right.Count - 1 - index : index];
            if (!leftPoint.X.EqualsWithin(rightPoint.X, P3FloatComparison.DefaultEpsilon)
                || !leftPoint.Y.EqualsWithin(rightPoint.Y, P3FloatComparison.DefaultEpsilon))
            {
                return false;
            }
        }

        return true;
    }

    private static bool NeedsXEditDirectionFlip(IReadOnlyList<P2Float> points)
    {
        if (points.Count <= 1)
        {
            return false;
        }

        var first = points[0];
        var last = points[^1];
        var xComparison = first.X.CompareTo(last.X);
        return xComparison > 0 || (xComparison == 0 && first.Y.CompareTo(last.Y) > 0);
    }
}
