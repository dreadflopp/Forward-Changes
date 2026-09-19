using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.Contexts;

/// <summary>
/// Tracks nullable flag-field presence and each named flag bit independently.
/// </summary>
public sealed class NullableFlagPropertyContext<TFlag> : IPropertyContext
    where TFlag : struct, Enum
{
    public bool IsResolved { get; set; }

    public bool OriginalHasValue { get; set; }
    public bool ForwardHasValue { get; set; }
    public string PresenceOwnerMod { get; set; } = string.Empty;

    public List<FlagPropertyValueContext<TFlag>> OriginalFlagContexts { get; set; } = [];
    public List<FlagPropertyValueContext<TFlag>> ForwardFlagContexts { get; set; } = [];

    public object? GetForwardValue()
    {
        long bits = 0;
        foreach (var context in ForwardFlagContexts.Where(context => context.IsSet))
        {
            bits |= Convert.ToInt64(context.Flag);
        }

        // A retained bit necessarily requires the nullable field to be present,
        // even if a later attempted removal did not own that bit.
        if (!ForwardHasValue && bits == 0)
        {
            return null;
        }

        return (TFlag)Enum.ToObject(typeof(TFlag), bits);
    }
}
