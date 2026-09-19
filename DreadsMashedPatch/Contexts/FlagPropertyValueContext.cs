using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.Contexts
{
    /// <summary>
    /// Represents the context for a single flag value, tracking its state and ownership.
    /// </summary>
    /// <typeparam name="TFlag">The flag enum type</typeparam>
    public class FlagPropertyValueContext<TFlag> : IPropertyValueContext<TFlag>
    {
        /// <summary>
        /// The specific flag this context represents
        /// </summary>
        public TFlag Flag { get; set; }

        /// <summary>
        /// Whether this flag is currently set (true) or cleared (false)
        /// </summary>
        public bool IsSet { get; set; }

        /// <summary>
        /// The mod that owns this flag setting
        /// </summary>
        public string OwnerMod { get; set; }

        /// <summary>
        /// The current value of the flag (derived from IsSet)
        /// </summary>
        public TFlag Value => IsSet ? Flag : (TFlag)(object)0;

        public FlagPropertyValueContext(TFlag flag, bool isSet, string ownerMod)
        {
            Flag = flag;
            IsSet = isSet;
            OwnerMod = ownerMod;
        }

        public override string ToString()
        {
            return $"{Flag} = {IsSet} (owned by {OwnerMod})";
        }
    }
}