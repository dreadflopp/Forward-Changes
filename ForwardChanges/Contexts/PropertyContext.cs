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
    ///     <item><description><see cref="LastChangedByMod"/>: The mod that last modified the property</description></item>
    /// </list>
    /// </remarks>
    public class PropertyContext
    {
        public bool IsResolved { get; set; } = false;
        public IItemContext<object?> OriginalValue { get; set; }
        public IItemContext<object?> ForwardValue { get; set; }
        //public string LastChangedByMod { get; set; } = string.Empty;

        public PropertyContext()
        {
            ForwardValue = new ItemContext<object?>(null, string.Empty);
            OriginalValue = new ItemContext<object?>(null, string.Empty);
        }
    }
}