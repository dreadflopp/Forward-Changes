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

    /// <summary>
    /// Generic version of PropertyContext that provides type safety for property handlers.
    /// </summary>
    /// <typeparam name="T">The type of value this property context manages</typeparam>
    public class PropertyContext<T> : PropertyContext
    {
        public new IItemContext<T>? OriginalValue
        {
            get => (IItemContext<T>?)base.OriginalValue;
            set => base.OriginalValue = value;
        }
        public new IItemContext<T>? ForwardValue
        {
            get => (IItemContext<T>?)base.ForwardValue;
            set => base.ForwardValue = value;
        }
    }
}