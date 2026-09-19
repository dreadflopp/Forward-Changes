using DreadsMashedPatch.Enums;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace DreadsMashedPatch
{
    /// <summary>
    /// Runtime view of the settings selected by the standalone application.
    /// </summary>
    public static class PatcherSettings
    {
        private static PatcherConfiguration _current = new();
        private static HashSet<ModKey> _creationClubPlugins = [];
        private static Dictionary<ModKey, HashSet<ModKey>> _virtualMastersByTarget = [];

        public static bool TreatCreationClubAsVanilla => _current.Forwarding.TreatCreationClubAsVanilla;

        public static IReadOnlySet<ModKey> CreationClubPlugins => _creationClubPlugins;

        public static int CompatibilityRuleCount => _current.CompatibilityRules.Count;

        public static int CompatibilityTargetCount => _virtualMastersByTarget.Count;

        public static ProtectionForwardingPolicy ProtectionPolicy => _current.Forwarding.ProtectionPolicy;

        public static PerkForwardingPolicy PerkPolicy => _current.Forwarding.PerkPolicy;

        public static QuestForwardingPolicy QuestPolicy => _current.Forwarding.QuestPolicy;

        public static StoryManagerForwardingPolicy StoryManagerPolicy => _current.Forwarding.StoryManagerPolicy;

        public static void Apply(PatcherConfiguration configuration, IEnumerable<ModKey>? creationClubPlugins = null)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            _current = configuration.Copy();
            _creationClubPlugins = creationClubPlugins?.ToHashSet() ?? [];
            _virtualMastersByTarget = BuildVirtualMasterLookup(_current.CompatibilityRules);
            LoggingSettings.Apply(_current.Diagnostics);
        }

        public static bool HasMasterOrVirtualMaster(ISkyrimModGetter mod, string? ownerMod)
        {
            if (ownerMod is null || !ModKey.TryFromFileName(ownerMod, out var ownerKey))
            {
                return false;
            }

            return mod.MasterReferences.Any(master => master.Master.Equals(ownerKey))
                   || HasVirtualMaster(mod.ModKey, ownerKey);
        }

        public static bool HasMasterOrVirtualMaster(
            ModKey currentMod,
            IEnumerable<ModKey> actualMasters,
            ModKey ownerMod) =>
            actualMasters.Contains(ownerMod) || HasVirtualMaster(currentMod, ownerMod);

        private static bool HasVirtualMaster(ModKey targetMod, ModKey masterMod) =>
            _virtualMastersByTarget.TryGetValue(targetMod, out var virtualMasters)
            && virtualMasters.Contains(masterMod);

        private static Dictionary<ModKey, HashSet<ModKey>> BuildVirtualMasterLookup(
            IEnumerable<VirtualMasterRule> rules)
        {
            var lookup = new Dictionary<ModKey, HashSet<ModKey>>();
            foreach (var rule in rules)
            {
                if (!ModKey.TryFromFileName(rule.InjectedMaster, out var injectedMaster))
                {
                    continue;
                }

                foreach (var targetName in rule.TargetMods)
                {
                    if (!ModKey.TryFromFileName(targetName, out var targetMod))
                    {
                        continue;
                    }

                    if (!lookup.TryGetValue(targetMod, out var virtualMasters))
                    {
                        virtualMasters = [];
                        lookup[targetMod] = virtualMasters;
                    }

                    virtualMasters.Add(injectedMaster);
                }
            }

            return lookup;
        }

        public static bool IsRecordTypeEnabled(Type recordType)
        {
            ArgumentNullException.ThrowIfNull(recordType);
            return !_current.DisabledRecordTypes.Contains(recordType.FullName ?? recordType.Name);
        }
    }
}
