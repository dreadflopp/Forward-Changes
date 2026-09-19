using DreadsMashedPatch.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using Noggog;

namespace DreadsMashedPatch.PropertyHandlers.General;

public sealed record ModelFileValue(string Path, byte[]? Data);

public abstract class GenderedModelFieldHandler<TValue, TRecord, TRecordGetter> : AbstractPropertyHandler<TValue>
    where TRecord : class, IMajorRecord
    where TRecordGetter : class, IMajorRecordGetter
{
    private readonly Func<TRecordGetter, IGenderedItemGetter<IModelGetter?>?> _getValue;
    private readonly Action<TRecord, IGenderedItem<Model?>?> _setValue;

    protected GenderedModelFieldHandler(
        string modelPropertyName,
        MaleFemaleGender gender,
        string fieldName,
        Func<TRecordGetter, IGenderedItemGetter<IModelGetter?>?> getValue,
        Action<TRecord, IGenderedItem<Model?>?> setValue)
    {
        Gender = gender;
        PropertyName = $"{modelPropertyName}.{gender}.{fieldName}";
        _getValue = getValue;
        _setValue = setValue;
    }

    public sealed override string PropertyName { get; }

    protected MaleFemaleGender Gender { get; }

    protected IModelGetter? GetModel(IMajorRecordGetter record)
    {
        if (record is not TRecordGetter typedRecord)
        {
            return null;
        }

        var gendered = _getValue(typedRecord);
        return gendered?[Gender];
    }

    protected void SetModel(IMajorRecord record, Model? model)
    {
        if (record is not TRecord typedRecord || record is not TRecordGetter getterRecord)
        {
            return;
        }

        var current = _getValue(getterRecord);
        var male = Gender == MaleFemaleGender.Male ? model : CopyModel(current?.Male);
        var female = Gender == MaleFemaleGender.Female ? model : CopyModel(current?.Female);
        _setValue(
            typedRecord,
            male == null && female == null
                ? null
                : new GenderedItem<Model?>(male, female));
    }

    protected static string GetModelPathNormalized(IAssetLinkGetter<SkyrimModelAssetType> file)
        => ModelPathHelper.NormalizeAsset(file);

    protected static string NormalizeModelPath(string path)
        => ModelPathHelper.NormalizePath(path);

    protected static Model? CopyModel(IModelGetter? source)
    {
        if (source == null)
        {
            return null;
        }

        return new Model
        {
            File = new AssetLink<SkyrimModelAssetType>(GetModelPathNormalized(source.File)),
            Data = source.Data?.ToArray(),
            AlternateTextures = CopyAlternateTextures(source.AlternateTextures)
        };
    }

    protected static ExtendedList<AlternateTexture>? CopyAlternateTextures(
        IReadOnlyList<IAlternateTextureGetter>? source)
    {
        if (source == null)
        {
            return null;
        }

        var copy = new ExtendedList<AlternateTexture>();
        foreach (var alternateTexture in source)
        {
            copy.Add(new AlternateTexture
            {
                Name = alternateTexture.Name,
                NewTexture = new FormLink<ITextureSetGetter>(alternateTexture.NewTexture.FormKey),
                Index = alternateTexture.Index
            });
        }

        return copy;
    }
}

/// <summary>
/// Resolves a model filename independently. Model information travels with a
/// selected filename but does not participate in conflict detection itself.
/// </summary>
public sealed class GenderedModelFileHandler<TRecord, TRecordGetter>
    : GenderedModelFieldHandler<ModelFileValue, TRecord, TRecordGetter>
    where TRecord : class, IMajorRecord
    where TRecordGetter : class, IMajorRecordGetter
{
    public GenderedModelFileHandler(
        string modelPropertyName,
        MaleFemaleGender gender,
        Func<TRecordGetter, IGenderedItemGetter<IModelGetter?>?> getValue,
        Action<TRecord, IGenderedItem<Model?>?> setValue)
        : base(modelPropertyName, gender, "File", getValue, setValue)
    {
    }

    public override ModelFileValue? GetValue(IMajorRecordGetter record)
    {
        var model = GetModel(record);
        return model == null
            ? null
            : new ModelFileValue(GetModelPathNormalized(model.File), model.Data?.ToArray());
    }

    public override void SetValue(IMajorRecord record, ModelFileValue? value)
    {
        if (value == null)
        {
            SetModel(record, null);
            return;
        }

        var model = CopyModel(GetModel(record)) ?? new Model();
        model.File = new AssetLink<SkyrimModelAssetType>(value.Path);
        model.Data = value.Data?.ToArray();
        SetModel(record, model);
    }

    public override bool AreValuesEqual(ModelFileValue? value1, ModelFileValue? value2)
    {
        if (value1 == null || value2 == null)
        {
            return value1 == null && value2 == null;
        }

        return StringComparer.OrdinalIgnoreCase.Equals(
            NormalizeModelPath(value1.Path),
            NormalizeModelPath(value2.Path));
    }

    public override string FormatValue(object? value)
        => value is ModelFileValue file ? file.Path : "null";
}

public sealed class GenderedModelAlternateTexturesHandler<TRecord, TRecordGetter>
    : GenderedModelFieldHandler<IReadOnlyList<IAlternateTextureGetter>?, TRecord, TRecordGetter>
    where TRecord : class, IMajorRecord
    where TRecordGetter : class, IMajorRecordGetter
{
    public GenderedModelAlternateTexturesHandler(
        string modelPropertyName,
        MaleFemaleGender gender,
        Func<TRecordGetter, IGenderedItemGetter<IModelGetter?>?> getValue,
        Action<TRecord, IGenderedItem<Model?>?> setValue)
        : base(modelPropertyName, gender, "AlternateTextures", getValue, setValue)
    {
    }

    public override IReadOnlyList<IAlternateTextureGetter>? GetValue(IMajorRecordGetter record)
        => GetModel(record)?.AlternateTextures;

    public override void SetValue(IMajorRecord record, IReadOnlyList<IAlternateTextureGetter>? value)
    {
        var currentModel = GetModel(record);
        if (currentModel == null && value == null)
        {
            return;
        }

        var model = CopyModel(currentModel) ?? new Model();
        model.AlternateTextures = CopyAlternateTextures(value);
        SetModel(record, model);
    }

    public override bool AreValuesEqual(
        IReadOnlyList<IAlternateTextureGetter>? value1,
        IReadOnlyList<IAlternateTextureGetter>? value2)
    {
        var count1 = value1?.Count ?? 0;
        var count2 = value2?.Count ?? 0;
        if (count1 != count2)
        {
            return false;
        }

        for (var index = 0; index < count1; index++)
        {
            var alternate1 = value1![index];
            var alternate2 = value2![index];
            if (!string.Equals(alternate1.Name, alternate2.Name, StringComparison.Ordinal)
                || alternate1.NewTexture.FormKey != alternate2.NewTexture.FormKey
                || alternate1.Index != alternate2.Index)
            {
                return false;
            }
        }

        return true;
    }

    public override string FormatValue(object? value)
    {
        if (value is not IReadOnlyList<IAlternateTextureGetter> alternateTextures
            || alternateTextures.Count == 0)
        {
            return "[]";
        }

        return $"[{string.Join(", ", alternateTextures.Select(FormatAlternateTexture))}]";
    }

    private static string FormatAlternateTexture(IAlternateTextureGetter texture)
        => $"Name: {texture.Name}, NewTexture: {texture.NewTexture.FormKey}, Index: {texture.Index}";
}
