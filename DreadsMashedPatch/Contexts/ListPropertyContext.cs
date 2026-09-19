using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.Contexts
{
    /// <summary>
    /// Represents a context for a list property.
    /// </summary>
    /// <typeparam name="T">The type of the property value</typeparam>
    public class ListPropertyContext<T> : IPropertyContext where T : class
    {
        public bool IsResolved { get; set; }
        public List<ListPropertyValueContext<T>>? OriginalValueContexts { get; set; }
        public List<ListPropertyValueContext<T>>? ForwardValueContexts { get; set; }
        public List<ListAlignmentRow<T>> AlignmentRows { get; set; } = [];
        public int NextAlignmentRowId { get; set; }
        public bool CanBeNull { get; set; }
        public bool OriginalIsNull { get; set; }
        public bool ForwardIsNull { get; set; }
        public string? ForwardPresenceOwnerMod { get; set; }

        public ListPropertyContext()
        {
            OriginalValueContexts = null;
            ForwardValueContexts = null;
        }
        public ListPropertyContext(
            List<ListPropertyValueContext<T>>? originalValueContexts,
            List<ListPropertyValueContext<T>>? forwardValueContexts,
            bool canBeNull = false,
            bool originalIsNull = false,
            bool forwardIsNull = false,
            string? forwardPresenceOwnerMod = null
        )
        {
            OriginalValueContexts = originalValueContexts;
            ForwardValueContexts = forwardValueContexts;
            CanBeNull = canBeNull;
            OriginalIsNull = originalIsNull;
            ForwardIsNull = forwardIsNull;
            ForwardPresenceOwnerMod = forwardPresenceOwnerMod;
        }

        public object? GetForwardValue()
        {
            if (ForwardValueContexts == null) return null;

            var activeItems = ForwardValueContexts
                .Where(i => !i.IsRemoved)
                .Select(i => (object)i.Value!)
                .ToList();

            if (activeItems.Count > 0)
            {
                return activeItems;
            }

            return ForwardIsNull ? null : activeItems;
        }
    }
}
