using DreadsMashedPatch.Contexts;
using DreadsMashedPatch.Contexts.Interfaces;
using DreadsMashedPatch.PropertyHandlers.Armor;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;

namespace DreadsMashedPatch.PropertyHandlers.General;

/// <summary>
/// Coordinates model and object-bounds ownership. A model filename change takes the bounds from
/// the same override; bounds-only changes continue through the normal addition/reversion policy.
/// </summary>
public sealed class ModelBoundsHandler : IPropertyHandler
{
    private readonly IPropertyHandler _modelHandler;
    private readonly ObjectBoundsHandler _boundsHandler = new();
    private readonly Func<object?, object?> _geometryIdentity;

    public ModelBoundsHandler()
        : this(new ModelHandler(), GetModelGeometryIdentity)
    {
    }

    private ModelBoundsHandler(IPropertyHandler modelHandler, Func<object?, object?> geometryIdentity)
    {
        _modelHandler = modelHandler;
        _geometryIdentity = geometryIdentity;
    }

    public static ModelBoundsHandler ForArmorWorldModel() =>
        new(new WorldModelHandler(), GetArmorWorldModelGeometryIdentity);

    public string PropertyName => $"{_modelHandler.PropertyName}AndBounds";
    public bool RequiresFullLoadOrderProcessing => true;

    public object GetValue(IMajorRecordGetter record) =>
        new ModelBoundsValue(_modelHandler.GetValue(record), _boundsHandler.GetValue(record));

    public void SetValue(IMajorRecord record, object? value)
    {
        if (value is not ModelBoundsValue combined)
        {
            throw new ArgumentException($"Expected {nameof(ModelBoundsValue)}", nameof(value));
        }

        _modelHandler.SetValue(record, combined.Model);
        _boundsHandler.SetValue(record, combined.Bounds);
    }

    public bool AreValuesEqual(object? value1, object? value2)
    {
        if (value1 is not ModelBoundsValue left || value2 is not ModelBoundsValue right)
        {
            return value1 == null && value2 == null;
        }

        return _modelHandler.AreValuesEqual(left.Model, right.Model)
            && _boundsHandler.AreValuesEqual(left.Bounds, right.Bounds);
    }

    public IPropertyContext CreatePropertyContext() => new ModelBoundsPropertyContext();

    public void InitializeContext(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> originalContext,
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPropertyContext propertyContext)
    {
        var context = RequireContext(propertyContext);
        var original = (ModelBoundsValue)GetValue(originalContext.Record);
        context.OriginalModel = original.Model;
        context.ForwardModel = original.Model;
        context.ModelOwner = originalContext.ModKey.ToString();
        context.OriginalBounds = original.Bounds;
        context.ForwardBounds = original.Bounds;
        context.BoundsOwner = originalContext.ModKey.ToString();
        context.PreviousModel = original.Model;
        context.PreviousBounds = original.Bounds;
        context.IsResolved = false;
    }

    public void UpdatePropertyContext(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> recordContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
        IPropertyContext propertyContext)
    {
        var context = RequireContext(propertyContext);
        var current = (ModelBoundsValue)GetValue(recordContext.Record);
        var geometryChanged = !Equals(
            _geometryIdentity(context.PreviousModel),
            _geometryIdentity(current.Model));
        var boundsExplicitlyChanged = !_boundsHandler.AreValuesEqual(
            context.PreviousBounds,
            current.Bounds);

        var modelAccepted = TryUpdateValue(
            current.Model,
            context.OriginalModel,
            context.ForwardModel,
            context.ModelOwner,
            recordContext,
            state,
            _modelHandler.AreValuesEqual,
            out var nextModel,
            out var nextModelOwner);

        if (modelAccepted)
        {
            context.ForwardModel = nextModel;
            context.ModelOwner = nextModelOwner;
        }

        if (modelAccepted && geometryChanged)
        {
            context.ForwardBounds = current.Bounds;
            context.BoundsOwner = recordContext.ModKey.ToString();
            LogCollector.Add(PropertyName,
                $"[{PropertyName}] {recordContext.ModKey}: Model geometry changed; bounds ownership followed the model");
        }
        else if (boundsExplicitlyChanged && TryUpdateValue(
                     current.Bounds,
                     context.OriginalBounds,
                     context.ForwardBounds,
                     context.BoundsOwner,
                     recordContext,
                     state,
                     (left, right) => _boundsHandler.AreValuesEqual(
                         left as IObjectBoundsGetter,
                         right as IObjectBoundsGetter),
                     out var nextBounds,
                     out var nextBoundsOwner))
        {
            context.ForwardBounds = nextBounds as IObjectBoundsGetter;
            context.BoundsOwner = nextBoundsOwner;
        }

        context.PreviousModel = current.Model;
        context.PreviousBounds = current.Bounds;
    }

    public string FormatValue(object? value)
    {
        if (value is not ModelBoundsValue combined) return value?.ToString() ?? "null";
        return $"Model=({_modelHandler.FormatValue(combined.Model)}), Bounds=({_boundsHandler.FormatValue(combined.Bounds)})";
    }

    private static bool TryUpdateValue(
        object? current,
        object? original,
        object? forward,
        string forwardOwner,
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> recordContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
        Func<object?, object?, bool> equals,
        out object? next,
        out string nextOwner)
    {
        next = forward;
        nextOwner = forwardOwner;

        if (!equals(current, original) && !equals(current, forward))
        {
            next = current;
            nextOwner = recordContext.ModKey.ToString();
            return true;
        }

        if (!equals(current, original) || equals(current, forward))
        {
            return false;
        }

        var mod = state.LoadOrder[recordContext.ModKey].Mod;
        if (mod == null || !PatcherSettings.HasMasterOrVirtualMaster(mod, forwardOwner))
        {
            return false;
        }

        next = current;
        nextOwner = recordContext.ModKey.ToString();
        return true;
    }

    private static object? GetModelGeometryIdentity(object? value) => value is IModelGetter model
        ? AssetPathHelper.NormalizeForComparison(model.File.GivenPath).ToUpperInvariant()
        : null;

    private static object GetArmorWorldModelGeometryIdentity(object? value)
    {
        if (value is not IGenderedItemGetter<IArmorModelGetter?> models)
        {
            return new ValueTuple<string?, string?>(null, null);
        }

        return (
            Normalize(models.Male?.Model?.File.GivenPath),
            Normalize(models.Female?.Model?.File.GivenPath));
    }

    private static string? Normalize(string? path) => path == null
        ? null
        : AssetPathHelper.NormalizeForComparison(path).ToUpperInvariant();

    private static ModelBoundsPropertyContext RequireContext(IPropertyContext context) =>
        context as ModelBoundsPropertyContext
        ?? throw new InvalidOperationException($"Expected {nameof(ModelBoundsPropertyContext)}");
}
