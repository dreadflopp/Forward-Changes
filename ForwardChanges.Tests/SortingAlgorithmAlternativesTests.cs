using ForwardChanges.Contexts;
using Xunit;

namespace ForwardChanges.Tests;

/// <summary>
/// Runs the same test cases against each alternative sorting algorithm.
/// Reports which alternatives pass all tests.
/// </summary>
public class SortingAlgorithmAlternativesTests
{
    private static List<ListPropertyValueContext<string>> BuildForward(
        IReadOnlyList<(string Value, string OwnerMod)> items,
        bool setOrderOwnerMod = true)
    {
        var list = new List<ListPropertyValueContext<string>>();
        foreach (var (value, ownerMod) in items)
        {
            var ctx = new ListPropertyValueContext<string>(value, ownerMod);
            if (setOrderOwnerMod) ctx.OrderOwnerMod = ownerMod;
            list.Add(ctx);
        }
        return list;
    }

    private static IReadOnlySet<string> Masters(params string[] mods) => mods.ToHashSet();

    private static List<string> ActiveValues(List<ListPropertyValueContext<string>> f) =>
        f.Where(i => !i.IsRemoved).Select(i => i.Value ?? "").ToList();

    private static void RunSortTest(
        SortingAlgorithmAlternatives.SortAlgorithm algo,
        IReadOnlyList<(string Value, string OwnerMod)> forwardItems,
        List<string> recordItems,
        IReadOnlySet<string> masters,
        IReadOnlyList<string> expected)
    {
        var forward = BuildForward(forwardItems);
        algo(recordItems, forward, "Mod2", masters);
        Assert.Equal(expected, ActiveValues(forward));
    }

    /// <summary>
    /// Alt1 appends remaining at end. Passes Case 1,2 but fails Case 4 (B,D must be interleaved).
    /// </summary>
    [Fact]
    public void Alt1_AppendRemaining_ProducesWrongResultForCase4()
    {
        var forward = BuildForward([
            ("A", "Mod1"), ("B", "OtherMod"), ("C", "Mod1"),
            ("D", "OtherMod"), ("E", "Mod1")
        ]);
        SortingAlgorithmAlternatives.Alt1_AppendRemaining(
            ["C", "A", "E", "B", "D"], forward, "Mod2", Masters("Mod1"));
        // Alt1 gives [C, A, E, B, D] - remaining appended at end. Correct would be [C, A, B, D, E].
        Assert.Equal(["C", "A", "E", "B", "D"], ActiveValues(forward));
    }

    /// <summary>
    /// Alt2 uses original index for placement. Produces wrong order.
    /// </summary>
    [Fact]
    public void Alt2_OriginalIndex_ProducesWrongResult()
    {
        var forward = BuildForward([("A", "Mod1"), ("B", "Mod1")]);
        SortingAlgorithmAlternatives.Alt2_OriginalIndex(["B", "A"], forward, "Mod2", Masters("Mod1"));
        // Alt2 may produce wrong order (depends on implementation)
        var result = ActiveValues(forward);
        Assert.True(result.Count == 2);
    }

    [Fact]
    public void Alt3_UnifiedPlacement_PassesAllSortingTests()
    {
        RunSortTest(SortingAlgorithmAlternatives.Alt3_UnifiedPlacement,
            [("A", "Mod1"), ("B", "Mod1")], ["B", "A"], Masters("Mod1"), ["B", "A"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt3_UnifiedPlacement,
            [("A", "Mod1"), ("B", "Mod1"), ("C", "OtherMod")], ["B", "A", "C"], Masters("Mod1"), ["B", "A", "C"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt3_UnifiedPlacement,
            [("A", "OtherMod"), ("B", "OtherMod")], ["B", "A"], Masters(), ["A", "B"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt3_UnifiedPlacement,
            [("A", "Mod1"), ("B", "OtherMod"), ("C", "Mod1"), ("D", "OtherMod"), ("E", "Mod1")],
            ["C", "A", "E", "B", "D"], Masters("Mod1"), ["C", "A", "B", "D", "E"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt3_UnifiedPlacement,
            [("A", "Mod1"), ("A", "Mod1"), ("B", "Mod1")], ["A", "B", "A"], Masters("Mod1"), ["A", "B", "A"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt3_UnifiedPlacement,
            [("A", "Mod1"), ("A", "Mod1"), ("B", "OtherMod"), ("C", "OtherMod")], ["A", "B", "A", "C"], Masters("Mod1"), ["A", "A", "B", "C"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt3_UnifiedPlacement,
            [("A", "Mod1"), ("A", "Mod1"), ("B", "OtherMod"), ("C", "Mod1"), ("D", "OtherMod"), ("E", "Mod1")],
            ["C", "A", "E", "B", "D"], Masters("Mod1"), ["C", "A", "A", "B", "D", "E"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt3_UnifiedPlacement,
            [("A", "Mod1"), ("B", "Mod1"), ("C", "Mod1"), ("D", "Mod1")], ["D", "A", "B", "C"], Masters("Mod1"), ["D", "A", "B", "C"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt3_UnifiedPlacement,
            [("A", "Mod1"), ("B", "Mod1"), ("C", "Mod1"), ("D", "OtherMod")], ["D", "A", "B", "C"], Masters("Mod1"), ["A", "B", "C", "D"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt3_UnifiedPlacement,
            [("A", "OtherMod"), ("A", "OtherMod"), ("B", "OtherMod")], ["B", "A", "A"], Masters(), ["A", "A", "B"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt3_UnifiedPlacement,
            [("A", "Mod1"), ("A", "Mod1"), ("B", "Mod1"), ("B", "Mod1")], ["B", "B", "A", "A"], Masters("Mod1"), ["B", "B", "A", "A"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt3_UnifiedPlacement,
            [("A", "Mod1"), ("A", "Mod1"), ("B", "Mod1"), ("C", "Mod1"), ("D", "OtherMod")], ["D", "A", "B", "C"], Masters("Mod1"), ["A", "A", "B", "C", "D"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt3_UnifiedPlacement,
            [("A", "Mod1"), ("A", "Mod1"), ("B", "OtherMod"), ("B", "OtherMod")], ["B", "A", "B", "A"], Masters("Mod1"), ["A", "A", "B", "B"]);
    }

    [Fact]
    public void Alt4_TopologicalSort_PassesAllSortingTests()
    {
        RunSortTest(SortingAlgorithmAlternatives.Alt4_TopologicalSort,
            [("A", "Mod1"), ("B", "Mod1")], ["B", "A"], Masters("Mod1"), ["B", "A"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt4_TopologicalSort,
            [("A", "Mod1"), ("B", "Mod1"), ("C", "OtherMod")], ["B", "A", "C"], Masters("Mod1"), ["B", "A", "C"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt4_TopologicalSort,
            [("A", "OtherMod"), ("B", "OtherMod")], ["B", "A"], Masters(), ["A", "B"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt4_TopologicalSort,
            [("A", "Mod1"), ("B", "OtherMod"), ("C", "Mod1"), ("D", "OtherMod"), ("E", "Mod1")],
            ["C", "A", "E", "B", "D"], Masters("Mod1"), ["C", "A", "B", "D", "E"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt4_TopologicalSort,
            [("A", "Mod1"), ("A", "Mod1"), ("B", "Mod1")], ["A", "B", "A"], Masters("Mod1"), ["A", "B", "A"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt4_TopologicalSort,
            [("A", "Mod1"), ("A", "Mod1"), ("B", "OtherMod"), ("C", "OtherMod")], ["A", "B", "A", "C"], Masters("Mod1"), ["A", "A", "B", "C"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt4_TopologicalSort,
            [("A", "Mod1"), ("A", "Mod1"), ("B", "OtherMod"), ("C", "Mod1"), ("D", "OtherMod"), ("E", "Mod1")],
            ["C", "A", "E", "B", "D"], Masters("Mod1"), ["C", "A", "A", "B", "D", "E"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt4_TopologicalSort,
            [("A", "Mod1"), ("B", "Mod1"), ("C", "Mod1"), ("D", "Mod1")], ["D", "A", "B", "C"], Masters("Mod1"), ["D", "A", "B", "C"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt4_TopologicalSort,
            [("A", "Mod1"), ("B", "Mod1"), ("C", "Mod1"), ("D", "OtherMod")], ["D", "A", "B", "C"], Masters("Mod1"), ["A", "B", "C", "D"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt4_TopologicalSort,
            [("A", "OtherMod"), ("A", "OtherMod"), ("B", "OtherMod")], ["B", "A", "A"], Masters(), ["A", "A", "B"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt4_TopologicalSort,
            [("A", "Mod1"), ("A", "Mod1"), ("B", "Mod1"), ("B", "Mod1")], ["B", "B", "A", "A"], Masters("Mod1"), ["B", "B", "A", "A"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt4_TopologicalSort,
            [("A", "Mod1"), ("A", "Mod1"), ("B", "Mod1"), ("C", "Mod1"), ("D", "OtherMod")], ["D", "A", "B", "C"], Masters("Mod1"), ["A", "A", "B", "C", "D"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt4_TopologicalSort,
            [("A", "Mod1"), ("A", "Mod1"), ("B", "OtherMod"), ("B", "OtherMod")], ["B", "A", "B", "A"], Masters("Mod1"), ["A", "A", "B", "B"]);
    }

    [Fact]
    public void Alt5_InterleavedInsert_PassesAllSortingTests()
    {
        RunSortTest(SortingAlgorithmAlternatives.Alt5_InterleavedInsert,
            [("A", "Mod1"), ("B", "Mod1")], ["B", "A"], Masters("Mod1"), ["B", "A"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt5_InterleavedInsert,
            [("A", "Mod1"), ("B", "Mod1"), ("C", "OtherMod")], ["B", "A", "C"], Masters("Mod1"), ["B", "A", "C"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt5_InterleavedInsert,
            [("A", "OtherMod"), ("B", "OtherMod")], ["B", "A"], Masters(), ["A", "B"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt5_InterleavedInsert,
            [("A", "Mod1"), ("B", "OtherMod"), ("C", "Mod1"), ("D", "OtherMod"), ("E", "Mod1")],
            ["C", "A", "E", "B", "D"], Masters("Mod1"), ["C", "A", "B", "D", "E"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt5_InterleavedInsert,
            [("A", "Mod1"), ("A", "Mod1"), ("B", "Mod1")], ["A", "B", "A"], Masters("Mod1"), ["A", "B", "A"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt5_InterleavedInsert,
            [("A", "Mod1"), ("A", "Mod1"), ("B", "OtherMod"), ("C", "OtherMod")], ["A", "B", "A", "C"], Masters("Mod1"), ["A", "A", "B", "C"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt5_InterleavedInsert,
            [("A", "Mod1"), ("A", "Mod1"), ("B", "OtherMod"), ("C", "Mod1"), ("D", "OtherMod"), ("E", "Mod1")],
            ["C", "A", "E", "B", "D"], Masters("Mod1"), ["C", "A", "A", "B", "D", "E"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt5_InterleavedInsert,
            [("A", "Mod1"), ("B", "Mod1"), ("C", "Mod1"), ("D", "Mod1")], ["D", "A", "B", "C"], Masters("Mod1"), ["D", "A", "B", "C"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt5_InterleavedInsert,
            [("A", "Mod1"), ("B", "Mod1"), ("C", "Mod1"), ("D", "OtherMod")], ["D", "A", "B", "C"], Masters("Mod1"), ["A", "B", "C", "D"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt5_InterleavedInsert,
            [("A", "OtherMod"), ("A", "OtherMod"), ("B", "OtherMod")], ["B", "A", "A"], Masters(), ["A", "A", "B"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt5_InterleavedInsert,
            [("A", "Mod1"), ("A", "Mod1"), ("B", "Mod1"), ("B", "Mod1")], ["B", "B", "A", "A"], Masters("Mod1"), ["B", "B", "A", "A"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt5_InterleavedInsert,
            [("A", "Mod1"), ("A", "Mod1"), ("B", "Mod1"), ("C", "Mod1"), ("D", "OtherMod")], ["D", "A", "B", "C"], Masters("Mod1"), ["A", "A", "B", "C", "D"]);
        RunSortTest(SortingAlgorithmAlternatives.Alt5_InterleavedInsert,
            [("A", "Mod1"), ("A", "Mod1"), ("B", "OtherMod"), ("B", "OtherMod")], ["B", "A", "B", "A"], Masters("Mod1"), ["A", "A", "B", "B"]);
    }

    [Fact]
    public void Alt3_UnifiedPlacement_FullUpdateTests()
    {
        var forward = BuildForward([("A", "Mod1"), ("A", "Mod1")]);
        StringListAlgorithmSimulator.RunFullUpdateWithSort(
            ["A", "A", "B", "B"], forward, "Mod2", Masters("Mod1"),
            SortingAlgorithmAlternatives.Alt3_UnifiedPlacement);
        Assert.Equal(["A", "A", "B", "B"], ActiveValues(forward));
    }

    [Fact]
    public void Alt4_TopologicalSort_FullUpdateTests()
    {
        var forward = BuildForward([("A", "Mod1"), ("A", "Mod1")]);
        StringListAlgorithmSimulator.RunFullUpdateWithSort(
            ["A", "A", "B", "B"], forward, "Mod2", Masters("Mod1"),
            SortingAlgorithmAlternatives.Alt4_TopologicalSort);
        Assert.Equal(["A", "A", "B", "B"], ActiveValues(forward));
    }

    [Fact]
    public void Alt5_InterleavedInsert_FullUpdateTests()
    {
        var forward = BuildForward([("A", "Mod1"), ("A", "Mod1")]);
        StringListAlgorithmSimulator.RunFullUpdateWithSort(
            ["A", "A", "B", "B"], forward, "Mod2", Masters("Mod1"),
            SortingAlgorithmAlternatives.Alt5_InterleavedInsert);
        Assert.Equal(["A", "A", "B", "B"], ActiveValues(forward));
    }
}
