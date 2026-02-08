using ForwardChanges.Contexts;
using Xunit;

namespace ForwardChanges.Tests;

/// <summary>
/// Unit tests for the NEIGHBOR-BASED PARTIAL REORDERING ALGORITHM simulation.
/// Uses string-based simulator and ListPropertyContext&lt;string&gt; / ListPropertyValueContext&lt;string&gt;
/// (same types as production). No real records or Mutagen required.
/// </summary>
public class NeighborBasedReorderingTests
{
    private static List<ListPropertyValueContext<string>> BuildForwardContexts(
        IReadOnlyList<(string Value, string OwnerMod)> items,
        bool setOrderOwnerMod = true)
    {
        var list = new List<ListPropertyValueContext<string>>();
        foreach (var (value, ownerMod) in items)
        {
            var ctx = new ListPropertyValueContext<string>(value, ownerMod);
            if (setOrderOwnerMod)
                ctx.OrderOwnerMod = ownerMod;
            list.Add(ctx);
        }
        return list;
    }

    private static IReadOnlySet<string> Masters(params string[] mods) =>
        mods.ToHashSet();

    private static List<string> ActiveValues(List<ListPropertyValueContext<string>> forward) =>
        forward.Where(i => !i.IsRemoved).Select(i => i.Value ?? "").ToList();

    [Fact]
    public void Sort_Case1_FullReorderWithPermission()
    {
        // Current: A, B. Mod wants: B, A. Has permission for both.
        var forward = BuildForwardContexts([("A", "Mod1"), ("B", "Mod1")]);
        var recordItems = new List<string> { "B", "A" };
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunSortingOnly(recordItems, forward, "Mod2", masters);

        Assert.Equal(["B", "A"], ActiveValues(forward));
    }

    [Fact]
    public void Sort_Case2_PartialReorderMixedPermissions()
    {
        // Current: A, B, C. Mod wants: B, A. Has permission for A,B but not C.
        var forward = BuildForwardContexts([("A", "Mod1"), ("B", "Mod1"), ("C", "OtherMod")]);
        var recordItems = new List<string> { "B", "A", "C" };
        var masters = Masters("Mod1"); // Mod2 has Mod1 as master, not OtherMod

        StringListAlgorithmSimulator.RunSortingOnly(recordItems, forward, "Mod2", masters);

        Assert.Equal(["B", "A", "C"], ActiveValues(forward));
    }

    [Fact]
    public void Sort_Case3_NoPermission()
    {
        // Current: A, B. Mod wants: B, A. No permission for either.
        var forward = BuildForwardContexts([("A", "OtherMod"), ("B", "OtherMod")]);
        var recordItems = new List<string> { "B", "A" };
        var masters = Masters(); // Mod2 has no masters

        StringListAlgorithmSimulator.RunSortingOnly(recordItems, forward, "Mod2", masters);

        Assert.Equal(["A", "B"], ActiveValues(forward));
    }

    [Fact]
    public void Sort_Case4_ComplexPartialReorder()
    {
        // Current: A, B, C, D, E. Mod wants: C, A, E. Has permission for A,C,E but not B,D.
        var forward = BuildForwardContexts([
            ("A", "Mod1"), ("B", "OtherMod"), ("C", "Mod1"), ("D", "OtherMod"), ("E", "Mod1")
        ]);
        var recordItems = new List<string> { "C", "A", "E", "B", "D" };
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunSortingOnly(recordItems, forward, "Mod2", masters);

        Assert.Equal(["C", "A", "B", "D", "E"], ActiveValues(forward));
    }

    [Fact]
    public void Sort_Case5_DuplicatesWithReordering()
    {
        // Current: A, A, B. Mod wants: A, B, A. Has permission for all.
        var forward = BuildForwardContexts([("A", "Mod1"), ("A", "Mod1"), ("B", "Mod1")]);
        var recordItems = new List<string> { "A", "B", "A" };
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunSortingOnly(recordItems, forward, "Mod2", masters);

        Assert.Equal(["A", "B", "A"], ActiveValues(forward));
    }

    [Fact]
    public void Sort_Case6_PureReorderRelativePosition()
    {
        // Current: A, B, C, D. Mod wants: D, A, B, C. Has permission for all.
        var forward = BuildForwardContexts([("A", "Mod1"), ("B", "Mod1"), ("C", "Mod1"), ("D", "Mod1")]);
        var recordItems = new List<string> { "D", "A", "B", "C" };
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunSortingOnly(recordItems, forward, "Mod2", masters);

        Assert.Equal(["D", "A", "B", "C"], ActiveValues(forward));
    }

    [Fact]
    public void Sort_Case7_PartialPermissionReorder()
    {
        // Current: A, B, C, D. Mod wants: D, A, B, C. Has permission for A,B,C but not D.
        var forward = BuildForwardContexts([
            ("A", "Mod1"), ("B", "Mod1"), ("C", "Mod1"), ("D", "OtherMod")
        ]);
        var recordItems = new List<string> { "D", "A", "B", "C" };
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunSortingOnly(recordItems, forward, "Mod2", masters);

        Assert.Equal(["A", "B", "C", "D"], ActiveValues(forward));
    }

    // --- Duplicate-value variants of sorting cases ---

    [Fact]
    public void Sort_Case1_Duplicates_FullReorderWithPermission()
    {
        // Current: A, A, B, B. Mod wants: B, B, A, A. Has permission for all.
        var forward = BuildForwardContexts([("A", "Mod1"), ("A", "Mod1"), ("B", "Mod1"), ("B", "Mod1")]);
        var recordItems = new List<string> { "B", "B", "A", "A" };
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunSortingOnly(recordItems, forward, "Mod2", masters);

        Assert.Equal(["B", "B", "A", "A"], ActiveValues(forward));
    }

    [Fact]
    public void Sort_Case2_Duplicates_PartialReorderMixedPermissions()
    {
        // Current: A, A, B, C. A from Mod1, B,C from OtherMod. Mod wants: A, B, A, C.
        var forward = BuildForwardContexts([("A", "Mod1"), ("A", "Mod1"), ("B", "OtherMod"), ("C", "OtherMod")]);
        var recordItems = new List<string> { "A", "B", "A", "C" };
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunSortingOnly(recordItems, forward, "Mod2", masters);

        Assert.Equal(["A", "A", "B", "C"], ActiveValues(forward));
    }

    [Fact]
    public void Sort_Case3_Duplicates_NoPermission()
    {
        // Current: A, A, B. All OtherMod. Mod wants: B, A, A. No permission.
        var forward = BuildForwardContexts([("A", "OtherMod"), ("A", "OtherMod"), ("B", "OtherMod")]);
        var recordItems = new List<string> { "B", "A", "A" };
        var masters = Masters();

        StringListAlgorithmSimulator.RunSortingOnly(recordItems, forward, "Mod2", masters);

        Assert.Equal(["A", "A", "B"], ActiveValues(forward));
    }

    [Fact]
    public void Sort_Case4_Duplicates_ComplexPartialReorder()
    {
        // Current: A, A, B, C, D, E (6 items). A,C,E from Mod1, B,D from OtherMod. Record declares: C, A, E, B, D (5).
        // One A stays in remaining; placed by before-relationships. Expected: C, A, A, B, D, E.
        var forward = BuildForwardContexts([
            ("A", "Mod1"), ("A", "Mod1"), ("B", "OtherMod"), ("C", "Mod1"), ("D", "OtherMod"), ("E", "Mod1")
        ]);
        var recordItems = new List<string> { "C", "A", "E", "B", "D" };
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunSortingOnly(recordItems, forward, "Mod2", masters);

        Assert.Equal(["C", "A", "A", "B", "D", "E"], ActiveValues(forward));
    }

    [Fact]
    public void Sort_Case6_Duplicates_PureReorderRelativePosition()
    {
        // Current: A, A, B, B. Mod wants: B, B, A, A. Has permission for all.
        var forward = BuildForwardContexts([("A", "Mod1"), ("A", "Mod1"), ("B", "Mod1"), ("B", "Mod1")]);
        var recordItems = new List<string> { "B", "B", "A", "A" };
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunSortingOnly(recordItems, forward, "Mod2", masters);

        Assert.Equal(["B", "B", "A", "A"], ActiveValues(forward));
    }

    [Fact]
    public void Sort_Case7_Duplicates_PartialPermissionReorder()
    {
        // Current: A, A, B, C, D. A,B,C from Mod1, D from OtherMod. Mod wants: D, A, B, C.
        var forward = BuildForwardContexts([
            ("A", "Mod1"), ("A", "Mod1"), ("B", "Mod1"), ("C", "Mod1"), ("D", "OtherMod")
        ]);
        var recordItems = new List<string> { "D", "A", "B", "C" };
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunSortingOnly(recordItems, forward, "Mod2", masters);

        Assert.Equal(["A", "A", "B", "C", "D"], ActiveValues(forward));
    }

    [Fact]
    public void Sort_Duplicates_PartialPermission_OnlySomeInstancesReorderable()
    {
        // Current: A, A, B, B. A from Mod1, B from OtherMod. Mod wants: B, A, B, A.
        var forward = BuildForwardContexts([("A", "Mod1"), ("A", "Mod1"), ("B", "OtherMod"), ("B", "OtherMod")]);
        var recordItems = new List<string> { "B", "A", "B", "A" };
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunSortingOnly(recordItems, forward, "Mod2", masters);

        Assert.Equal(["A", "A", "B", "B"], ActiveValues(forward));
    }

    [Fact]
    public void Removals_ExcessItems_RemovesFromModWeCanModify()
    {
        var forward = BuildForwardContexts([("A", "Mod1"), ("A", "Mod1"), ("A", "Mod1")]);
        var recordItems = new List<string> { "A" }; // record now has only one A
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunFullUpdate(recordItems, forward, "Mod2", masters);

        var active = forward.Where(i => !i.IsRemoved).ToList();
        Assert.Single(active);
        Assert.Equal("A", active[0].Value);
        Assert.Equal(2, forward.Count(i => i.IsRemoved));
    }

    [Fact]
    public void Removals_ExcessItems_Duplicates_RecordHasTwoForwardHasThree()
    {
        var forward = BuildForwardContexts([("A", "Mod1"), ("A", "Mod1"), ("A", "Mod1")]);
        var recordItems = new List<string> { "A", "A" };
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunFullUpdate(recordItems, forward, "Mod2", masters);

        Assert.Equal(2, forward.Count(i => !i.IsRemoved));
        Assert.Equal(1, forward.Count(i => i.IsRemoved));
        Assert.Equal(["A", "A"], ActiveValues(forward));
    }

    [Fact]
    public void Removals_ExcessItems_Duplicates_MixedOwners_RemovesFromModWeCanModify()
    {
        // Forward: A(Mod1), A(Mod2), A(Mod1). Record: A, A. Remove one (backwards: prefer last Mod1).
        var forward = BuildForwardContexts([("A", "Mod1"), ("A", "Mod2"), ("A", "Mod1")]);
        var recordItems = new List<string> { "A", "A" };
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunFullUpdate(recordItems, forward, "Mod2", masters);

        Assert.Equal(2, forward.Count(i => !i.IsRemoved));
        Assert.Equal(1, forward.Count(i => i.IsRemoved));
        Assert.Equal(["A", "A"], ActiveValues(forward));
    }

    [Fact]
    public void Removals_ItemNotInRecord_RemovesWhenWeHavePermission()
    {
        var forward = BuildForwardContexts([("A", "Mod1"), ("B", "Mod1")]);
        var recordItems = new List<string> { "A" }; // B removed from record
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunFullUpdate(recordItems, forward, "Mod2", masters);

        Assert.Single(forward, i => !i.IsRemoved);
        Assert.Equal("A", forward.First(i => !i.IsRemoved).Value);
    }

    [Fact]
    public void Removals_ItemNotInRecord_Duplicates_RemovesAllInstancesWhenWeHavePermission()
    {
        var forward = BuildForwardContexts([("A", "Mod1"), ("A", "Mod1"), ("B", "Mod1"), ("B", "Mod1")]);
        var recordItems = new List<string> { "A", "A" };
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunFullUpdate(recordItems, forward, "Mod2", masters);

        Assert.Equal(2, forward.Count(i => !i.IsRemoved));
        Assert.Equal(["A", "A"], ActiveValues(forward));
        Assert.Equal(2, forward.Count(i => i.IsRemoved));
    }

    [Fact]
    public void Removals_NoPermission_DoesNotRemove()
    {
        var forward = BuildForwardContexts([("A", "OtherMod"), ("B", "OtherMod")]);
        var recordItems = new List<string> { "A" };
        var masters = Masters();

        StringListAlgorithmSimulator.RunFullUpdate(recordItems, forward, "Mod2", masters);

        Assert.Equal(2, forward.Count(i => !i.IsRemoved));
    }

    [Fact]
    public void Removals_NoPermission_Duplicates_DoesNotRemoveItemNotInRecord()
    {
        var forward = BuildForwardContexts([("A", "OtherMod"), ("A", "OtherMod"), ("B", "OtherMod")]);
        var recordItems = new List<string> { "A", "A" };
        var masters = Masters();

        StringListAlgorithmSimulator.RunFullUpdate(recordItems, forward, "Mod2", masters);

        Assert.Equal(3, forward.Count(i => !i.IsRemoved));
        Assert.Equal(["A", "A", "B"], ActiveValues(forward));
    }

    [Fact]
    public void Additions_NewItem_AddsToForward()
    {
        var forward = BuildForwardContexts([("A", "Mod1")]);
        var recordItems = new List<string> { "A", "B" };
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunFullUpdate(recordItems, forward, "Mod2", masters);

        Assert.Equal(["A", "B"], ActiveValues(forward));
        var bCtx = forward.First(i => !i.IsRemoved && i.Value == "B");
        Assert.Equal("Mod2", bCtx.OwnerMod);
    }

    [Fact]
    public void Additions_NewItem_Duplicates_AddsMultipleInstances()
    {
        var forward = BuildForwardContexts([("A", "Mod1")]);
        var recordItems = new List<string> { "A", "B", "B" };
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunFullUpdate(recordItems, forward, "Mod2", masters);

        Assert.Equal(["A", "B", "B"], ActiveValues(forward));
        Assert.Equal(2, forward.Count(i => !i.IsRemoved && i.Value == "B"));
    }

    [Fact]
    public void Additions_Unremove_PrioritizedOverNew()
    {
        var forward = BuildForwardContexts([("A", "Mod1"), ("B", "Mod1")]);
        forward[1].IsRemoved = true;
        var recordItems = new List<string> { "A", "B" };
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunFullUpdate(recordItems, forward, "Mod2", masters);

        Assert.Equal(2, forward.Count(i => !i.IsRemoved));
        Assert.Equal(["A", "B"], ActiveValues(forward));
    }

    [Fact]
    public void Additions_Unremove_Duplicates_OneUnremoveOneNewWhenTwoNeeded()
    {
        // Forward: A, A, B (second B removed). Record: A, A, B. Need 2 B's: un-remove one, add one.
        var forward = BuildForwardContexts([("A", "Mod1"), ("A", "Mod1"), ("B", "Mod1"), ("B", "Mod1")]);
        forward[3].IsRemoved = true;
        var recordItems = new List<string> { "A", "A", "B", "B" };
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunFullUpdate(recordItems, forward, "Mod2", masters);

        Assert.Equal(4, forward.Count(i => !i.IsRemoved));
        Assert.Equal(["A", "A", "B", "B"], ActiveValues(forward));
    }

    [Fact]
    public void FullUpdate_RemovalsThenAdditionsThenSort_WithDuplicates()
    {
        // Forward: A, A. Record: A, A, B, B. Removals: none. Additions: add B, B. Sort: new B,B after A,A.
        var forward = BuildForwardContexts([("A", "Mod1"), ("A", "Mod1")]);
        var recordItems = new List<string> { "A", "A", "B", "B" };
        var masters = Masters("Mod1");

        StringListAlgorithmSimulator.RunFullUpdate(recordItems, forward, "Mod2", masters);

        Assert.Equal(["A", "A", "B", "B"], ActiveValues(forward));
    }

    [Fact]
    public void IPropertyContext_GetForwardValue_WorksWithListContext()
    {
        var forward = BuildForwardContexts([("X", "Mod1"), ("Y", "Mod1")]);
        var context = new ListPropertyContext<string>(null, forward);
        var value = context.GetForwardValue();
        Assert.NotNull(value);
        var list = (System.Collections.IList)value!;
        Assert.Equal(2, list.Count);
        Assert.Equal("X", list[0]);
        Assert.Equal("Y", list[1]);
    }

    [Fact]
    public void IPropertyValueContext_OwnerMod_OnStringContext()
    {
        var ctx = new ListPropertyValueContext<string>("test", "MyMod.esp");
        Assert.Equal("MyMod.esp", ctx.OwnerMod);
        Assert.Equal("test", ctx.Value);
    }
}
