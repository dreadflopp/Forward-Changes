using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.Contexts
{
    /// <summary>
    /// Represents a context for a simple property value.
    /// </summary>
    /// <typeparam name="T">The type of the property value</typeparam>
    public class SimplePropertyValueContext<T>(T? item, string ownerMod) : IPropertyValueContext<T>
    {
        public T? Value { get; set; } = item;
        public string OwnerMod { get; set; } = ownerMod;
    }
}