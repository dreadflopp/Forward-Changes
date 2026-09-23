using DreadsMashedPatch.Contexts;
using DreadsMashedPatch.Contexts.Interfaces;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;

namespace DreadsMashedPatch.RecordHandlers;

/// <summary>
/// Keeps xEdit's safe undelete-and-disable state together without treating every
/// Initially Disabled reference as a UDR. The approved flag handler remains the
/// authority for whether the Initially Disabled bit wins.
/// </summary>
internal static class PlacedReferenceUdrCoordinator
{
    private const float UdrZ = -30000f;
    private const string PlacementProperty = "Placement";
    private const string EnableParentProperty = "EnableParent";
    private const string SkyrimFlagsProperty = "SkyrimMajorRecordFlags";

    public static PropertyForwardingCoordination Coordinate(
        IReadOnlyList<IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>> contexts,
        IReadOnlyDictionary<string, IPropertyContext> propertyContexts)
    {
        if (!propertyContexts.TryGetValue(SkyrimFlagsProperty, out var rawFlagContext)
            || rawFlagContext is not FlagPropertyContext<SkyrimMajorRecord.SkyrimMajorRecordFlag> flagContext)
        {
            return PropertyForwardingCoordination.None;
        }

        var initiallyDisabled = flagContext.GetFlagContext(
            SkyrimMajorRecord.SkyrimMajorRecordFlag.InitiallyDisabled);
        if (initiallyDisabled == null)
        {
            return PropertyForwardingCoordination.None;
        }

        var placedContexts = contexts
            .Where(context => context.Record is IPlacedGetter)
            .Select(context => (Context: context, Placed: (IPlacedGetter)context.Record))
            .ToArray();
        if (!placedContexts.Any(entry => IsSafeUdrState(entry.Placed)))
        {
            // Initially Disabled is also used legitimately without the xEdit safe-disable bundle.
            return PropertyForwardingCoordination.None;
        }

        var owner = placedContexts.FirstOrDefault(entry =>
            string.Equals(
                entry.Context.ModKey.ToString(),
                initiallyDisabled.OwnerMod,
                StringComparison.OrdinalIgnoreCase));
        if (owner.Context == null)
        {
            return PropertyForwardingCoordination.None;
        }

        if (initiallyDisabled.IsSet)
        {
            if (!IsSafeUdrState(owner.Placed))
            {
                // A normal Initially Disabled use owns the decision; do not impose UDR semantics.
                return PropertyForwardingCoordination.None;
            }
        }
        else if (HasUdrZ(owner.Placed.Placement))
        {
            // No coherent non-UDR placement exists in the flag-owning snapshot.
            return PropertyForwardingCoordination.None;
        }

        var stateDescription = initiallyDisabled.IsSet
            ? "selected the complete safe UDR bundle"
            : "selected the complete non-UDR restoration";
        return new PropertyForwardingCoordination(
            new Dictionary<string, object?>
            {
                [PlacementProperty] = owner.Placed.Placement,
                [EnableParentProperty] = owner.Placed.EnableParent
            },
            $"[UDR] {stateDescription} from {owner.Context.ModKey}; " +
            "Placement and EnableParent follow the Initially Disabled owner.");
    }

    internal static bool IsSafeUdrState(IPlacedGetter placed)
    {
        return placed.SkyrimMajorRecordFlags.HasFlag(
                SkyrimMajorRecord.SkyrimMajorRecordFlag.InitiallyDisabled)
            && HasUdrZ(placed.Placement)
            && IsPlayerOppositeEnableParent(placed.EnableParent);
    }

    private static bool HasUdrZ(IPlacementGetter? placement)
    {
        return placement != null
            && P3FloatComparison.EqualsAtDecimalPrecision(
                placement.Position.Z,
                UdrZ,
                P3FloatComparison.PositionDecimalPlaces);
    }

    private static bool IsPlayerOppositeEnableParent(IEnableParentGetter? enableParent)
    {
        return enableParent != null
            && enableParent.Reference.FormKey == Constants.Player.FormKey
            && enableParent.Flags.HasFlag(EnableParent.Flag.SetEnableStateToOppositeOfParent);
    }
}
