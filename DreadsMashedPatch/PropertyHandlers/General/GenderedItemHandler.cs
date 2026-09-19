using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Formatting;
using Mutagen.Bethesda.Plugins.Records;

namespace DreadsMashedPatch.PropertyHandlers.General;

/// <summary>
/// Copies and compares a two-sided gendered aggregate without treating its
/// IEnumerable implementation as an ordinary list.
/// </summary>
public sealed class GenderedItemHandler<TGetterItem, TMutableItem, TRecord, TRecordGetter>
    : AbstractPropertyHandler<IGenderedItemGetter<TGetterItem>>
    where TRecord : class, IMajorRecord
    where TRecordGetter : class, IMajorRecordGetter
{
    private readonly string _propertyName;
    private readonly Func<TRecordGetter, IGenderedItemGetter<TGetterItem>?> _getValue;
    private readonly Action<TRecord, IGenderedItem<TMutableItem>?> _setValue;
    private readonly Func<TGetterItem, TMutableItem> _copyItem;
    private readonly Func<TGetterItem, TGetterItem, bool> _itemsEqual;

    public GenderedItemHandler(
        string propertyName,
        Func<TRecordGetter, IGenderedItemGetter<TGetterItem>?> getValue,
        Action<TRecord, IGenderedItem<TMutableItem>?> setValue,
        Func<TGetterItem, TMutableItem> copyItem,
        Func<TGetterItem, TGetterItem, bool>? itemsEqual = null)
    {
        _propertyName = propertyName;
        _getValue = getValue;
        _setValue = setValue;
        _copyItem = copyItem;
        _itemsEqual = itemsEqual ?? EqualityComparer<TGetterItem>.Default.Equals;
    }

    public override string PropertyName => _propertyName;

    public override IGenderedItemGetter<TGetterItem>? GetValue(IMajorRecordGetter record)
        => record is TRecordGetter typedRecord ? _getValue(typedRecord) : null;

    public override void SetValue(IMajorRecord record, IGenderedItemGetter<TGetterItem>? value)
    {
        if (record is not TRecord typedRecord)
        {
            return;
        }

        _setValue(
            typedRecord,
            value == null
                ? null
                : new GenderedItem<TMutableItem>(_copyItem(value.Male), _copyItem(value.Female)));
    }

    public override bool AreValuesEqual(
        IGenderedItemGetter<TGetterItem>? value1,
        IGenderedItemGetter<TGetterItem>? value2)
    {
        if (value1 == null || value2 == null)
        {
            return value1 == null && value2 == null;
        }

        return _itemsEqual(value1.Male, value2.Male)
            && _itemsEqual(value1.Female, value2.Female);
    }

    public override string FormatValue(object? value)
    {
        if (value is not IGenderedItemGetter<TGetterItem> gendered)
        {
            return DiagnosticValueFormatter.Format(value);
        }

        return $"Male: {DiagnosticValueFormatter.Format(gendered.Male)}, Female: {DiagnosticValueFormatter.Format(gendered.Female)}";
    }
}

/// <summary>
/// Tracks one side of a gendered aggregate independently and preserves the
/// opposite side when applying a forwarded value.
/// </summary>
public sealed class GenderedItemSideHandler<TItem, TRecord, TRecordGetter>
    : AbstractPropertyHandler<TItem>
    where TRecord : class, IMajorRecord
    where TRecordGetter : class, IMajorRecordGetter
{
    private readonly MaleFemaleGender _gender;
    private readonly Func<TRecordGetter, IGenderedItemGetter<TItem>?> _getValue;
    private readonly Action<TRecord, IGenderedItem<TItem>?> _setValue;
    private readonly Func<TItem, TItem> _copyItem;
    private readonly Func<TItem, TItem, bool> _itemsEqual;

    public GenderedItemSideHandler(
        string propertyName,
        MaleFemaleGender gender,
        Func<TRecordGetter, IGenderedItemGetter<TItem>?> getValue,
        Action<TRecord, IGenderedItem<TItem>?> setValue,
        Func<TItem, TItem> copyItem,
        Func<TItem, TItem, bool>? itemsEqual = null)
    {
        PropertyName = $"{propertyName}.{gender}";
        _gender = gender;
        _getValue = getValue;
        _setValue = setValue;
        _copyItem = copyItem;
        _itemsEqual = itemsEqual ?? EqualityComparer<TItem>.Default.Equals;
    }

    public override string PropertyName { get; }

    public override TItem? GetValue(IMajorRecordGetter record)
    {
        if (record is not TRecordGetter typedRecord)
        {
            return default;
        }

        var gendered = _getValue(typedRecord);
        return gendered == null ? default : gendered[_gender];
    }

    public override void SetValue(IMajorRecord record, TItem? value)
    {
        if (record is not TRecord typedRecord || record is not TRecordGetter getterRecord)
        {
            return;
        }

        var current = _getValue(getterRecord);
        var male = _gender == MaleFemaleGender.Male ? value! : current == null ? default! : current.Male;
        var female = _gender == MaleFemaleGender.Female ? value! : current == null ? default! : current.Female;
        _setValue(typedRecord, new GenderedItem<TItem>(_copyItem(male), _copyItem(female)));
    }

    public override bool AreValuesEqual(TItem? value1, TItem? value2)
    {
        if (value1 is null || value2 is null)
        {
            return value1 is null && value2 is null;
        }

        return _itemsEqual(value1, value2);
    }
}
