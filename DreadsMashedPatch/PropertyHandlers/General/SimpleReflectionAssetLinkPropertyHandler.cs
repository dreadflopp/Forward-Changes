using DreadsMashedPatch.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Assets;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Plugins.Records;

namespace DreadsMashedPatch.PropertyHandlers.General;

/// <summary>
/// Reflection-backed scalar asset property that converts getter overlays to the
/// mutable AssetLink type while preserving the exact serialized GivenPath.
/// </summary>
public sealed class SimpleReflectionAssetLinkPropertyHandler<TAssetType, TRecord, TRecordGetter>
    : AbstractPropertyHandler<AssetLinkGetter<TAssetType>?>
    where TAssetType : class, IAssetType
    where TRecord : class, IMajorRecord
    where TRecordGetter : class, IMajorRecordGetter
{
    private readonly System.Reflection.PropertyInfo _getterProperty;
    private readonly System.Reflection.PropertyInfo? _setterProperty;
    private readonly bool _canBeNull;

    public SimpleReflectionAssetLinkPropertyHandler(string propertyName)
    {
        PropertyName = propertyName;
        _getterProperty = ReflectionPropertyResolver.Find(typeof(TRecordGetter), propertyName)
            ?? throw new ArgumentException($"Property '{propertyName}' not found on {typeof(TRecordGetter).Name}");
        _setterProperty = ReflectionPropertyResolver.Find(typeof(TRecord), propertyName);
        _canBeNull = ReflectionPropertyResolver.IsNullable(_getterProperty);
    }

    public override string PropertyName { get; }

    public override AssetLinkGetter<TAssetType>? GetValue(IMajorRecordGetter record)
        => record is TRecordGetter typedRecord
            ? _getterProperty.GetValue(typedRecord) as AssetLinkGetter<TAssetType>
            : null;

    public override void SetValue(IMajorRecord record, AssetLinkGetter<TAssetType>? value)
    {
        if (record is not TRecord typedRecord || _setterProperty == null)
        {
            return;
        }

        _setterProperty.SetValue(
            typedRecord,
            value == null && !_canBeNull
                ? new AssetLink<TAssetType>()
                : AssetPathHelper.Copy(value));
    }

    public override bool AreValuesEqual(
        AssetLinkGetter<TAssetType>? value1,
        AssetLinkGetter<TAssetType>? value2)
        => AssetPathHelper.AreEqual(value1, value2);

    public override string FormatValue(object? value)
        => value is IAssetLinkGetter<TAssetType> assetLink
            ? AssetPathHelper.Format(assetLink)
            : "null";
}
