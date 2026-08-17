using Mutagen.Bethesda.Skyrim;

namespace ForwardChanges.PropertyHandlers.General
{
    internal static class OwnerTargetUtility
    {
        public static OwnerTarget DeepCopy(IOwnerTargetGetter? owner)
        {
            return owner?.DeepCopy() ?? new UntypedOwner();
        }

        public static bool AreEqual(IOwnerTargetGetter? left, IOwnerTargetGetter? right)
        {
            if (left == null || right == null)
            {
                return left == null && right == null;
            }

            return OwnerTargetMixIn.Equals(left, right);
        }
    }
}
