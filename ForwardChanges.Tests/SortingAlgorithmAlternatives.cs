using ForwardChanges.Contexts;

namespace ForwardChanges.Tests;

/// <summary>
/// Alternative sorting algorithms for the list reordering problem.
/// Each alternative implements the same public interface: ProcessSortingAlgorithm.
/// Run against the same test suite to compare behavior.
///
/// SUMMARY OF ALTERNATIVES:
///
/// Alt1_AppendRemaining: Declared items first, then append remaining at end.
///   FAILS - remaining items must be interleaved to preserve before-relationships (e.g. Case 4).
///
/// Alt2_OriginalIndex: Place remaining at their original index (clamped).
///   FAILS - ignores before-relationships, produces wrong order.
///
/// Alt3_UnifiedPlacement: Same logic as current, restructured with single PlaceAfter(beforeItems) helper.
///   PASSES - simplest formulation that works. Same algorithm, cleaner structure.
///
/// Alt4_TopologicalSort: PlaceDeclared uses record order; PlaceRemaining uses recursive before-constraints.
///   PASSES - different structure, separates "declared" ordering from "remaining" ordering.
///
/// Alt5_InterleavedInsert: Before adding each declared item, insert remaining (no-permission) items that must go before it.
///   PASSES - interleaves as we go. More complex condition (only insert items we can't reorder).
///
/// RECOMMENDATION: Alt3 is the simplest that passes. It uses one PlaceAfter helper for both
/// existing remaining and new items, reducing code duplication.
/// </summary>
public static class SortingAlgorithmAlternatives
{
    public delegate void SortAlgorithm(
        List<string> recordItems,
        List<ListPropertyValueContext<string>> forwardValueContexts,
        string modKey,
        IReadOnlySet<string> masters);

    private static bool IsItemEqual(string? a, string? b) =>
        (a == null && b == null) || (a != null && b != null && a == b);

    private static bool HasPermissionsToModify(string recordModKey, IReadOnlySet<string> masters, string? ownerMod) =>
        ownerMod != null && (recordModKey == ownerMod || masters.Contains(ownerMod));

    // -------------------------------------------------------------------------
    // ALTERNATIVE 1: "Append remaining at end" - Declared first, then remaining
    // Expected to FAIL: remaining items must be interleaved to preserve before-relationships
    // -------------------------------------------------------------------------
    public static void Alt1_AppendRemaining(
        List<string> recordItems,
        List<ListPropertyValueContext<string>> forwardValueContexts,
        string modKey,
        IReadOnlySet<string> masters)
    {
        var active = forwardValueContexts.Where(i => !i.IsRemoved).ToList();
        var finalOrder = new List<ListPropertyValueContext<string>>();
        var remaining = new List<ListPropertyValueContext<string>>(active);

        foreach (var v in recordItems)
        {
            var m = remaining.FirstOrDefault(x => IsItemEqual(x.Value, v));
            if (m != null && HasPermissionsToModify(modKey, masters, m.OrderOwnerMod))
            {
                finalOrder.Add(m);
                remaining.Remove(m);
            }
        }
        finalOrder.AddRange(remaining);
        AppendOwnershipAndRemoved(forwardValueContexts, finalOrder, active, modKey);
    }

    // -------------------------------------------------------------------------
    // ALTERNATIVE 2: "Insert by original index" - Place remaining at their
    // original index (clamped). Ignores before-relationships.
    // Expected to FAIL: e.g. Case 4 needs C,A,B,D,E not C,A,E,B,D
    // -------------------------------------------------------------------------
    public static void Alt2_OriginalIndex(
        List<string> recordItems,
        List<ListPropertyValueContext<string>> forwardValueContexts,
        string modKey,
        IReadOnlySet<string> masters)
    {
        var active = forwardValueContexts.Where(i => !i.IsRemoved).ToList();
        var finalOrder = new List<ListPropertyValueContext<string>>();
        var remaining = new List<ListPropertyValueContext<string>>(active);

        foreach (var v in recordItems)
        {
            var m = remaining.FirstOrDefault(x => IsItemEqual(x.Value, v));
            if (m != null && HasPermissionsToModify(modKey, masters, m.OrderOwnerMod))
            {
                finalOrder.Add(m);
                remaining.Remove(m);
            }
        }

        var existingRemaining = remaining.Where(x => x.OrderOwnerMod != null).ToList();
        var newRemaining = remaining.Where(x => x.OrderOwnerMod == null).ToList();

        foreach (var r in existingRemaining)
        {
            int origIdx = active.FindIndex(x => ReferenceEquals(x, r));
            int pos = Math.Min(origIdx, finalOrder.Count);
            finalOrder.Insert(pos, r);
        }

        foreach (var r in newRemaining)
        {
            int idx = recordItems.FindIndex(x => IsItemEqual(x, r.Value));
            if (idx < 0) idx = recordItems.Count;
            int pos = Math.Min(idx, finalOrder.Count);
            finalOrder.Insert(pos, r);
        }

        AppendOwnershipAndRemoved(forwardValueContexts, finalOrder, active, modKey);
    }

    // -------------------------------------------------------------------------
    // ALTERNATIVE 3: "Before-relationships only" - Same as current but with
    // a single unified PlaceItem(beforeItems, finalOrder) helper.
    // Should PASS - same logic, restructured.
    // -------------------------------------------------------------------------
    public static void Alt3_UnifiedPlacement(
        List<string> recordItems,
        List<ListPropertyValueContext<string>> forwardValueContexts,
        string modKey,
        IReadOnlySet<string> masters)
    {
        var active = forwardValueContexts.Where(i => !i.IsRemoved).ToList();
        var finalOrder = new List<ListPropertyValueContext<string>>();
        var remaining = new List<ListPropertyValueContext<string>>(active);

        foreach (var v in recordItems)
        {
            var m = remaining.FirstOrDefault(x => IsItemEqual(x.Value, v));
            if (m != null && HasPermissionsToModify(modKey, masters, m.OrderOwnerMod))
            {
                finalOrder.Add(m);
                remaining.Remove(m);
            }
        }

        var existingRemaining = remaining.Where(x => x.OrderOwnerMod != null).ToList();
        var newRemaining = remaining.Where(x => x.OrderOwnerMod == null).ToList();

        int PlaceAfter(IReadOnlyList<ListPropertyValueContext<string>> beforeItems)
        {
            int pos = 0;
            foreach (var b in beforeItems)
            {
                int i = finalOrder.FindIndex(x => ReferenceEquals(x, b));
                if (i != -1) pos = Math.Max(pos, i + 1);
            }
            return Math.Min(pos, finalOrder.Count);
        }

        foreach (var r in existingRemaining)
        {
            var before = GetBeforeItems(active, r);
            finalOrder.Insert(PlaceAfter(before), r);
        }

        var sortedNew = new List<(ListPropertyValueContext<string> Item, int RecordIndex)>();
        var toMatch = new List<ListPropertyValueContext<string>>(newRemaining);
        for (int i = 0; i < recordItems.Count; i++)
        {
            var mi = toMatch.FindIndex(x => IsItemEqual(x.Value, recordItems[i]));
            if (mi >= 0) { sortedNew.Add((toMatch[mi], i)); toMatch.RemoveAt(mi); }
        }

        foreach (var (item, recIdx) in sortedNew)
        {
            if (recIdx <= 0) { finalOrder.Insert(0, item); }
            else
            {
                int pos = 0;
                for (int j = 0; j < recIdx; j++)
                {
                    int k = finalOrder.FindLastIndex(x => IsItemEqual(x.Value, recordItems[j]));
                    if (k != -1) pos = Math.Max(pos, k + 1);
                }
                finalOrder.Insert(Math.Min(pos, finalOrder.Count), item);
            }
            item.OrderOwnerMod = modKey;
        }

        UpdateOrderOwnership(finalOrder, active, modKey);
        var removed = forwardValueContexts.Where(x => x.IsRemoved).ToList();
        forwardValueContexts.Clear();
        forwardValueContexts.AddRange(finalOrder);
        forwardValueContexts.AddRange(removed);
    }

    private static List<ListPropertyValueContext<string>> GetBeforeItems(
        List<ListPropertyValueContext<string>> originalOrder,
        ListPropertyValueContext<string> item)
    {
        var before = new List<ListPropertyValueContext<string>>();
        foreach (var o in originalOrder)
        {
            if (ReferenceEquals(o, item)) break;
            before.Add(o);
        }
        return before;
    }

    private static void UpdateOrderOwnership(
        List<ListPropertyValueContext<string>> finalOrder,
        List<ListPropertyValueContext<string>> originalOrder,
        string modKey)
    {
        foreach (var f in finalOrder)
        {
            var ob = GetNeighbor(originalOrder, f, -1);
            var oa = GetNeighbor(originalOrder, f, 1);
            var fb = GetNeighbor(finalOrder, f, -1);
            var fa = GetNeighbor(finalOrder, f, 1);
            if (!AreEqual(ob, fb) && !AreEqual(oa, fa))
                f.OrderOwnerMod = modKey;
        }
    }

    private static ListPropertyValueContext<string>? GetNeighbor(
        List<ListPropertyValueContext<string>> list,
        ListPropertyValueContext<string> item,
        int offset)
    {
        int i = list.FindIndex(x => ReferenceEquals(x, item));
        int j = i + offset;
        return j >= 0 && j < list.Count ? list[j] : null;
    }

    private static bool AreEqual(ListPropertyValueContext<string>? a, ListPropertyValueContext<string>? b) =>
        (a == null && b == null) || (a != null && b != null && IsItemEqual(a.Value, b.Value));

    private static void AppendOwnershipAndRemoved(
        List<ListPropertyValueContext<string>> forwardValueContexts,
        List<ListPropertyValueContext<string>> finalOrder,
        List<ListPropertyValueContext<string>> active,
        string modKey)
    {
        UpdateOrderOwnership(finalOrder, active, modKey);
        var removed = forwardValueContexts.Where(x => x.IsRemoved).ToList();
        forwardValueContexts.Clear();
        forwardValueContexts.AddRange(finalOrder);
        forwardValueContexts.AddRange(removed);
    }

    // -------------------------------------------------------------------------
    // ALTERNATIVE 4: "Topological sort" - Build DAG from before-constraints,
    // then toposort. Different structure, same semantics.
    // -------------------------------------------------------------------------
    public static void Alt4_TopologicalSort(
        List<string> recordItems,
        List<ListPropertyValueContext<string>> forwardValueContexts,
        string modKey,
        IReadOnlySet<string> masters)
    {
        var active = forwardValueContexts.Where(i => !i.IsRemoved).ToList();
        var remaining = new List<ListPropertyValueContext<string>>(active);
        var inFinal = new HashSet<ListPropertyValueContext<string>>();
        var orderFromRecord = new List<ListPropertyValueContext<string>>();

        foreach (var v in recordItems)
        {
            var m = remaining.FirstOrDefault(x => IsItemEqual(x.Value, v));
            if (m != null && HasPermissionsToModify(modKey, masters, m.OrderOwnerMod))
            {
                orderFromRecord.Add(m);
                remaining.Remove(m);
                inFinal.Add(m);
            }
        }

        var existingRemaining = remaining.Where(x => x.OrderOwnerMod != null).ToList();
        var newRemaining = remaining.Where(x => x.OrderOwnerMod == null).ToList();

        int ConstraintPriority(ListPropertyValueContext<string> item)
        {
            int idx = active.FindIndex(x => ReferenceEquals(x, item));
            return idx >= 0 ? idx : int.MaxValue;
        }

        var finalOrder = new List<ListPropertyValueContext<string>>();
        var placed = new HashSet<ListPropertyValueContext<string>>();

        void PlaceDeclared(int ordinal)
        {
            if (ordinal >= orderFromRecord.Count) return;
            var item = orderFromRecord[ordinal];
            if (placed.Contains(item)) return;
            for (int i = 0; i < ordinal; i++)
                PlaceDeclared(i);
            int pos = 0;
            for (int i = 0; i < ordinal; i++)
            {
                int j = finalOrder.FindIndex(x => ReferenceEquals(x, orderFromRecord[i]));
                if (j != -1) pos = Math.Max(pos, j + 1);
            }
            pos = Math.Min(pos, finalOrder.Count);
            finalOrder.Insert(pos, item);
            placed.Add(item);
        }

        void PlaceRemaining(ListPropertyValueContext<string> item)
        {
            if (placed.Contains(item)) return;
            var before = GetBeforeItems(active, item);
            foreach (var b in before)
            {
                if (inFinal.Contains(b) || existingRemaining.Contains(b) || newRemaining.Contains(b))
                    PlaceRemaining(b);
            }
            int pos = 0;
            foreach (var b in before)
            {
                int i = finalOrder.FindIndex(x => ReferenceEquals(x, b));
                if (i != -1) pos = Math.Max(pos, i + 1);
            }
            pos = Math.Min(pos, finalOrder.Count);
            finalOrder.Insert(pos, item);
            placed.Add(item);
        }

        for (int i = 0; i < orderFromRecord.Count; i++)
            PlaceDeclared(i);

        foreach (var r in existingRemaining.OrderBy(ConstraintPriority))
            PlaceRemaining(r);

        var sortedNew = new List<(ListPropertyValueContext<string> Item, int RecordIndex)>();
        var toMatch = new List<ListPropertyValueContext<string>>(newRemaining);
        for (int i = 0; i < recordItems.Count; i++)
        {
            var mi = toMatch.FindIndex(x => IsItemEqual(x.Value, recordItems[i]));
            if (mi >= 0) { sortedNew.Add((toMatch[mi], i)); toMatch.RemoveAt(mi); }
        }

        foreach (var (item, recIdx) in sortedNew.OrderBy(x => x.RecordIndex))
        {
            if (placed.Contains(item)) continue;
            int pos = 0;
            for (int j = 0; j < recIdx; j++)
            {
                int k = finalOrder.FindLastIndex(x => IsItemEqual(x.Value, recordItems[j]));
                if (k != -1) pos = Math.Max(pos, k + 1);
            }
            finalOrder.Insert(pos, item);
            placed.Add(item);
            item.OrderOwnerMod = modKey;
        }

        UpdateOrderOwnership(finalOrder, active, modKey);
        var removed = forwardValueContexts.Where(x => x.IsRemoved).ToList();
        forwardValueContexts.Clear();
        forwardValueContexts.AddRange(finalOrder);
        forwardValueContexts.AddRange(removed);
    }

    // -------------------------------------------------------------------------
    // ALTERNATIVE 5: "Insert remaining before each declared" - Interleave.
    // Before adding each declared item, insert any remaining that must go before it.
    // -------------------------------------------------------------------------
    public static void Alt5_InterleavedInsert(
        List<string> recordItems,
        List<ListPropertyValueContext<string>> forwardValueContexts,
        string modKey,
        IReadOnlySet<string> masters)
    {
        var active = forwardValueContexts.Where(i => !i.IsRemoved).ToList();
        var finalOrder = new List<ListPropertyValueContext<string>>();
        var remaining = new List<ListPropertyValueContext<string>>(active);

        foreach (var v in recordItems)
        {
            var m = remaining.FirstOrDefault(x => IsItemEqual(x.Value, v));
            if (m != null && HasPermissionsToModify(modKey, masters, m.OrderOwnerMod))
            {
                // Only insert remaining items we DON'T have permission for (can't reorder)
                var toInsert = remaining
                    .Where(r => r.OrderOwnerMod != null && !HasPermissionsToModify(modKey, masters, r.OrderOwnerMod) && ShouldInsertBefore(r, m, active, finalOrder))
                    .ToList();
                foreach (var r in toInsert.OrderBy(x => active.FindIndex(a => ReferenceEquals(a, x))))
                {
                    var before = GetBeforeItems(active, r);
                    int pos = 0;
                    foreach (var b in before)
                    {
                        int i = finalOrder.FindIndex(x => ReferenceEquals(x, b));
                        if (i != -1) pos = Math.Max(pos, i + 1);
                    }
                    pos = Math.Min(pos, finalOrder.Count);
                    finalOrder.Insert(pos, r);
                    remaining.Remove(r);
                }
                finalOrder.Add(m);
                remaining.Remove(m);
            }
        }

        var existingRemaining = remaining.Where(x => x.OrderOwnerMod != null).ToList();
        var newRemaining = remaining.Where(x => x.OrderOwnerMod == null).ToList();

        foreach (var r in existingRemaining)
        {
            var before = GetBeforeItems(active, r);
            int pos = 0;
            foreach (var b in before)
            {
                int i = finalOrder.FindIndex(x => ReferenceEquals(x, b));
                if (i != -1) pos = Math.Max(pos, i + 1);
            }
            finalOrder.Insert(Math.Min(pos, finalOrder.Count), r);
        }

        var sortedNew = new List<(ListPropertyValueContext<string> Item, int RecordIndex)>();
        var toMatch = new List<ListPropertyValueContext<string>>(newRemaining);
        for (int i = 0; i < recordItems.Count; i++)
        {
            var mi = toMatch.FindIndex(x => IsItemEqual(x.Value, recordItems[i]));
            if (mi >= 0) { sortedNew.Add((toMatch[mi], i)); toMatch.RemoveAt(mi); }
        }

        foreach (var (item, recIdx) in sortedNew)
        {
            int pos = 0;
            for (int j = 0; j < recIdx; j++)
            {
                int k = finalOrder.FindLastIndex(x => IsItemEqual(x.Value, recordItems[j]));
                if (k != -1) pos = Math.Max(pos, k + 1);
            }
            finalOrder.Insert(Math.Min(pos, finalOrder.Count), item);
            item.OrderOwnerMod = modKey;
        }

        UpdateOrderOwnership(finalOrder, active, modKey);
        var removed = forwardValueContexts.Where(x => x.IsRemoved).ToList();
        forwardValueContexts.Clear();
        forwardValueContexts.AddRange(finalOrder);
        forwardValueContexts.AddRange(removed);
    }

    private static bool ShouldInsertBefore(
        ListPropertyValueContext<string> r,
        ListPropertyValueContext<string> declared,
        List<ListPropertyValueContext<string>> active,
        List<ListPropertyValueContext<string>> finalOrder)
    {
        int rIdx = active.FindIndex(x => ReferenceEquals(x, r));
        int dIdx = active.FindIndex(x => ReferenceEquals(x, declared));
        if (rIdx < 0 || dIdx < 0) return false;
        if (rIdx >= dIdx) return false;
        var beforeR = GetBeforeItems(active, r);
        return beforeR.All(b => finalOrder.Any(x => ReferenceEquals(x, b)));
    }
}
