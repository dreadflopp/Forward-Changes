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

        public object? GetForwardValue()
        {
            if (ForwardValueContexts == null) return null;

            var activeItems = ForwardValueContexts
                .Where(i => !i.IsRemoved)
                .Select(i => (object)i.Value!)
                .ToList();

            if (activeItems.Count >= 4)
            {
                var preview = string.Join(", ", activeItems.Take(8).Select((o, i) => $"[{i}]{o?.ToString() ?? "null"}"));
                System.Console.WriteLine($"[DEBUG GetForwardValue] List count={activeItems.Count} order: {preview}");
            }

            // Return null for empty lists to match the expected type
            return activeItems.Count > 0 ? activeItems : null;
        }
    }
}