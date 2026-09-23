using DreadsMashedPatch.PropertyHandlers.General;
using Mutagen.Bethesda.Skyrim;

namespace DreadsMashedPatch.PropertyHandlers.PlacedObject
{
    /// <summary>
    /// Placed-object specialization of the shared cohesive Placement policy.
    /// </summary>
    public sealed class PlacementHandler
        : PlacementPropertyHandler<IPlacedObject, IPlacedObjectGetter>
    {
    }
}
