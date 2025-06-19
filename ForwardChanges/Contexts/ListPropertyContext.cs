using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.Contexts
{
    /// <summary>
    /// Represents a context for a list property.
    /// </summary>
    /// <typeparam name="T">The type of the property value</typeparam>
    public class ListPropertyContext<T> : IPropertyContext
    {
        public bool IsResolved { get; set; }
        public List<ListPropertyValueContext<T>>? OriginalValueContexts { get; set; }
        public List<ListPropertyValueContext<T>>? ForwardValueContexts { get; set; }

        public ListPropertyContext()
        {
            OriginalValueContexts = null;
            ForwardValueContexts = null;
        }
            public ListPropertyContext(
                List<ListPropertyValueContext<T>>? originalValueContexts,
                List<ListPropertyValueContext<T>>? forwardValueContexts
            )
            {
            OriginalValueContexts = originalValueContexts;
            ForwardValueContexts = forwardValueContexts;
        }
    }
}