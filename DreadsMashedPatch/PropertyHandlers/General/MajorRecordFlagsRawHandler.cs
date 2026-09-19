using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.Contexts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.Contexts.Interfaces;

namespace DreadsMashedPatch.PropertyHandlers.General
{
    /// <summary>
    /// Handler for base TES5 MajorRecordFlagsRaw (integer flags like Persistent, Sky Marker, etc.)
    /// These are separate from SkyrimMajorRecordFlags which are Skyrim-specific.
    /// </summary>
    public class MajorRecordFlagsRawHandler : IPropertyHandler<int>
    {
        public string PropertyName => "MajorRecordFlagsRaw";
        public bool RequiresFullLoadOrderProcessing => true;

        /// <summary>
        /// Base TES5 record flags that are stored in MajorRecordFlagsRaw.
        /// These are the flags that appear in xEdit but are not in SkyrimMajorRecordFlag enum.
        /// 
        /// Bit flag reference:
        /// - 0x0200 (Bit 9): Sky Marker / Hidden From Local Map (varies by record type)
        /// - 0x0400 (Bit 10): Persistent Reference
        /// - 0x4000 (Bit 14): Must Update Anims
        /// - 0x10000 (Bit 16): Is Full LOD
        /// - 0x2000000 (Bit 25): No AI Acquire
        /// - 0x10000000 (Bit 28): Reflected By Auto Water
        /// - 0x20000000 (Bit 29): Don't Havok Settle
        /// - 0x40000000 (Bit 30): Not Respawns
        /// 
        /// Reference: xEdit source code (wbDefinitionsTES5.pas) and TES5 record format documentation.
        /// See also: https://github.com/wrye-bash/wrye-bash/wiki/[dev]-Record-Header-Flags
        /// </summary>
        private static readonly IReadOnlyDictionary<int, string> BaseFlags = new Dictionary<int, string>
        {
            { 0x0200, "Sky Marker" },              // Hidden From Local Map / Sky Marker
            { 0x0400, "Persistent" },              // Persistent Reference
            { 0x4000, "Must Update Anims" },       // Must Update Anims
            { 0x10000, "Is Full LOD" },            // Is Full LOD
            { 0x2000000, "No AI Acquire" },        // No AI Acquire
            { 0x10000000, "Reflected By Auto Water" }, // Reflected By Auto Water
            { 0x20000000, "Don't Havok Settle" },  // Don't Havok Settle
            { 0x40000000, "Not Respawns" }         // Not Respawns
        };

        private readonly IReadOnlyDictionary<int, string> _flagDefinitions;
        private readonly int _ownedMask;

        public MajorRecordFlagsRawHandler(params Type[] additionalFlagEnumTypes)
        {
            var flagDefinitions = new Dictionary<int, string>(BaseFlags);
            foreach (var enumType in additionalFlagEnumTypes)
            {
                if (!enumType.IsEnum)
                {
                    throw new ArgumentException($"{enumType.Name} must be an enum type.", nameof(additionalFlagEnumTypes));
                }

                foreach (var value in Enum.GetValues(enumType))
                {
                    var flag = Convert.ToInt32(value);
                    if (flag != 0)
                    {
                        flagDefinitions[flag] = Enum.GetName(enumType, value) ?? $"0x{flag:X8}";
                    }
                }
            }

            _flagDefinitions = flagDefinitions;
            _ownedMask = flagDefinitions.Keys.Aggregate(0, (mask, flag) => mask | flag);
        }

        public void SetValue(IMajorRecord record, int value)
        {
            // This handler owns only the configured bits. Preserve any bit which is
            // unknown to this record's composite header view.
            record.MajorRecordFlagsRaw =
                (record.MajorRecordFlagsRaw & ~_ownedMask) |
                (value & _ownedMask);
        }

        public int GetValue(IMajorRecordGetter record)
        {
            return record.MajorRecordFlagsRaw;
        }

        public bool AreValuesEqual(int value1, int value2)
        {
            // Only compare the flags this handler owns.
            foreach (var flag in _flagDefinitions.Keys)
            {
                var flag1Set = (value1 & flag) == flag;
                var flag2Set = (value2 & flag) == flag;

                if (flag1Set != flag2Set)
                {
                    return false;
                }
            }

            return true;
        }

        public string FormatValue(object? value)
        {
            if (value is not int flags)
            {
                return value?.ToString() ?? "null";
            }

            var setFlags = _flagDefinitions
                .Where(kvp => (flags & kvp.Key) == kvp.Key)
                .Select(kvp => kvp.Value)
                .ToList();

            if (setFlags.Count == 0)
            {
                return "None";
            }

            return string.Join(", ", setFlags);
        }

        public IPropertyContext CreatePropertyContext()
        {
            return new IntFlagPropertyContext();
        }

        public void InitializeContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> originalContext,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPropertyContext propertyContext)
        {
            if (propertyContext is not IntFlagPropertyContext intFlagPropertyContext)
            {
                throw new InvalidOperationException($"Error: Property context is not an IntFlagPropertyContext for {PropertyName}");
            }

            var originalFlags = GetValue(originalContext.Record);
            var winningFlags = GetValue(winningContext.Record);

            // Initialize original flag contexts
            intFlagPropertyContext.OriginalFlagContexts = _flagDefinitions
                .Select(kvp => new IntFlagPropertyValueContext(
                    kvp.Key,
                    kvp.Value,
                    (originalFlags & kvp.Key) == kvp.Key,
                    originalContext.ModKey.ToString()))
                .ToList();

            // Initialize forward flag contexts with original values (will be updated as we process mods through load order)
            intFlagPropertyContext.ForwardFlagContexts = _flagDefinitions
                .Select(kvp => new IntFlagPropertyValueContext(
                    kvp.Key,
                    kvp.Value,
                    (originalFlags & kvp.Key) == kvp.Key,
                    originalContext.ModKey.ToString()))
                .ToList();

            intFlagPropertyContext.IsResolved = false;
        }

        public void UpdatePropertyContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            IPropertyContext propertyContext)
        {
            LogCollector.Add(PropertyName, $"[{PropertyName}] Processing mod: {context.ModKey}");

            if (context == null)
            {
                Console.WriteLine($"Error: Context is null for {PropertyName}");
                return;
            }

            if (propertyContext is not IntFlagPropertyContext intFlagPropertyContext)
            {
                Console.WriteLine($"Error: Property context is not an IntFlagPropertyContext for {PropertyName}");
                return;
            }

            var forwardFlagContexts = intFlagPropertyContext.ForwardFlagContexts;
            if (forwardFlagContexts == null)
            {
                Console.WriteLine($"Error: Property context is not properly initialized for {PropertyName}");
                return;
            }

            var recordMod = state.LoadOrder[context.ModKey].Mod;
            if (recordMod == null)
            {
                Console.WriteLine($"Error: Record mod is null for {PropertyName}");
                return;
            }

            var recordFlags = GetValue(context.Record);

            // Process each individual flag
            foreach (var flagDef in _flagDefinitions)
            {
                var flagValue = flagDef.Key;
                var flagName = flagDef.Value;
                var isFlagSetInRecord = (recordFlags & flagValue) == flagValue;
                var existingFlagContext = forwardFlagContexts.FirstOrDefault(fc => fc.Flag == flagValue);

                if (existingFlagContext == null)
                {
                    // New flag context - add it
                    var newFlagContext = new IntFlagPropertyValueContext(flagValue, flagName, isFlagSetInRecord, context.ModKey.ToString());
                    forwardFlagContexts.Add(newFlagContext);
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Adding flag {flagName} = {isFlagSetInRecord} Success");
                }
                else
                {
                    // Get original flag value for comparison
                    var originalFlagContext = intFlagPropertyContext.OriginalFlagContexts.FirstOrDefault(fc => fc.Flag == flagValue);
                    var originalValue = originalFlagContext?.IsSet ?? false;

                    if (isFlagSetInRecord != existingFlagContext.IsSet)
                    {
                        if (isFlagSetInRecord != originalValue)
                        {
                            // Change/Addition: Different from original - always allowed
                            var oldState = existingFlagContext.IsSet;
                            existingFlagContext.IsSet = isFlagSetInRecord;
                            existingFlagContext.OwnerMod = context.ModKey.ToString();
                            LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Change flag {flagName}: {originalValue} -> {isFlagSetInRecord} Success");
                        }
                        else
                        {
                            // Reversion: Same as original, different from current - check permission
                            var canRevert = PatcherSettings.HasMasterOrVirtualMaster(
                                recordMod,
                                existingFlagContext.OwnerMod);
                            if (canRevert)
                            {
                                var oldState = existingFlagContext.IsSet;
                                existingFlagContext.IsSet = isFlagSetInRecord;
                                existingFlagContext.OwnerMod = context.ModKey.ToString();
                                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Reversion flag {flagName}: {oldState} -> {isFlagSetInRecord} Success");
                            }
                            else
                            {
                                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Reversion flag {flagName}: {existingFlagContext.IsSet} -> {isFlagSetInRecord} Permission denied");
                            }
                        }
                    }
                    else
                    {
                        // Values match forward context - no change needed
                        LogCollector.Add(PropertyName, $"[{PropertyName}] [{context.ModKey}] No change needed for flag: {flagName}");
                    }
                }
            }

            // Update the state
            intFlagPropertyContext.ForwardFlagContexts = forwardFlagContexts;
        }

        // Non-generic interface implementations
        void IPropertyHandler.SetValue(IMajorRecord record, object? value)
        {
            SetValue(record, (int)value!);
        }

        object? IPropertyHandler.GetValue(IMajorRecordGetter record)
        {
            return GetValue(record);
        }

        bool IPropertyHandler.AreValuesEqual(object? value1, object? value2)
        {
            return AreValuesEqual((int)value1!, (int)value2!);
        }
    }
}
