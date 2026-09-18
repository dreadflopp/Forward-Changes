using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class XEditListAlignmentTests
{
    [Fact]
    public void CurrentXEditDiffTieBreakPlacesOldRowBeforeNewRow()
    {
        var nextRowId = 3;
        var rows = Rows("A", "B", "C");

        var secondColumn = XEditSequenceAligner.Align(
            rows,
            ["B", "C"],
            StringEquals,
            () => nextRowId++);
        var thirdColumn = XEditSequenceAligner.Align(
            secondColumn.Rows,
            ["B", "A"],
            StringEquals,
            () => nextRowId++);

        // xEdit's release-note illustration places the new A before the old C.
        // The current TDiff source resolves this ambiguous move as delete C,
        // then add A, so source-compatible alignment produces A, B, C, A.
        Assert.Equal(["A", "B", "C", "A"], Representatives(thirdColumn.Rows));
        Assert.Equal([1, 3], thirdColumn.RightRowIds);
    }

    [Fact]
    public void ComparisonChangesReuseConditionLikeRows()
    {
        var nextRowId = 3;
        var rows = Rows(
            new TestValue("x", "x<=5"),
            new TestValue("y", "y>=60"),
            new TestValue("z", "z<=200"));

        var secondColumn = XEditSequenceAligner.Align(
            rows,
            [
                new TestValue("x", "x<=5"),
                new TestValue("y", "y>=60"),
                new TestValue("z", "z<200")
            ],
            AlignmentEquals,
            () => nextRowId++);
        var thirdColumn = XEditSequenceAligner.Align(
            secondColumn.Rows,
            [new TestValue("x", "x=-1")],
            AlignmentEquals,
            () => nextRowId++);

        Assert.Equal(["x", "y", "z"], Keys(thirdColumn.Rows));
        Assert.Equal([0], thirdColumn.RightRowIds);
    }

    [Fact]
    public void LoadScreenMergeEmitsXBeforeRetainedZ()
    {
        var x = Item(new TestValue("x", "x=-1"), 0, CurrentModKey.ToString());
        var z = Item(new TestValue("z", "z<200"), 2, OtherModKey.ToString());
        var context = new ListPropertyContext<TestValue>
        {
            AlignmentRows = Rows(
                new TestValue("x", "x<=5"),
                new TestValue("y", "y>=60"),
                new TestValue("z", "z<=200")).ToList(),
            NextAlignmentRowId = 3,
            ForwardValueContexts = [z, x]
        };
        var forward = context.ForwardValueContexts!;

        Handler.Reconcile(
            [new TestValue("x", "x=-1")],
            [0],
            context,
            forward);

        Assert.Equal(["x=-1", "z<200"], ActiveValues(forward));
    }

    [Fact]
    public void SameAlignedRowReplacesChangedContentInsteadOfAddingDuplicate()
    {
        var priorValue = new TestValue("x", "x>1");
        var context = new ListPropertyContext<TestValue>
        {
            AlignmentRows = Rows(priorValue).ToList(),
            NextAlignmentRowId = 1,
            OriginalValueContexts = [],
            ForwardValueContexts = [Item(priorValue, 0, OtherModKey.ToString())]
        };

        Handler.Reconcile(
            [new TestValue("x", "x>=1")],
            [0],
            context,
            context.ForwardValueContexts);

        Assert.Equal(["x>=1"], ActiveValues(context.ForwardValueContexts));
        Assert.Equal(CurrentModKey.ToString(), context.ForwardValueContexts.Single().OwnerMod);
    }

    [Fact]
    public void SuccessiveColumnsReplaceDataWithinTheSameAlignedRow()
    {
        var context = new ListPropertyContext<TestValue>
        {
            OriginalValueContexts = [],
            ForwardValueContexts = []
        };

        Handler.ProcessColumn(
            OtherModKey,
            [new TestValue("x", "x>1")],
            context);
        Handler.ProcessColumn(
            CurrentModKey,
            [new TestValue("x", "x>=1")],
            context);

        Assert.Single(context.AlignmentRows);
        Assert.Equal(["x>=1"], ActiveValues(context.ForwardValueContexts));
        Assert.Equal(CurrentModKey.ToString(), context.ForwardValueContexts.Single().OwnerMod);
    }

    [Fact]
    public void DuplicateAlignmentKeysReplaceTheCorrectOccurrence()
    {
        var first = new TestValue("x", "x>1");
        var second = new TestValue("x", "x>2");
        var context = new ListPropertyContext<TestValue>
        {
            AlignmentRows = Rows(first, second).ToList(),
            NextAlignmentRowId = 2,
            OriginalValueContexts = [],
            ForwardValueContexts =
            [
                Item(first, 0, OtherModKey.ToString()),
                Item(second, 1, OtherModKey.ToString())
            ]
        };

        Handler.Reconcile(
            [first, new TestValue("x", "x>=2")],
            [0, 1],
            context,
            context.ForwardValueContexts);

        Assert.Equal(["x>1", "x>=2"], ActiveValues(context.ForwardValueContexts));
        Assert.Equal([0, 1], context.ForwardValueContexts
            .Where(item => !item.IsRemoved)
            .Select(item => item.AlignmentRowId));
    }

    [Fact]
    public void ReversionToOriginalAlignedValueRequiresPermission()
    {
        var originalValue = new TestValue("x", "x>1");
        var changedValue = new TestValue("x", "x>=1");
        var originalItem = Item(originalValue, 0, OriginalModKey.ToString());
        var context = new ListPropertyContext<TestValue>
        {
            AlignmentRows = Rows(originalValue).ToList(),
            NextAlignmentRowId = 1,
            OriginalValueContexts = [originalItem],
            ForwardValueContexts = [Item(changedValue, 0, OtherModKey.ToString())]
        };

        Handler.Reconcile(
            [originalValue],
            [0],
            context,
            context.ForwardValueContexts);

        Assert.Equal(["x>=1"], ActiveValues(context.ForwardValueContexts));
        Assert.Equal(OtherModKey.ToString(), context.ForwardValueContexts.Single().OwnerMod);
    }

    [Fact]
    public void ReversionToOriginalAlignedValueSucceedsForOwner()
    {
        var originalValue = new TestValue("x", "x>1");
        var changedValue = new TestValue("x", "x>=1");
        var context = new ListPropertyContext<TestValue>
        {
            AlignmentRows = Rows(originalValue).ToList(),
            NextAlignmentRowId = 1,
            OriginalValueContexts = [Item(originalValue, 0, OriginalModKey.ToString())],
            ForwardValueContexts = [Item(changedValue, 0, CurrentModKey.ToString())]
        };

        Handler.Reconcile(
            [originalValue],
            [0],
            context,
            context.ForwardValueContexts);

        Assert.Equal(["x>1"], ActiveValues(context.ForwardValueContexts));
        Assert.Equal(CurrentModKey.ToString(), context.ForwardValueContexts.Single().OwnerMod);
    }

    [Fact]
    public void ProtectedMoveDoesNotCreateDuplicateItem()
    {
        var value = new TestValue("A", "A");
        var context = new ListPropertyContext<TestValue>
        {
            AlignmentRows =
            [
                new ListAlignmentRow<TestValue>(0, value),
                new ListAlignmentRow<TestValue>(1, value)
            ],
            NextAlignmentRowId = 2,
            OriginalValueContexts = [],
            ForwardValueContexts = [Item(value, 0, OtherModKey.ToString())]
        };

        Handler.Reconcile(
            [value],
            [1],
            context,
            context.ForwardValueContexts);

        var activeItems = context.ForwardValueContexts.Where(item => !item.IsRemoved).ToList();
        Assert.Single(activeItems);
        Assert.Equal(0, activeItems[0].AlignmentRowId);
    }

    [Fact]
    public void ProtectedRemovedItemIsNotRestoredOrDuplicated()
    {
        var value = new TestValue("A", "A");
        var removedItem = Item(value, 0, OtherModKey.ToString());
        removedItem.IsRemoved = true;
        var context = new ListPropertyContext<TestValue>
        {
            AlignmentRows = Rows(value).ToList(),
            NextAlignmentRowId = 1,
            OriginalValueContexts = [],
            ForwardValueContexts = [removedItem]
        };

        Handler.Reconcile([value], [0], context, context.ForwardValueContexts);

        Assert.DoesNotContain(context.ForwardValueContexts, item => !item.IsRemoved);
        Assert.Single(context.ForwardValueContexts);
    }

    [Fact]
    public void RemovedItemIsRestoredWhenCurrentModOwnsRemoval()
    {
        var value = new TestValue("A", "A");
        var removedItem = Item(value, 0, CurrentModKey.ToString());
        removedItem.IsRemoved = true;
        var context = new ListPropertyContext<TestValue>
        {
            AlignmentRows = Rows(value).ToList(),
            NextAlignmentRowId = 1,
            OriginalValueContexts = [],
            ForwardValueContexts = [removedItem]
        };

        Handler.Reconcile([value], [0], context, context.ForwardValueContexts);

        var restoredItem = Assert.Single(
            context.ForwardValueContexts,
            item => !item.IsRemoved);
        Assert.Equal(0, restoredItem.AlignmentRowId);
        Assert.Equal(CurrentModKey.ToString(), restoredItem.OwnerMod);
    }

    [Fact]
    public void IndependentlyChangedValueCanOccupyAProtectedRemovedRow()
    {
        var removedValue = new TestValue("x", "x>1");
        var changedValue = new TestValue("x", "x>=1");
        var removedItem = Item(removedValue, 0, OtherModKey.ToString());
        removedItem.IsRemoved = true;
        var context = new ListPropertyContext<TestValue>
        {
            AlignmentRows = Rows(removedValue).ToList(),
            NextAlignmentRowId = 1,
            OriginalValueContexts = [],
            ForwardValueContexts = [removedItem]
        };

        Handler.Reconcile([changedValue], [0], context, context.ForwardValueContexts);

        Assert.Equal(["x>=1"], ActiveValues(context.ForwardValueContexts));
        Assert.Equal(2, context.ForwardValueContexts.Count);
        Assert.Single(context.ForwardValueContexts, item => item.IsRemoved);
    }

    [Fact]
    public void NewRowsAreInsertedBetweenSurvivingNeighbors()
    {
        var nextRowId = 2;

        var result = XEditSequenceAligner.Align(
            Rows("A", "B"),
            ["A", "C", "B"],
            StringEquals,
            () => nextRowId++);

        Assert.Equal(["A", "C", "B"], Representatives(result.Rows));
        Assert.Equal([0, 2, 1], result.RightRowIds);
    }

    [Fact]
    public void DuplicateKeysRemainSeparateOccurrences()
    {
        var nextRowId = 3;

        var result = XEditSequenceAligner.Align(
            Rows("A", "A", "B"),
            ["A", "C", "A", "B"],
            StringEquals,
            () => nextRowId++);

        Assert.Equal(["A", "C", "A", "B"], Representatives(result.Rows));
        Assert.Equal([0, 3, 1, 2], result.RightRowIds);
    }

    [Fact]
    public void AlignedOrderedEqualityDetectsOrderOnlyChanges()
    {
        Assert.False(Handler.AreValuesEqual(
            [new TestValue("A", "A"), new TestValue("B", "B")],
            [new TestValue("B", "B"), new TestValue("A", "A")]));
    }

    private static readonly ModKey CurrentModKey = new("Current.esp", ModType.Plugin);
    private static readonly ModKey OtherModKey = new("Other.esp", ModType.Plugin);
    private static readonly ModKey OriginalModKey = new("Original.esm", ModType.Master);
    private static readonly TestListHandler Handler = new();

    private static ListAlignmentRow<string>[] Rows(params string[] values)
        => values.Select((value, index) => new ListAlignmentRow<string>(index, value)).ToArray();

    private static ListAlignmentRow<TestValue>[] Rows(params TestValue[] values)
        => values.Select((value, index) => new ListAlignmentRow<TestValue>(index, value)).ToArray();

    private static ListPropertyValueContext<TestValue> Item(
        TestValue value,
        int rowId,
        string alignmentOwner)
        => new(value, alignmentOwner)
        {
            AlignmentRowId = rowId,
            AlignmentOwnerMod = alignmentOwner
        };

    private static bool StringEquals(string left, string right)
        => string.Equals(left, right, StringComparison.Ordinal);

    private static bool AlignmentEquals(TestValue left, TestValue right)
        => string.Equals(left.Key, right.Key, StringComparison.Ordinal);

    private static string[] Representatives(IEnumerable<ListAlignmentRow<string>> rows)
        => rows.Select(row => row.Representative).ToArray();

    private static string[] Keys(IEnumerable<ListAlignmentRow<TestValue>> rows)
        => rows.Select(row => row.Representative.Key).ToArray();

    private static string[] ActiveValues(
        IEnumerable<ListPropertyValueContext<TestValue>> items)
        => items.Where(item => !item.IsRemoved).Select(item => item.Value.Value).ToArray();

    private sealed record TestValue(string Key, string Value);

    private sealed class TestListHandler : AbstractListPropertyHandler<TestValue>
    {
        private readonly SkyrimMod _currentMod = new(CurrentModKey, SkyrimRelease.SkyrimSE);

        public override string PropertyName => "Test";
        public override ListSemantics Semantics => ListSemantics.AlignedOrdered;

        public void Reconcile(
            IReadOnlyList<TestValue> declaredItems,
            IReadOnlyList<int> declaredRowIds,
            ListPropertyContext<TestValue> context,
            List<ListPropertyValueContext<TestValue>> forwardItems)
            => ProcessAlignedItems(
                CurrentModKey.ToString(),
                _currentMod,
                declaredItems,
                declaredRowIds,
                context,
                forwardItems);

        public void ProcessColumn(
            ModKey modKey,
            IReadOnlyList<TestValue> declaredItems,
            ListPropertyContext<TestValue> context)
        {
            var declaredRows = AlignCurrentList(context, declaredItems);
            ProcessAlignedItems(
                modKey.ToString(),
                new SkyrimMod(modKey, SkyrimRelease.SkyrimSE),
                declaredItems,
                declaredRows,
                context,
                context.ForwardValueContexts!);
        }

        public override void SetValue(IMajorRecord record, List<TestValue>? value)
            => throw new NotSupportedException();

        public override List<TestValue>? GetValue(IMajorRecordGetter record)
            => throw new NotSupportedException();

        protected override bool IsItemEqual(TestValue? item1, TestValue? item2)
            => item1 != null && item2 != null && item1 == item2;

        protected override bool IsAlignmentEqual(TestValue? item1, TestValue? item2)
            => item1 != null && item2 != null && AlignmentEquals(item1, item2);
    }
}
