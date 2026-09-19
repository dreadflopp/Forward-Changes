namespace DreadsMashedPatch.Contexts
{
    /// <summary>
    /// Represents the context for a single integer flag value, tracking its state and ownership.
    /// </summary>
    public class IntFlagPropertyValueContext
    {
        /// <summary>
        /// The specific flag value (bit mask) this context represents
        /// </summary>
        public int Flag { get; set; }

        /// <summary>
        /// The name of this flag for display purposes
        /// </summary>
        public string FlagName { get; set; }

        /// <summary>
        /// Whether this flag is currently set (true) or cleared (false)
        /// </summary>
        public bool IsSet { get; set; }

        /// <summary>
        /// The mod that owns this flag setting
        /// </summary>
        public string OwnerMod { get; set; }

        public IntFlagPropertyValueContext(int flag, string flagName, bool isSet, string ownerMod)
        {
            Flag = flag;
            FlagName = flagName;
            IsSet = isSet;
            OwnerMod = ownerMod;
        }

        public override string ToString()
        {
            return $"{FlagName} (0x{Flag:X}) = {IsSet} (owned by {OwnerMod})";
        }
    }
}
