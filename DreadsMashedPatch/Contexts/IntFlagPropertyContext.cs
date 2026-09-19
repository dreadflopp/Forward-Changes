using System.Linq;
using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.Contexts
{
    /// <summary>
    /// Represents the context for an integer flag property, managing collections of individual flag contexts.
    /// </summary>
    public class IntFlagPropertyContext : IPropertyContext
    {
        /// <summary>
        /// The original flag contexts from the base record
        /// </summary>
        public List<IntFlagPropertyValueContext> OriginalFlagContexts { get; set; } = new();

        /// <summary>
        /// The current forward flag contexts being proposed
        /// </summary>
        public List<IntFlagPropertyValueContext> ForwardFlagContexts { get; set; } = new();

        /// <summary>
        /// Whether this property context has been resolved
        /// </summary>
        public bool IsResolved { get; set; } = false;

        public IntFlagPropertyContext()
        {
        }

        /// <summary>
        /// Gets a flag context by the specific flag value
        /// </summary>
        /// <param name="flag">The flag value to find</param>
        /// <param name="useForward">Whether to search in forward contexts (true) or original contexts (false)</param>
        /// <returns>The flag context if found, null otherwise</returns>
        public IntFlagPropertyValueContext? GetFlagContext(int flag, bool useForward = true)
        {
            var contexts = useForward ? ForwardFlagContexts : OriginalFlagContexts;
            return contexts.FirstOrDefault(fc => fc.Flag == flag);
        }

        /// <summary>
        /// Gets the combined flag value from all original contexts
        /// </summary>
        /// <returns>The combined flag value</returns>
        public int GetCombinedOriginalValue()
        {
            int result = 0;
            foreach (var context in OriginalFlagContexts.Where(fc => fc.IsSet))
            {
                result |= context.Flag;
            }
            return result;
        }

        /// <summary>
        /// Gets the forward value as an object, or null if not available.
        /// For flag properties, returns the combined flag value from all forward contexts.
        /// </summary>
        /// <returns>The combined flag value as an object</returns>
        public object? GetForwardValue()
        {
            int result = 0;
            foreach (var context in ForwardFlagContexts)
            {
                if (context.IsSet)
                {
                    result |= context.Flag;
                }
            }
            return result;
        }
    }
}
