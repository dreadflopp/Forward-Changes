using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;

namespace ForwardChanges.PropertyHandlers.General;

/// <summary>
/// Treats an ordered reflection-backed list as one property value. This is for
/// positional, structural, or otherwise non-alignable arrays whose entries must
/// not acquire ownership independently.
/// </summary>
public sealed class AtomicReflectionListPropertyHandler<TItem, TRecord, TRecordGetter>
    : AbstractPropertyHandler<List<TItem>>
    where TItem : class
    where TRecord : class, IMajorRecord
    where TRecordGetter : class, IMajorRecordGetter
{
    private readonly SimpleReflectionListPropertyHandler<TItem, TRecord, TRecordGetter> _accessor;

    public AtomicReflectionListPropertyHandler(string propertyName, bool? canBeNull = null)
    {
        _accessor = new SimpleReflectionListPropertyHandler<TItem, TRecord, TRecordGetter>(
            propertyName,
            ListSemantics.ExactOrdered,
            canBeNull);
    }

    public override string PropertyName => _accessor.PropertyName;

    public override List<TItem>? GetValue(IMajorRecordGetter record) => _accessor.GetValue(record);

    public override void SetValue(IMajorRecord record, List<TItem>? value) => _accessor.SetValue(record, value);

    public override bool AreValuesEqual(List<TItem>? value1, List<TItem>? value2) =>
        _accessor.AreValuesEqual(value1, value2);

    public override string FormatValue(object? value) => _accessor.FormatValue(value);
}
