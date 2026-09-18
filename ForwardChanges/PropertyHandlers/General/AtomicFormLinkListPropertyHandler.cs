using System.Collections;
using System.Reflection;
using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace ForwardChanges.PropertyHandlers.General;

/// <summary>
/// Treats a form-link list as one ordered value instead of merging individual entries.
/// </summary>
public sealed class AtomicFormLinkListPropertyHandler<TTarget, TRecord, TRecordGetter>
    : AbstractPropertyHandler<List<FormKey>>
    where TTarget : class, IMajorRecordGetter
    where TRecord : class, IMajorRecord
    where TRecordGetter : class, IMajorRecordGetter
{
    private readonly PropertyInfo _getterProperty;
    private readonly PropertyInfo _setterProperty;

    public AtomicFormLinkListPropertyHandler(string propertyName)
    {
        PropertyName = propertyName;
        _getterProperty = ReflectionPropertyResolver.Find(typeof(TRecordGetter), propertyName)
            ?? throw new ArgumentException($"Property '{propertyName}' not found on {typeof(TRecordGetter).Name}.");
        _setterProperty = ReflectionPropertyResolver.Find(typeof(TRecord), propertyName)
            ?? throw new ArgumentException($"Property '{propertyName}' not found on {typeof(TRecord).Name}.");
    }

    public override string PropertyName { get; }

    public override List<FormKey>? GetValue(IMajorRecordGetter record)
    {
        var typedRecord = TryCastRecord<TRecordGetter>(record, PropertyName);
        if (typedRecord == null)
        {
            return null;
        }

        var value = _getterProperty.GetValue(typedRecord);
        if (value == null)
        {
            return null;
        }

        if (value is not IEnumerable<IFormLinkGetter<TTarget>> links)
        {
            throw new InvalidOperationException(
                $"Property '{PropertyName}' on {typeof(TRecordGetter).Name} is not a form-link list targeting {typeof(TTarget).Name}.");
        }

        return links.Select(link => link.FormKey).ToList();
    }

    public override void SetValue(IMajorRecord record, List<FormKey>? value)
    {
        var typedRecord = TryCastRecord<TRecord>(record, PropertyName);
        if (typedRecord == null)
        {
            return;
        }

        var currentValue = _setterProperty.GetValue(typedRecord);
        if (currentValue is ICollection<IFormLinkGetter<TTarget>> links)
        {
            links.Clear();
            if (value == null)
            {
                return;
            }

            foreach (var formKey in value)
            {
                links.Add(new FormLink<TTarget>(formKey));
            }

            return;
        }

        if (currentValue is IList nonGenericList)
        {
            nonGenericList.Clear();
            if (value == null)
            {
                return;
            }

            foreach (var formKey in value)
            {
                nonGenericList.Add(new FormLink<TTarget>(formKey));
            }

            return;
        }

        throw new InvalidOperationException(
            $"Property '{PropertyName}' on {typeof(TRecord).Name} is not a mutable form-link collection.");
    }

    public override bool AreValuesEqual(List<FormKey>? value1, List<FormKey>? value2)
    {
        if (ReferenceEquals(value1, value2)) return true;
        if (value1 == null || value2 == null) return false;
        return value1.SequenceEqual(value2);
    }
}
