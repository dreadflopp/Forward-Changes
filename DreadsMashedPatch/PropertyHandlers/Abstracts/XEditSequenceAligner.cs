using DreadsMashedPatch.Contexts;

namespace DreadsMashedPatch.PropertyHandlers.Abstracts;

/*
 * The Myers diff below follows TDiff 3.1 by Angus Johnson.
 * Copyright (c) 2001-2009 Angus Johnson.
 * The original component is freeware provided its copyright notice, terms,
 * and conditions remain present; modifications must be documented.
 * This C# port uses generic equality and returns typed edit steps for Forward
 * Changes. It preserves TDiff's branch and tie choices for xEdit compatibility.
 */
internal static class XEditSequenceAligner
{
    internal sealed record Result<T>(
        List<ListAlignmentRow<T>> Rows,
        int[] RightRowIds)
        where T : class;

    internal static Result<T> Align<T>(
        IReadOnlyList<ListAlignmentRow<T>> leftRows,
        IReadOnlyList<T> rightItems,
        Func<T, T, bool> equals,
        Func<int> nextRowId)
        where T : class
    {
        var steps = MyersDiff<ListAlignmentRow<T>, T>.Create(
            leftRows,
            rightItems,
            (left, right) => equals(left.Representative, right));
        var rows = new List<ListAlignmentRow<T>>(steps.Count);
        var rightRowIds = new int[rightItems.Count];

        foreach (var step in steps)
        {
            switch (step.Kind)
            {
                case DiffKind.Match:
                {
                    var row = leftRows[step.LeftIndex];
                    rows.Add(row);
                    rightRowIds[step.RightIndex] = row.Id;
                    break;
                }
                case DiffKind.Delete:
                    rows.Add(leftRows[step.LeftIndex]);
                    break;
                case DiffKind.Add:
                {
                    var row = new ListAlignmentRow<T>(nextRowId(), rightItems[step.RightIndex]);
                    rows.Add(row);
                    rightRowIds[step.RightIndex] = row.Id;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return new Result<T>(rows, rightRowIds);
    }

    private enum DiffKind
    {
        Match,
        Add,
        Delete
    }

    private readonly record struct DiffStep(DiffKind Kind, int LeftIndex, int RightIndex);

    /// <summary>
    /// Port of the Myers O(ND) integer-sequence diff used by xEdit's TDiff.
    /// The branch choices intentionally mirror TDiff so ambiguous duplicates
    /// align the same way instead of depending on a different LCS tie-breaker.
    /// </summary>
    private sealed class MyersDiff<TLeft, TRight>(
        IReadOnlyList<TLeft> left,
        IReadOnlyList<TRight> right,
        Func<TLeft, TRight, bool> equals)
    {
        private const int MaxDiagonal = 0xFFFFFF;
        private readonly List<DiffStep> _steps = [];
        private int _lastLeft = -1;
        private int _lastRight = -1;

        public static IReadOnlyList<DiffStep> Create(
            IReadOnlyList<TLeft> left,
            IReadOnlyList<TRight> right,
            Func<TLeft, TRight, bool> equals)
            => new MyersDiff<TLeft, TRight>(left, right, equals).Execute();

        private IReadOnlyList<DiffStep> Execute()
        {
            var leftLength = left.Count;
            var rightLength = right.Count;
            var savedLeftEnd = leftLength - 1;

            while (leftLength > 0
                   && rightLength > 0
                   && equals(left[leftLength - 1], right[rightLength - 1]))
            {
                leftLength--;
                rightLength--;
            }

            if (leftLength != 0 || rightLength != 0)
            {
                var leftOffset = 0;
                var rightOffset = 0;
                while (leftLength > 0
                       && rightLength > 0
                       && equals(left[leftOffset], right[rightOffset]))
                {
                    leftLength--;
                    rightLength--;
                    leftOffset++;
                    rightOffset++;
                }

                RecursiveDiff(leftOffset, rightOffset, leftLength, rightLength);
            }

            while (_lastLeft < savedLeftEnd)
            {
                AddMatch();
            }

            return _steps;
        }

        private void RecursiveDiff(int leftOffset, int rightOffset, int leftLength, int rightLength)
        {
            if (leftLength == 0)
            {
                AddChange(leftOffset, rightLength, DiffKind.Add);
                return;
            }

            if (rightLength == 0)
            {
                AddChange(leftOffset, leftLength, DiffKind.Delete);
                return;
            }

            if (leftLength == 1 && rightLength == 1)
            {
                AddChange(leftOffset, 1, DiffKind.Delete);
                AddChange(leftOffset, 1, DiffKind.Add);
                return;
            }

            var maxOscillation = Math.Min(Math.Max(leftLength, rightLength), MaxDiagonal);
            var diagonalOffset = maxOscillation + 1;
            var forward = Enumerable.Repeat(-int.MaxValue, maxOscillation * 2 + 3).ToArray();
            var backward = Enumerable.Repeat(int.MaxValue, maxOscillation * 2 + 3).ToArray();

            int GetForward(int diagonal) => forward[diagonal + diagonalOffset];
            void SetForward(int diagonal, int value) => forward[diagonal + diagonalOffset] = value;
            int GetBackward(int diagonal) => backward[diagonal + diagonalOffset];
            void SetBackward(int diagonal, int value) => backward[diagonal + diagonalOffset] = value;

            SetForward(0, -1);
            var lengthDelta = leftLength - rightLength;
            SetBackward(lengthDelta, leftLength - 1);

            for (var oscillation = 1; oscillation <= maxOscillation; oscillation++)
            {
                var diagonal = oscillation;
                while (diagonal > leftLength) diagonal -= 2;
                while (diagonal >= Math.Max(-oscillation, -rightLength))
                {
                    var leftIndex = GetForward(diagonal - 1) < GetForward(diagonal + 1)
                        ? GetForward(diagonal + 1)
                        : GetForward(diagonal - 1) + 1;
                    var rightIndex = leftIndex - diagonal;

                    while (leftIndex < leftLength - 1
                           && rightIndex < rightLength - 1
                           && equals(
                               left[leftOffset + leftIndex + 1],
                               right[rightOffset + rightIndex + 1]))
                    {
                        leftIndex++;
                        rightIndex++;
                    }

                    SetForward(diagonal, leftIndex);
                    if ((lengthDelta & 1) != 0 && GetForward(diagonal) >= GetBackward(diagonal))
                    {
                        leftIndex++;
                        rightIndex++;
                        var splitLeft = leftIndex;
                        var splitRight = rightIndex;

                        while (leftIndex > 0
                               && rightIndex > 0
                               && equals(
                                   left[leftOffset + leftIndex - 1],
                                   right[rightOffset + rightIndex - 1]))
                        {
                            leftIndex--;
                            rightIndex--;
                        }

                        RecursiveDiff(leftOffset, rightOffset, leftIndex, rightIndex);
                        RecursiveDiff(
                            leftOffset + splitLeft,
                            rightOffset + splitRight,
                            leftLength - splitLeft,
                            rightLength - splitRight);
                        return;
                    }

                    diagonal -= 2;
                }

                diagonal = lengthDelta + oscillation;
                while (diagonal > leftLength) diagonal -= 2;
                while (diagonal >= Math.Max(lengthDelta - oscillation, -rightLength))
                {
                    var leftIndex = GetBackward(diagonal - 1) < GetBackward(diagonal + 1)
                        ? GetBackward(diagonal - 1)
                        : GetBackward(diagonal + 1) - 1;
                    var rightIndex = leftIndex - diagonal;

                    while (leftIndex > -1
                           && rightIndex > -1
                           && equals(
                               left[leftOffset + leftIndex],
                               right[rightOffset + rightIndex]))
                    {
                        leftIndex--;
                        rightIndex--;
                    }

                    SetBackward(diagonal, leftIndex);
                    if (GetBackward(diagonal) <= GetForward(diagonal))
                    {
                        leftIndex++;
                        rightIndex++;
                        RecursiveDiff(leftOffset, rightOffset, leftIndex, rightIndex);

                        while (leftIndex < leftLength
                               && rightIndex < rightLength
                               && equals(
                                   left[leftOffset + leftIndex],
                                   right[rightOffset + rightIndex]))
                        {
                            leftIndex++;
                            rightIndex++;
                        }

                        RecursiveDiff(
                            leftOffset + leftIndex,
                            rightOffset + rightIndex,
                            leftLength - leftIndex,
                            rightLength - rightIndex);
                        return;
                    }

                    diagonal -= 2;
                }
            }

            throw new InvalidOperationException("xEdit sequence alignment failed to find a diff path.");
        }

        private void AddChange(int leftOffset, int range, DiffKind kind)
        {
            while (_lastLeft < leftOffset - 1)
            {
                AddMatch();
            }

            for (var index = 0; index < range; index++)
            {
                if (kind == DiffKind.Add)
                {
                    _lastRight++;
                    _steps.Add(new DiffStep(DiffKind.Add, -1, _lastRight));
                }
                else
                {
                    _lastLeft++;
                    _steps.Add(new DiffStep(DiffKind.Delete, _lastLeft, -1));
                }
            }
        }

        private void AddMatch()
        {
            _lastLeft++;
            _lastRight++;
            _steps.Add(new DiffStep(DiffKind.Match, _lastLeft, _lastRight));
        }
    }
}
