using ForwardChanges.Enums;

namespace ForwardChanges
{
    /// <summary>
    /// Central location for patch behavior settings until they are replaced by user configuration.
    /// </summary>
    public static class PatcherSettings
    {
        public static readonly ProtectionForwardingPolicy ProtectionPolicy =
            ProtectionForwardingPolicy.PreferHigherWithAuthorizedDowngrades;

        public static readonly PerkForwardingPolicy PerkPolicy =
            PerkForwardingPolicy.AtomicOnCoupledPropertyChange;

        public static readonly QuestForwardingPolicy QuestPolicy =
            QuestForwardingPolicy.AtomicOnStructuralChange;

        public static readonly StoryManagerForwardingPolicy StoryManagerPolicy =
            StoryManagerForwardingPolicy.AtomicOnConfigurationChange;
    }
}
