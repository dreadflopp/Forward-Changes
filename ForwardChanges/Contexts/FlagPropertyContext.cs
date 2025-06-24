using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.Contexts
{
    /// <summary>
    /// Represents the context for a flag property, managing collections of individual flag contexts.
    /// </summary>
    /// <typeparam name="TFlag">The flag enum type</typeparam>
    public class FlagPropertyContext<TFlag> : IPropertyContext where TFlag : struct, Enum
    {
        /// <summary>
        /// The original flag contexts from the base record
        /// </summary>
        public List<FlagPropertyValueContext<TFlag>> OriginalFlagContexts { get; set; } = new();

        /// <summary>
        /// The current forward flag contexts being proposed
        /// </summary>
        public List<FlagPropertyValueContext<TFlag>> ForwardFlagContexts { get; set; } = new();

        /// <summary>
        /// Whether this property context has been resolved
        /// </summary>
        public bool IsResolved { get; set; } = false;

        public FlagPropertyContext()
        {
        }

        /// <summary>
        /// Gets a flag context by the specific flag value
        /// </summary>
        /// <param name="flag">The flag to find</param>
        /// <param name="useForward">Whether to search in forward contexts (true) or original contexts (false)</param>
        /// <returns>The flag context if found, null otherwise</returns>
        public FlagPropertyValueContext<TFlag>? GetFlagContext(TFlag flag, bool useForward = true)
        {
            var contexts = useForward ? ForwardFlagContexts : OriginalFlagContexts;
            return contexts.FirstOrDefault(fc => fc.Flag.Equals(flag));
        }

        /// <summary>
        /// Adds or updates a flag context in the forward contexts
        /// </summary>
        /// <param name="flag">The flag</param>
        /// <param name="isSet">Whether the flag is set</param>
        /// <param name="ownerMod">The mod that owns this flag setting</param>
        public void SetFlagContext(TFlag flag, bool isSet, string ownerMod)
        {
            var existing = GetFlagContext(flag, true);
            if (existing != null)
            {
                existing.IsSet = isSet;
                existing.OwnerMod = ownerMod;
            }
            else
            {
                ForwardFlagContexts.Add(new FlagPropertyValueContext<TFlag>(flag, isSet, ownerMod));
            }
        }

        /// <summary>
        /// Removes a flag context from the forward contexts
        /// </summary>
        /// <param name="flag">The flag to remove</param>
        public void RemoveFlagContext(TFlag flag)
        {
            var existing = GetFlagContext(flag, true);
            if (existing != null)
            {
                ForwardFlagContexts.Remove(existing);
            }
        }

        /// <summary>
        /// Gets the combined flag value from all original contexts
        /// </summary>
        /// <returns>The combined flag value</returns>
        public TFlag GetCombinedOriginalValue()
        {
            TFlag result = default;
            foreach (var context in OriginalFlagContexts.Where(fc => fc.IsSet))
            {
                result = CombineFlags(result, context.Flag);
            }
            return result;
        }

        /// <summary>
        /// Combines two flag values using bitwise OR
        /// </summary>
        /// <param name="flags1">First flag value</param>
        /// <param name="flags2">Second flag value</param>
        /// <returns>Combined flag value</returns>
        private static TFlag CombineFlags(TFlag flags1, TFlag flags2)
        {
            // Use dynamic to handle the bitwise OR operation on enum types
            dynamic d1 = flags1;
            dynamic d2 = flags2;
            return (TFlag)(d1 | d2);
        }

        /// <summary>
        /// Gets the forward value as an object, or null if not available.
        /// For flag properties, returns the combined flag value from all forward contexts.
        /// </summary>
        /// <returns>The combined flag value as an object</returns>
        public object? GetForwardValue()
        {
            TFlag result = default;
            foreach (var context in ForwardFlagContexts)
            {
                if (context.IsSet)
                {
                    result = CombineFlags(result, context.Flag);
                }
            }
            return result;
        }
    }
}