using DreadsMashedPatch.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;

namespace DreadsMashedPatch.PropertyHandlers.General;

/// <summary>
/// Reflection access for generated Mutagen aggregates. The supplied generated
/// copy operation handles overlay-to-mutable conversion, including nested assets.
/// </summary>
public sealed class GeneratedCopyReflectionPropertyHandler<TGetter, TMutable, TRecord, TRecordGetter>
    : AbstractPropertyHandler<TGetter?>
    where TGetter : class
    where TMutable : class, TGetter
    where TRecord : class, IMajorRecord
    where TRecordGetter : class, IMajorRecordGetter
{
    private readonly System.Reflection.PropertyInfo _getterProperty;
    private readonly System.Reflection.PropertyInfo? _setterProperty;
    private readonly Func<TGetter, TMutable> _copy;
    private readonly Func<TGetter, TGetter, bool> _equals;

    public GeneratedCopyReflectionPropertyHandler(
        string propertyName,
        Func<TGetter, TMutable> copy,
        Func<TGetter, TGetter, bool> equals)
    {
        PropertyName = propertyName;
        _copy = copy;
        _equals = equals;
        _getterProperty = ReflectionPropertyResolver.Find(typeof(TRecordGetter), propertyName)
            ?? throw new ArgumentException($"Property '{propertyName}' not found on {typeof(TRecordGetter).Name}");
        _setterProperty = ReflectionPropertyResolver.Find(typeof(TRecord), propertyName);
    }

    public override string PropertyName { get; }

    public override TGetter? GetValue(IMajorRecordGetter record)
        => record is TRecordGetter typedRecord
            ? _getterProperty.GetValue(typedRecord) as TGetter
            : null;

    public override void SetValue(IMajorRecord record, TGetter? value)
    {
        if (record is TRecord typedRecord && _setterProperty != null)
        {
            _setterProperty.SetValue(typedRecord, value == null ? null : _copy(value));
        }
    }

    public override bool AreValuesEqual(TGetter? value1, TGetter? value2)
    {
        if (value1 == null || value2 == null)
        {
            return value1 == null && value2 == null;
        }

        return _equals(value1, value2);
    }
}
