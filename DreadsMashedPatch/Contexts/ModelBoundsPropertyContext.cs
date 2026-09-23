using DreadsMashedPatch.Contexts.Interfaces;
using Mutagen.Bethesda.Skyrim;

namespace DreadsMashedPatch.Contexts;

public sealed class ModelBoundsPropertyContext : IPropertyContext
{
    public bool IsResolved { get; set; }
    public object? OriginalModel { get; set; }
    public object? ForwardModel { get; set; }
    public string ModelOwner { get; set; } = string.Empty;
    public IObjectBoundsGetter? OriginalBounds { get; set; }
    public IObjectBoundsGetter? ForwardBounds { get; set; }
    public string BoundsOwner { get; set; } = string.Empty;
    public object? PreviousModel { get; set; }
    public IObjectBoundsGetter? PreviousBounds { get; set; }

    public object GetForwardValue() => new ModelBoundsValue(ForwardModel, ForwardBounds);
}

public sealed record ModelBoundsValue(object? Model, IObjectBoundsGetter? Bounds);
