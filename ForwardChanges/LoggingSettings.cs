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
        private static DiagnosticsSettings _settings = new();

        public static bool DebugMode => _settings.DebugMode;

        // Debug mode is the gate for verbose diagnostics. Warnings remain visible
        // through LogCollector even when this evaluates to Summary.
        public static PatcherLogVerbosity Verbosity =>
            _settings.DebugMode ? _settings.Verbosity : PatcherLogVerbosity.Summary;

        public static bool EnableStartupDiagnostics =>
            _settings.DebugMode && _settings.EnableStartupDiagnostics;

        public static bool IncludeNoChangeDecisionsInDetailed =>
            _settings.DebugMode && _settings.IncludeNoChangeDecisionsInDetailed;

        public static int MaxValuePreviewLength => _settings.MaxValuePreviewLength;

        // Deep-dive selectors. Keep empty for normal runs.
        // Add xEdit record signatures like "QUST" to dive into that record family.
        public static IReadOnlySet<string> DeepDiveRecordSignatures => _settings.DeepDiveRecordSignatures;

        // Add exact FormKey strings like "050CED:Skyrim.esm".
        public static IReadOnlySet<string> DeepDiveFormKeys => _settings.DeepDiveFormKeys;

        // Add property names like "Responses" or "Conditions".
        // Empty means all properties for matched deep-dive records.
        public static IReadOnlySet<string> DeepDiveProperties => _settings.DeepDiveProperties;

        public static void Apply(DiagnosticsSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);
            _settings = settings.Copy();
        }

        public static bool IsDeepDiveRecord(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext)
        {
            if (!DebugMode)
            {
                return false;
            }

            var recordFormKey = winningContext.Record.FormKey;

            if (DeepDiveFormKeys.Any(configured => IsMatchingDeepDiveFormKey(configured, recordFormKey)))
            {
                return true;
            }

            var getterInterfaces = winningContext.Record.GetType()
                .GetInterfaces()
                .Where(i => i.Name.EndsWith("Getter", StringComparison.Ordinal)
                    && Program.SupportedRecordTypes.Contains(i));

            return getterInterfaces.Any(i =>
                DeepDiveRecordSignatures.Contains(RecordTypeCatalog.GetSignature(i)));
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

            return DeepDiveProperties.Count == 0
                || DeepDiveProperties.Contains(propertyName)
                || DeepDiveProperties.Any(selector => MatchesXEditFieldSelector(selector, propertyName));
        }

        private static bool MatchesXEditFieldSelector(string selector, string propertyName)
        {
            return selector.ToUpperInvariant() switch
            {
                "EDID" => propertyName.Equals("EditorID", StringComparison.OrdinalIgnoreCase),
                "FULL" => propertyName.Equals("Name", StringComparison.OrdinalIgnoreCase),
                "DESC" => propertyName.Equals("Description", StringComparison.OrdinalIgnoreCase),
                "KWDA" => propertyName.Equals("Keywords", StringComparison.OrdinalIgnoreCase),
                "VMAD" => propertyName.Equals("VirtualMachineAdapter", StringComparison.OrdinalIgnoreCase),
                "CTDA" => propertyName.Contains("Condition", StringComparison.OrdinalIgnoreCase),
                _ => false
            };
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
