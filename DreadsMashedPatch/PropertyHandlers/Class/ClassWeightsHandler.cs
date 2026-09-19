using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace ForwardChanges.PropertyHandlers.Class;

public sealed class ClassWeightsHandler<TKey> : AbstractPropertyHandler<IReadOnlyDictionary<TKey, byte>>
    where TKey : notnull
{
    private readonly string _propertyName;
    private readonly Func<IClassGetter, IReadOnlyDictionary<TKey, byte>> _get;
    private readonly Func<IClass, IDictionary<TKey, byte>> _getMutable;

    public ClassWeightsHandler(
        string propertyName,
        Func<IClassGetter, IReadOnlyDictionary<TKey, byte>> get,
        Func<IClass, IDictionary<TKey, byte>> getMutable)
    {
        _propertyName = propertyName;
        _get = get;
        _getMutable = getMutable;
    }

    public override string PropertyName => _propertyName;

    public override IReadOnlyDictionary<TKey, byte>? GetValue(IMajorRecordGetter record)
    {
        if (record is not IClassGetter classRecord)
        {
            Console.WriteLine($"Error: Record does not implement IClassGetter for {PropertyName}");
            return null;
        }

        // Snapshot the dictionary so property contexts never alias a mutable output record.
        return new Dictionary<TKey, byte>(_get(classRecord));
    }

    public override void SetValue(IMajorRecord record, IReadOnlyDictionary<TKey, byte>? value)
    {
        if (record is not IClass classRecord)
        {
            Console.WriteLine($"Error: Record does not implement IClass for {PropertyName}");
            return;
        }

        var target = _getMutable(classRecord);
        target.Clear();
        if (value == null)
        {
            return;
        }

        foreach (var (key, weight) in value)
        {
            target[key] = weight;
        }
    }

    public override bool AreValuesEqual(
        IReadOnlyDictionary<TKey, byte>? value1,
        IReadOnlyDictionary<TKey, byte>? value2)
    {
        if (ReferenceEquals(value1, value2)) return true;
        if (value1 == null || value2 == null || value1.Count != value2.Count) return false;

        return value1.All(pair =>
            value2.TryGetValue(pair.Key, out var otherWeight)
            && pair.Value == otherWeight);
    }
}
