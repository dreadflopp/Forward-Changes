using Noggog;

namespace DreadsMashedPatch
{
    /// <summary>
    /// Shared P3Float comparison helpers. Generic vector fields use epsilon comparison;
    /// placed-reference positions and rotations use their xEdit display semantics.
    /// </summary>
    public static class P3FloatComparison
    {
        /// <summary>Same as Noggog's FloatExt.EqualsWithin default (1e-9).</summary>
        public const float DefaultEpsilon = 1E-09f;

        /// <summary>Number of decimal places xEdit displays for positions.</summary>
        public const int PositionDecimalPlaces = 6;

        public const float PositionPrecision = 1E-06f;

        /// <summary>Number of decimal places xEdit displays for rotations after converting to degrees.</summary>
        public const int RotationDegreeDecimalPlaces = 4;

        public const double RotationDegreePrecision = 1E-04d;

        /// <summary>Epsilon for float fields with 6 decimals (for example, LightData in xEdit).</summary>
        public const float Float6DecimalsEpsilon = 1E-06f;

        /// <summary>
        /// Slightly looser (5 decimals) for float fields where we need reliable equality across mods,
        /// for example LightData so reverts are classified correctly.
        /// </summary>
        public const float Float5DecimalsEpsilon = 1E-05f;

        /// <summary>Returns true if each component is within the given tolerance.</summary>
        public static bool EqualsWithin(P3Float a, P3Float b, float within = DefaultEpsilon)
        {
            return a.X.EqualsWithin(b.X, within)
                && a.Y.EqualsWithin(b.Y, within)
                && a.Z.EqualsWithin(b.Z, within);
        }

        /// <summary>Compares positions within one unit of xEdit's six-decimal display precision.</summary>
        public static bool PositionsEqual(P3Float a, P3Float b)
        {
            return a.X.EqualsWithin(b.X, PositionPrecision)
                && a.Y.EqualsWithin(b.Y, PositionPrecision)
                && a.Z.EqualsWithin(b.Z, PositionPrecision);
        }

        /// <summary>
        /// Compares rotations as circular angles. Mutagen exposes the stored radians while xEdit
        /// normalizes them into [0, 360) degrees and displays four decimal places.
        /// </summary>
        public static bool RotationsEqual(P3Float a, P3Float b)
        {
            return CircularDegreesEqual(a.X, b.X)
                && CircularDegreesEqual(a.Y, b.Y)
                && CircularDegreesEqual(a.Z, b.Z);
        }

        private static bool CircularDegreesEqual(float aRadians, float bRadians)
        {
            if (float.IsNaN(aRadians) || float.IsNaN(bRadians))
            {
                return float.IsNaN(aRadians) && float.IsNaN(bRadians);
            }

            if (float.IsInfinity(aRadians) || float.IsInfinity(bRadians))
            {
                return aRadians.Equals(bRadians);
            }

            var difference = Math.Abs(NormalizeDegrees(aRadians) - NormalizeDegrees(bRadians));
            var circularDifference = Math.Min(difference, 360d - difference);
            return circularDifference <= RotationDegreePrecision;
        }

        public static bool EqualsAtDecimalPrecision(float a, float b, int decimalPlaces)
        {
            if (float.IsNaN(a) || float.IsNaN(b))
            {
                return float.IsNaN(a) && float.IsNaN(b);
            }

            if (float.IsInfinity(a) || float.IsInfinity(b))
            {
                return a.Equals(b);
            }

            return Math.Round(a, decimalPlaces, MidpointRounding.ToEven)
                == Math.Round(b, decimalPlaces, MidpointRounding.ToEven);
        }

        public static double NormalizedDegreesAtXEditPrecision(float radians)
        {
            if (!float.IsFinite(radians))
            {
                return radians;
            }

            var degrees = NormalizeDegrees(radians);
            var rounded = Math.Round(degrees, RotationDegreeDecimalPlaces, MidpointRounding.ToEven);
            return rounded == 360d || rounded == -0d ? 0d : rounded;
        }

        private static double NormalizeDegrees(float radians)
        {
            var degrees = radians * (180d / Math.PI);
            degrees %= 360d;
            if (degrees < 0d)
            {
                degrees += 360d;
            }

            return degrees;
        }
    }
}
