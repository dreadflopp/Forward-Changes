using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.Contexts
{
    /// <summary>
    /// Represents the context of a property.
    /// </summary>
    /// <remarks>
    /// Contains metadata about the property's context in the mod load order:
    /// <list type="bullet">
    ///     <item><description><see cref="IsResolved"/>: Indicates if the property has been resolved</description></item>
    ///     <item><description><see cref="OriginalValue"/>: The original value of the property</description></item>
    ///     <item><description><see cref="ForwardValue"/>: The final value of the property</description></item>
    /// </list>
    /// </remarks>
    public class PropertyContext
    {
        public bool IsResolved { get; set; } = false;
        public object? OriginalValue { get; set; }  // Either ItemContext<T> or List<ListItemContext<T>>
        public object? ForwardValue { get; set; }   // Either ItemContext<T> or List<ListItemContext<T>>

        public PropertyContext()
        {
            OriginalValue = null;
            ForwardValue = null;
        }
    }
}