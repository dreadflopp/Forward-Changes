using System.Security.Cryptography;
using System.Text;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Formatting;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace DreadsMashedPatch.PropertyHandlers.Package;

/// <summary>
/// The PACK template reference, template data and procedure tree are one
/// serialized graph. Procedure branches refer to entries in <see cref="Data"/>
/// by index, so independently owned values can produce a graph no source mod
/// authored. Mutagen serializes the dictionary's value records and UNAM index
/// records in key order; this handler preserves the association, not the source
/// plugin's incidental on-disk slot order.
///
/// Registration is temporarily disabled in PackageRecordHandler because that
/// canonical ordering changes xEdit's positional PACK data rows. Keep this
/// implementation and its binary regression tests for re-enabling once an
/// order-preserving Mutagen writer path exists.
/// </summary>
public sealed record PackageTemplateGraphValue(
    FormKey PackageTemplate,
    int DataInputVersion,
    IReadOnlyDictionary<sbyte, IAPackageDataGetter> Data,
    byte[] XnamMarker,
    IReadOnlyList<IPackageBranchGetter> ProcedureTree);

public sealed class PackageTemplateGraphHandler : AbstractPropertyHandler<PackageTemplateGraphValue>, IDiagnosticDiffPropertyHandler
{
    public override string PropertyName => "PackageTemplateGraph";

    public override PackageTemplateGraphValue? GetValue(IMajorRecordGetter record)
    {
        if (record is not IPackageGetter package)
        {
            return null;
        }

        return new PackageTemplateGraphValue(
            package.PackageTemplate.FormKey,
            package.DataInputVersion,
            package.Data,
            package.XnamMarker.ToArray(),
            package.ProcedureTree);
    }

    public override void SetValue(IMajorRecord record, PackageTemplateGraphValue? value)
    {
        if (record is not IPackage package || value == null)
        {
            return;
        }

        package.PackageTemplate.SetTo(value.PackageTemplate);
        package.DataInputVersion = value.DataInputVersion;

        package.Data.Clear();
        foreach (var (index, data) in value.Data)
        {
            package.Data[index] = data.DeepCopy();
        }

        package.XnamMarker = new MemorySlice<byte>(value.XnamMarker.ToArray());

        package.ProcedureTree.Clear();
        foreach (var branch in value.ProcedureTree)
        {
            package.ProcedureTree.Add(branch.DeepCopy());
        }
    }

    public override bool AreValuesEqual(
        PackageTemplateGraphValue? value1,
        PackageTemplateGraphValue? value2)
    {
        if (value1 == null || value2 == null)
        {
            return value1 == null && value2 == null;
        }

        return value1.PackageTemplate.Equals(value2.PackageTemplate)
            && value1.DataInputVersion == value2.DataInputVersion
            && value1.XnamMarker.AsSpan().SequenceEqual(value2.XnamMarker)
            && DataEquals(value1.Data, value2.Data)
            && ProcedureTreeEquals(value1.ProcedureTree, value2.ProcedureTree);
    }

    public override string FormatValue(object? value)
    {
        if (value is not PackageTemplateGraphValue graph)
        {
            return value?.ToString() ?? "null";
        }

        if (!LogCollector.IsDeepDiveMode)
        {
            return $"Template:{graph.PackageTemplate}, Version:{graph.DataInputVersion}, " +
                   $"Data:{graph.Data.Count}, ProcedureTree:{graph.ProcedureTree.Count}";
        }

        var sourceOrder = GetSourceValueOrder(graph.Data);
        var writerOrder = sourceOrder.Order().ToArray();
        var firstOrderMismatch = FirstMismatch(sourceOrder, writerOrder);
        var order = firstOrderMismatch < 0
            ? "key-sorted"
            : $"mismatch@{firstOrderMismatch} " +
              $"source:{Preview(sourceOrder)} writer:{Preview(writerOrder)}";
        var metadataIndexes = graph.Data
            .Where(entry => !IsSerializedDataInput(entry.Value))
            .Select(entry => entry.Key)
            .Order()
            .ToArray();

        return $"{FormatIdentity(graph)} Template:{graph.PackageTemplate}, " +
               $"Version:{graph.DataInputVersion}, Data:{graph.Data.Count} " +
               $"(values:{sourceOrder.Length}, metadata:{metadataIndexes.Length}), " +
               $"Order:{order}, Metadata:{Preview(metadataIndexes)}, " +
               $"ProcedureTree:{graph.ProcedureTree.Count}";
    }

    public string FormatIdentity(object? value)
    {
        return value is PackageTemplateGraphValue graph
            ? $"Graph#{ComputeFingerprint(graph)}"
            : "Graph#null";
    }

    public string FormatDifference(object? olderValue, object? newerValue)
    {
        if (olderValue is not PackageTemplateGraphValue older
            || newerValue is not PackageTemplateGraphValue newer)
        {
            return $"{FormatIdentity(olderValue)} -> {FormatIdentity(newerValue)}";
        }

        var changes = new List<string>();
        if (older.PackageTemplate != newer.PackageTemplate)
        {
            changes.Add($"Template:{older.PackageTemplate}->{newer.PackageTemplate}");
        }
        if (older.DataInputVersion != newer.DataInputVersion)
        {
            changes.Add($"Version:{older.DataInputVersion}->{newer.DataInputVersion}");
        }
        if (!older.XnamMarker.AsSpan().SequenceEqual(newer.XnamMarker))
        {
            changes.Add($"Marker:{Convert.ToHexString(older.XnamMarker)}->{Convert.ToHexString(newer.XnamMarker)}");
        }

        AppendDataDifference(changes, older.Data, newer.Data);
        AppendProcedureTreeDifference(changes, older.ProcedureTree, newer.ProcedureTree);

        return changes.Count == 0 ? "No semantic changes" : string.Join(" | ", changes);
    }

    private static bool IsSerializedDataInput(IAPackageDataGetter data) =>
        data is IPackageDataBoolGetter
            or IPackageDataIntGetter
            or IPackageDataTargetGetter
            or IPackageDataFloatGetter
            or IPackageDataObjectListGetter
            or IPackageDataTopicGetter
            or IPackageDataLocationGetter;

    private static void AppendDataDifference(
        ICollection<string> changes,
        IReadOnlyDictionary<sbyte, IAPackageDataGetter> older,
        IReadOnlyDictionary<sbyte, IAPackageDataGetter> newer)
    {
        var added = newer.Keys.Except(older.Keys).Order().ToArray();
        var removed = older.Keys.Except(newer.Keys).Order().ToArray();
        var changed = older.Keys.Intersect(newer.Keys)
            .Where(index => !older[index].Equals(newer[index], equalsMask: null))
            .Order()
            .ToArray();

        if (added.Length > 0)
        {
            changes.Add($"Data added:{Preview(added)} {FormatDataDetails(newer, added)}");
        }
        if (removed.Length > 0)
        {
            changes.Add($"Data removed:{Preview(removed)} {FormatDataDetails(older, removed)}");
        }
        if (changed.Length > 0)
        {
            var details = changed.Take(8).Select(index =>
                $"{index}:{DiagnosticValueFormatter.Format(older[index])}" +
                $"->{DiagnosticValueFormatter.Format(newer[index])}");
            changes.Add($"Data changed:{Preview(changed)} [{string.Join("; ", details)}" +
                        $"{Remainder(changed.Length, 8)}]");
        }

        var olderOrder = GetSourceValueOrder(older);
        var newerOrder = GetSourceValueOrder(newer);
        if (!olderOrder.SequenceEqual(newerOrder))
        {
            changes.Add($"Source order:{Preview(olderOrder)}->{Preview(newerOrder)}");
        }
    }

    private static void AppendProcedureTreeDifference(
        ICollection<string> changes,
        IReadOnlyList<IPackageBranchGetter> older,
        IReadOnlyList<IPackageBranchGetter> newer)
    {
        var commonCount = Math.Min(older.Count, newer.Count);
        var changedPositions = Enumerable.Range(0, commonCount)
            .Where(index => !older[index].Equals(newer[index], equalsMask: null))
            .ToArray();

        if (older.Count != newer.Count)
        {
            changes.Add($"Tree count:{older.Count}->{newer.Count}");
            if (newer.Count > older.Count)
            {
                var addedPositions = Enumerable.Range(older.Count, newer.Count - older.Count).ToArray();
                var details = addedPositions.Take(8).Select(index =>
                    $"{index}:{FormatBranch(newer[index])}");
                changes.Add($"Tree added:{Preview(addedPositions)} [{string.Join("; ", details)}" +
                            $"{Remainder(addedPositions.Length, 8)}]");
            }
            else
            {
                var removedPositions = Enumerable.Range(newer.Count, older.Count - newer.Count).ToArray();
                var details = removedPositions.Take(8).Select(index =>
                    $"{index}:{FormatBranch(older[index])}");
                changes.Add($"Tree removed:{Preview(removedPositions)} [{string.Join("; ", details)}" +
                            $"{Remainder(removedPositions.Length, 8)}]");
            }
        }
        if (changedPositions.Length > 0)
        {
            var details = changedPositions.Take(8).Select(index =>
                $"{index}:{FormatBranch(older[index])}->{FormatBranch(newer[index])}");
            changes.Add($"Tree changed:{Preview(changedPositions)} [{string.Join("; ", details)}" +
                        $"{Remainder(changedPositions.Length, 8)}]");
        }
    }

    private static string FormatDataDetails(
        IReadOnlyDictionary<sbyte, IAPackageDataGetter> data,
        IReadOnlyCollection<sbyte> indexes)
    {
        var details = indexes.Take(8).Select(index =>
            $"{index}:{DiagnosticValueFormatter.Format(data[index])}");
        return $"[{string.Join("; ", details)}{Remainder(indexes.Count, 8)}]";
    }

    private static string FormatBranch(IPackageBranchGetter branch) =>
        $"{branch.BranchType}/{branch.ProcedureType ?? "-"}" +
        $"(inputs:{Preview(branch.DataInputIndices)}, conditions:{branch.Conditions.Count})";

    private static sbyte[] GetSourceValueOrder(
        IReadOnlyDictionary<sbyte, IAPackageDataGetter> data) =>
        data.Where(entry => IsSerializedDataInput(entry.Value))
            .Select(entry => entry.Key)
            .ToArray();

    private static int FirstMismatch<T>(IReadOnlyList<T> left, IReadOnlyList<T> right)
    {
        var count = Math.Min(left.Count, right.Count);
        for (var index = 0; index < count; index++)
        {
            if (!EqualityComparer<T>.Default.Equals(left[index], right[index]))
            {
                return index;
            }
        }

        return left.Count == right.Count ? -1 : count;
    }

    private static string Preview<T>(IEnumerable<T> values, int limit = 16)
    {
        var materialized = values.ToArray();
        var preview = string.Join(",", materialized.Take(limit));
        return $"[{preview}{Remainder(materialized.Length, limit)}]";
    }

    private static string Remainder(int count, int limit) =>
        count > limit ? $",...(+{count - limit})" : string.Empty;

    private static string ComputeFingerprint(PackageTemplateGraphValue graph)
    {
        try
        {
            var text = new StringBuilder()
                .Append(graph.PackageTemplate).Append('|')
                .Append(graph.DataInputVersion).Append('|')
                .Append(Convert.ToHexString(graph.XnamMarker));

            foreach (var (index, data) in graph.Data.OrderBy(entry => entry.Key))
            {
                text.Append("|D:").Append(index).Append(':').Append(data.Print());
            }
            foreach (var branch in graph.ProcedureTree)
            {
                text.Append("|B:").Append(branch.Print());
            }

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text.ToString()));
            return Convert.ToHexString(hash.AsSpan(0, 4));
        }
        catch (Exception ex)
        {
            return $"ERROR-{ex.GetType().Name}";
        }
    }

    private static bool DataEquals(
        IReadOnlyDictionary<sbyte, IAPackageDataGetter> value1,
        IReadOnlyDictionary<sbyte, IAPackageDataGetter> value2)
    {
        if (value1.Count != value2.Count)
        {
            return false;
        }

        foreach (var (index, data) in value1)
        {
            if (!value2.TryGetValue(index, out var rhs)
                || !data.Equals(rhs, equalsMask: null))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ProcedureTreeEquals(
        IReadOnlyList<IPackageBranchGetter> value1,
        IReadOnlyList<IPackageBranchGetter> value2)
    {
        if (value1.Count != value2.Count)
        {
            return false;
        }

        for (var index = 0; index < value1.Count; index++)
        {
            if (!value1[index].Equals(value2[index], equalsMask: null))
            {
                return false;
            }
        }

        return true;
    }
}
