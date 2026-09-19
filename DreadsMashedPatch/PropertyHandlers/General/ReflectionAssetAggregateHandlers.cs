using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace ForwardChanges.PropertyHandlers.General;

public sealed class SimpleReflectionIconsPropertyHandler<TRecord, TRecordGetter>
    : AbstractPropertyHandler<IIconsGetter?>
    where TRecord : class, IMajorRecord
    where TRecordGetter : class, IMajorRecordGetter
{
    private readonly System.Reflection.PropertyInfo _getterProperty;
    private readonly System.Reflection.PropertyInfo? _setterProperty;

    public SimpleReflectionIconsPropertyHandler(string propertyName)
    {
        PropertyName = propertyName;
        _getterProperty = ReflectionPropertyResolver.Find(typeof(TRecordGetter), propertyName)
            ?? throw new ArgumentException($"Property '{propertyName}' not found on {typeof(TRecordGetter).Name}");
        _setterProperty = ReflectionPropertyResolver.Find(typeof(TRecord), propertyName);
    }

    public override string PropertyName { get; }

    public override IIconsGetter? GetValue(IMajorRecordGetter record)
        => record is TRecordGetter typedRecord
            ? _getterProperty.GetValue(typedRecord) as IIconsGetter
            : null;

    public override void SetValue(IMajorRecord record, IIconsGetter? value)
    {
        if (record is TRecord typedRecord && _setterProperty != null)
        {
            _setterProperty.SetValue(typedRecord, value?.DeepCopy());
        }
    }

    public override bool AreValuesEqual(IIconsGetter? value1, IIconsGetter? value2)
    {
        if (value1 == null || value2 == null)
        {
            return value1 == null && value2 == null;
        }

        return AssetPathHelper.AreEqual(value1.LargeIconFilename, value2.LargeIconFilename)
            && AssetPathHelper.AreEqual(value1.SmallIconFilename, value2.SmallIconFilename);
    }
}

public sealed class SimpleReflectionModelPropertyHandler<TRecord, TRecordGetter>
    : AbstractPropertyHandler<IModelGetter?>
    where TRecord : class, IMajorRecord
    where TRecordGetter : class, IMajorRecordGetter
{
    private readonly System.Reflection.PropertyInfo _getterProperty;
    private readonly System.Reflection.PropertyInfo? _setterProperty;

    public SimpleReflectionModelPropertyHandler(string propertyName)
    {
        PropertyName = propertyName;
        _getterProperty = ReflectionPropertyResolver.Find(typeof(TRecordGetter), propertyName)
            ?? throw new ArgumentException($"Property '{propertyName}' not found on {typeof(TRecordGetter).Name}");
        _setterProperty = ReflectionPropertyResolver.Find(typeof(TRecord), propertyName);
    }

    public override string PropertyName { get; }

    public override IModelGetter? GetValue(IMajorRecordGetter record)
        => record is TRecordGetter typedRecord
            ? _getterProperty.GetValue(typedRecord) as IModelGetter
            : null;

    public override void SetValue(IMajorRecord record, IModelGetter? value)
    {
        if (record is TRecord typedRecord && _setterProperty != null)
        {
            _setterProperty.SetValue(typedRecord, value?.DeepCopy());
        }
    }

    public override bool AreValuesEqual(IModelGetter? value1, IModelGetter? value2)
    {
        if (value1 == null || value2 == null)
        {
            return value1 == null && value2 == null;
        }

        if (!AssetPathHelper.AreEqual(value1.File, value2.File))
        {
            return false;
        }

        return value1.Equals(value2);
    }
}
