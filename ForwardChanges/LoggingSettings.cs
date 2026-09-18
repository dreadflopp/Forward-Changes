using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;

namespace ForwardChanges
{
    public enum PatcherLogVerbosity
    {
        Summary,
        ContextChanges,
        Detailed
    }

    public static class LoggingSettings
    {
        // Global verbosity for regular runs.
        // ContextChanges is intended for normal troubleshooting: it shows the
        // complete override chain only for properties which vary or are forwarded.
        public const PatcherLogVerbosity Verbosity = PatcherLogVerbosity.ContextChanges;

        // Enables startup type dumps and other one-off diagnostics.
        public static readonly bool EnableStartupDiagnostics = false;

        // When Detailed is enabled, include unchanged property decision blocks as well.
        public const bool IncludeNoChangeDecisionsInDetailed = true;

        // Truncate long values in non-deep runs to keep logs readable and smaller.
        public const int MaxValuePreviewLength = 240;

        // Deep-dive selectors. Keep empty for normal runs.
        // Add interface names like "IQuestGetter" to dive into that record family.
        public static readonly HashSet<string> DeepDiveRecordTypes = new(StringComparer.OrdinalIgnoreCase)
        {
        };

        // Add exact FormKey strings like "050CED:Skyrim.esm".
        public static readonly HashSet<string> DeepDiveFormKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "00285D:Update.esm"
        };

        // Add property names like "Responses" or "Conditions".
        // Empty means all properties for matched deep-dive records.
        public static readonly HashSet<string> DeepDiveProperties = new(StringComparer.OrdinalIgnoreCase)
        {
        };

        public static bool IsDeepDiveRecord(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext)
        {
            var recordFormKey = winningContext.Record.FormKey;

            if (DeepDiveFormKeys.Any(configured => IsMatchingDeepDiveFormKey(configured, recordFormKey)))
            {
                return true;
            }

            var getterInterfaces = winningContext.Record.GetType()
                .GetInterfaces()
                .Where(i => i.Name.EndsWith("Getter", StringComparison.Ordinal));

            return getterInterfaces.Any(i => DeepDiveRecordTypes.Contains(i.Name));
        }

        private static bool IsMatchingDeepDiveFormKey(string configuredValue, FormKey recordFormKey)
        {
            if (string.IsNullOrWhiteSpace(configuredValue))
            {
                return false;
            }

            var candidate = configuredValue.Trim();

            // Preferred format: 000000:SomeMod.esp
            if (FormKey.TryFactory(candidate.AsSpan(), out var configuredFormKey))
            {
                return configuredFormKey == recordFormKey;
            }

            // Support bare hex FormID selectors like 000D74.
            if (uint.TryParse(candidate, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var configuredId))
            {
                return (configuredId & 0xFFFFFF) == recordFormKey.ID;
            }

            // Last-resort textual compare for unusual formatting.
            return string.Equals(candidate, recordFormKey.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public static bool ShouldLogProperty(string propertyName, bool deepDiveRecord)
        {
            if (!deepDiveRecord)
            {
                return true;
            }

            return DeepDiveProperties.Count == 0 || DeepDiveProperties.Contains(propertyName);
        }

        public static string ForLog(string? value, bool deepDiveRecord)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value ?? "null";
            }

            if (deepDiveRecord || value.Length <= MaxValuePreviewLength)
            {
                return value;
            }

            return value.Substring(0, MaxValuePreviewLength) + "... (truncated)";
        }
    }
}
