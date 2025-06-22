namespace ForwardChanges.Contexts.Interfaces
{
    /// <summary>
    /// Represents a context for a property.
    /// </summary>
    public interface IPropertyContext
    {
        public bool IsResolved { get; set; }

        /// <summary>
        /// Gets the forward value as an object, or null if not available.
        /// For simple properties, returns the single value.
        /// For list properties, returns an IReadOnlyList&lt;object&gt;.
        /// </summary>
        object? GetForwardValue();
    }
}