namespace ForwardChanges.Contexts;

/// <summary>
/// A persistent row in the progressive alignment of an ordered list.
/// The representative is used only for alignment identity; the forwarded value
/// and its ownership remain in <see cref="ListPropertyValueContext{T}"/>.
/// </summary>
public sealed class ListAlignmentRow<T>(int id, T representative) where T : class
{
    public int Id { get; } = id;
    public T Representative { get; } = representative;
}
