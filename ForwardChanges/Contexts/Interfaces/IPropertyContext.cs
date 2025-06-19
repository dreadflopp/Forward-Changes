namespace ForwardChanges.Contexts.Interfaces
{
    /// <summary>
    /// Represents a context for a property.
    /// <typeparam name="T">The type of value this property context manages</typeparam>
    public interface IPropertyContext
    {
        public bool IsResolved { get; set; }
    }
}