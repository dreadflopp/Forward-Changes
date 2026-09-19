using System.Globalization;
using ForwardChanges.PropertyHandlers.General;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    /// <summary>
    /// Forwards a placed object's complete placement while providing readable log output.
    /// </summary>
    public sealed class PlacementHandler
        : ComplexReflectionPropertyHandler<IPlacementGetter, IPlacedObject, IPlacedObjectGetter>
    {
        public PlacementHandler()
            : base("Placement")
        {
        }

        public override string FormatValue(object? value)
        {
            if (value is not IPlacementGetter placement)
            {
                return base.FormatValue(value);
            }

            return $"Position=({Format(placement.Position)}), Rotation=({Format(placement.Rotation)})";
        }

        private static string Format(P3Float value)
        {
            var x = value.X == 0f ? 0f : value.X;
            var y = value.Y == 0f ? 0f : value.Y;
            var z = value.Z == 0f ? 0f : value.Z;

            return string.Join(", ",
                x.ToString("F4", CultureInfo.InvariantCulture),
                y.ToString("F4", CultureInfo.InvariantCulture),
                z.ToString("F4", CultureInfo.InvariantCulture));
        }
    }
}
