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
        private static HashSet<ModKey> _ignoredMods = [];
        private static Dictionary<ModKey, int> _alwaysWinningModPriorities = [];
        private static Dictionary<ModKey, HashSet<ModKey>> _virtualMastersByTarget = [];
        private static HashSet<FormKey> _vanillaWeaponTypeKeywords = [];

        public static bool TreatCreationClubAsVanilla => _current.Forwarding.TreatCreationClubAsVanilla;

        public static bool EnforceSingleVanillaWeaponTypeKeyword =>
            _current.Forwarding.EnforceSingleVanillaWeaponTypeKeyword;

        public static IReadOnlySet<FormKey> VanillaWeaponTypeKeywords => _vanillaWeaponTypeKeywords;

        public static EditorIdForwardingPolicy EditorIdPolicy => _current.Forwarding.EditorIdPolicy;

        public static IReadOnlySet<ModKey> CreationClubPlugins => _creationClubPlugins;

        public static int IgnoredModCount => _ignoredMods.Count;

        public static int AlwaysWinningModCount => _alwaysWinningModPriorities.Count;

        public static int CompatibilityRuleCount => _current.CompatibilityRules.Count;

        public static int CompatibilityTargetCount => _virtualMastersByTarget.Count;

        public static ProtectionForwardingPolicy ProtectionPolicy => _current.Forwarding.ProtectionPolicy;

        public static void Apply(PatcherConfiguration configuration, IEnumerable<ModKey>? creationClubPlugins = null)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            _current = configuration.Copy();
            _creationClubPlugins = creationClubPlugins?.ToHashSet() ?? [];
            _ignoredMods = _current.IgnoredMods
                .Select(name => ModKey.TryFromFileName(name, out var modKey) ? modKey : (ModKey?)null)
                .Where(modKey => modKey.HasValue)
                .Select(modKey => modKey!.Value)
                .ToHashSet();
            _alwaysWinningModPriorities = _current.AlwaysWinningMods
                .Select((name, priority) =>
                    ModKey.TryFromFileName(name, out var modKey)
                        ? (ModKey: (ModKey?)modKey, Priority: priority)
                        : (ModKey: (ModKey?)null, Priority: priority))
                .Where(entry => entry.ModKey.HasValue)
                .ToDictionary(entry => entry.ModKey!.Value, entry => entry.Priority);
            _virtualMastersByTarget = BuildVirtualMasterLookup(_current.CompatibilityRules);
            _vanillaWeaponTypeKeywords = _current.Forwarding.VanillaWeaponTypeKeywords
                .Select(value => FormKey.TryFactory(value.AsSpan(), out var formKey)
                    ? formKey
                    : (FormKey?)null)
                .Where(formKey => formKey.HasValue)
                .Select(formKey => formKey!.Value)
                .ToHashSet();
            LoggingSettings.Apply(_current.Diagnostics);
        }

        public static bool IsIgnoredMod(ModKey modKey) => _ignoredMods.Contains(modKey);

        public static bool IsAlwaysWinningMod(ModKey modKey) =>
            _alwaysWinningModPriorities.ContainsKey(modKey);

        public static int GetAlwaysWinningPriority(ModKey modKey) =>
            _alwaysWinningModPriorities.GetValueOrDefault(modKey, -1);

        internal static TContext? SelectAlwaysWinningContext<TContext>(
            IEnumerable<TContext> contexts,
            Func<TContext, ModKey> modKeySelector)
            where TContext : class
        {
            TContext? selected = null;
            var selectedPriority = -1;
            foreach (var context in contexts)
            {
                var priority = GetAlwaysWinningPriority(modKeySelector(context));
                if (priority <= selectedPriority)
                {
                    continue;
                }

                selected = context;
                selectedPriority = priority;
            }

            return selected;
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
