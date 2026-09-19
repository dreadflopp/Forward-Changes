namespace DreadsMashedPatch
{
    public static class StringComparisonHelper
    {
        public static string NormalizeForComparison(string? value, bool trimTrailingWhitespace = true)
        {
            var text = value ?? string.Empty;
            return trimTrailingWhitespace ? text.TrimEnd() : text;
        }

        public static bool EqualsNormalized(
            string? left,
            string? right,
            bool trimTrailingWhitespace = true,
            StringComparison comparison = StringComparison.Ordinal)
        {
            return string.Equals(
                NormalizeForComparison(left, trimTrailingWhitespace),
                NormalizeForComparison(right, trimTrailingWhitespace),
                comparison);
        }
    }
}