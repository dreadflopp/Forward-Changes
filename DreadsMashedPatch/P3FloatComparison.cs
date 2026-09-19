using Noggog;

namespace ForwardChanges
{
    /// <summary>
    /// Shared P3Float equality using Noggog's FloatExt.EqualsWithin so we get consistent
    /// epsilon comparison and Infinity/NaN handling. Use this everywhere we compare P3Float
    /// (Position, Rotation, WorldMapCellOffset, etc.) so reversions are detected across mods.
    /// </summary>
    public static class P3FloatComparison
    {
        /// <summary>
        /// Same as Noggog's FloatExt.EqualsWithin default (1e-9).
        /// </summary>
        public const float DefaultEpsilon = 1E-09f;

        /// <summary>
        /// Epsilon for position. xEdit uses 6 decimals for position; 1e-6 matches that precision.
        /// </summary>
        public const float PositionEpsilon = 1E-06f;

        /// <summary>
        /// Epsilon for rotation. Stored in radians; xEdit shows 4 decimals in degrees.
        /// 0.0001° ≈ 1.75e-6 rad, so 1e-6 matches that precision.
        /// </summary>
        public const float RotationEpsilon = 1E-06f;

        /// <summary>
        /// Epsilon for float fields with 6 decimals (e.g. LightData in xEdit).
        /// </summary>
        public const float Float6DecimalsEpsilon = 1E-06f;

        /// <summary>
        /// Slightly looser (5 decimals) for float fields where we need reliable equality across mods,
        /// e.g. LightData so reverts (base vs later mod) are classified correctly.
        /// </summary>
        public const float Float5DecimalsEpsilon = 1E-05f;

        /// <summary>
        /// Returns true if each component of a and b is within the given tolerance (Noggog EqualsWithin).
        /// </summary>
        public static bool EqualsWithin(P3Float a, P3Float b, float within = DefaultEpsilon)
        {
            return a.X.EqualsWithin(b.X, within) &&
                   a.Y.EqualsWithin(b.Y, within) &&
                   a.Z.EqualsWithin(b.Z, within);
        }
    }
}
