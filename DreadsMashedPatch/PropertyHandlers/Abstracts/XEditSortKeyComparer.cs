using Mutagen.Bethesda.Plugins;

namespace DreadsMashedPatch.PropertyHandlers.Abstracts;

/// <summary>
/// Compares typed StructSK components the same way xEdit compares their rendered
/// sort keys: lexicographically, with FormIDs expressed in load-order space.
/// </summary>
internal sealed class XEditSortKeyComparer : IComparer<IReadOnlyList<object?>>
{
    private readonly IReadOnlyDictionary<ModKey, int> _loadOrder;

    public XEditSortKeyComparer(IReadOnlyDictionary<ModKey, int> loadOrder)
    {
        _loadOrder = loadOrder;
    }

    public int Compare(IReadOnlyList<object?>? left, IReadOnlyList<object?>? right)
    {
        if (ReferenceEquals(left, right)) return 0;
        if (left == null) return -1;
        if (right == null) return 1;

        var count = Math.Min(left.Count, right.Count);
        for (var index = 0; index < count; index++)
        {
            var comparison = ComparePart(left[index], right[index]);
            if (comparison != 0) return comparison;
        }

        return left.Count.CompareTo(right.Count);
    }

    private int ComparePart(object? left, object? right)
    {
        if (ReferenceEquals(left, right)) return 0;
        if (left == null) return -1;
        if (right == null) return 1;

        if (left is FormKey leftFormKey && right is FormKey rightFormKey)
        {
            var leftIndex = leftFormKey.ModKey.IsNull
                ? -1
                : _loadOrder.GetValueOrDefault(leftFormKey.ModKey, int.MaxValue);
            var rightIndex = rightFormKey.ModKey.IsNull
                ? -1
                : _loadOrder.GetValueOrDefault(rightFormKey.ModKey, int.MaxValue);
            var modComparison = leftIndex.CompareTo(rightIndex);
            return modComparison != 0
                ? modComparison
                : leftFormKey.ID.CompareTo(rightFormKey.ID);
        }

        if (left is string leftString && right is string rightString)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(leftString, rightString);
        }

        if (left.GetType().IsEnum && right.GetType() == left.GetType())
        {
            return Convert.ToInt64(left).CompareTo(Convert.ToInt64(right));
        }

        if (left is IComparable comparable && left.GetType() == right.GetType())
        {
            return comparable.CompareTo(right);
        }

        throw new InvalidOperationException(
            $"Unsupported xEdit sort-key component types {left.GetType().Name} and {right.GetType().Name}.");
    }
}
