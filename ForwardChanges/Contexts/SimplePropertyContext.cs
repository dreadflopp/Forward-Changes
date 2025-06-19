using ForwardChanges.Contexts.Interfaces;

/// <summary>
/// Represents a context for a simple property.
/// </summary>
/// <typeparam name="T">The type of value this property context manages</typeparam>
namespace ForwardChanges.Contexts
{
    public class SimplePropertyContext<T> : IPropertyContext<T>
    {
        public bool IsResolved { get; set; }
        public SimplePropertyValueContext<T>? OriginalValueContext { get; set; }
        public SimplePropertyValueContext<T>? ForwardValueContext { get; set; }

        public SimplePropertyContext()
        {
            OriginalValueContext = null;
            ForwardValueContext = null;
        }

        public SimplePropertyContext(SimplePropertyValueContext<T>? originalValueContext, SimplePropertyValueContext<T>? forwardValueContext)
        {
            OriginalValueContext = originalValueContext;
            ForwardValueContext = forwardValueContext;
        }
    }
}