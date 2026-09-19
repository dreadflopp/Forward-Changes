using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace DreadsMashedPatch.PropertyHandlers.Debris;

/// <summary>
/// DEBR model entries are one atomic ordered value. Generated DeepCopy preserves
/// the nested model filename's serialized GivenPath.
/// </summary>
public sealed class ModelsHandler : AbstractPropertyHandler<List<IDebrisModelGetter>>
{
    public override string PropertyName => "Models";

    public override List<IDebrisModelGetter>? GetValue(IMajorRecordGetter record)
        => record is IDebrisGetter debris ? debris.Models.Cast<IDebrisModelGetter>().ToList() : null;

    public override void SetValue(IMajorRecord record, List<IDebrisModelGetter>? value)
    {
        if (record is not IDebris debris)
        {
            return;
        }

        debris.Models.Clear();
        if (value == null)
        {
            return;
        }

        foreach (var model in value)
        {
            debris.Models.Add(model.DeepCopy());
        }
    }

    public override bool AreValuesEqual(
        List<IDebrisModelGetter>? value1,
        List<IDebrisModelGetter>? value2)
    {
        if (value1 == null || value2 == null)
        {
            return value1 == null && value2 == null;
        }

        return value1.Count == value2.Count
            && value1.Zip(value2, AreItemsEqual).All(equal => equal);
    }

    private static bool AreItemsEqual(IDebrisModelGetter left, IDebrisModelGetter right)
        => left.Percentage == right.Percentage
           && left.Flags == right.Flags
           && left.DATADataTypeState == right.DATADataTypeState
           && AssetPathHelper.AreEqual(left.ModelFilename, right.ModelFilename)
           && BinaryEqual(left.TextureFileHashes, right.TextureFileHashes);

    private static bool BinaryEqual(Noggog.ReadOnlyMemorySlice<byte>? left, Noggog.ReadOnlyMemorySlice<byte>? right)
    {
        if (!left.HasValue || !right.HasValue)
        {
            return left.HasValue == right.HasValue;
        }

        return left.Value.Span.SequenceEqual(right.Value.Span);
    }

    public override string FormatValue(object? value)
        => value is IEnumerable<IDebrisModelGetter> models
            ? string.Join(", ", models.Select((model, index) =>
                $"#{index}(Percent={model.Percentage}, Model={AssetPathHelper.Format(model.ModelFilename)}, Flags={model.Flags})"))
            : "null";
}
