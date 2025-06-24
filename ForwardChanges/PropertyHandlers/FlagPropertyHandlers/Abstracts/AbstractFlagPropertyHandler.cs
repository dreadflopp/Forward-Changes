using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.PropertyHandlers.FlagPropertyHandlers.Abstracts
{
    public abstract class AbstractFlagPropertyHandler<TFlag> : IPropertyHandler<TFlag> where TFlag : struct, Enum
    {
        public abstract string PropertyName { get; }
        public bool IsListHandler => false;

        public abstract void SetValue(IMajorRecord record, TFlag value);
        public abstract TFlag GetValue(IMajorRecordGetter record);

        /// <summary>
        /// Gets all possible flags for this flag enum type
        /// </summary>
        /// <returns>Array of all possible flag values</returns>
        protected abstract TFlag[] GetAllFlags();

        /// <summary>
        /// Checks if a specific flag is set in the given flag value
        /// </summary>
        /// <param name="flags">The combined flag value</param>
        /// <param name="flag">The specific flag to check</param>
        /// <returns>True if the flag is set, false otherwise</returns>
        protected abstract bool IsFlagSet(TFlag flags, TFlag flag);

        /// <summary>
        /// Sets or clears a specific flag in the given flag value
        /// </summary>
        /// <param name="flags">The current flag value</param>
        /// <param name="flag">The flag to set or clear</param>
        /// <param name="value">True to set the flag, false to clear it</param>
        /// <returns>The new flag value</returns>
        protected abstract TFlag SetFlag(TFlag flags, TFlag flag, bool value);

        public virtual bool AreValuesEqual(TFlag value1, TFlag value2)
        {
            // Only compare the flags we care about (those returned by GetAllFlags)
            var relevantFlags = GetAllFlags();

            foreach (var flag in relevantFlags)
            {
                var flag1Set = IsFlagSet(value1, flag);
                var flag2Set = IsFlagSet(value2, flag);

                if (flag1Set != flag2Set)
                {
                    return false;
                }
            }

            return true;
        }

        public virtual void UpdatePropertyContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            IPropertyContext propertyContext)
        {
            if (context == null)
            {
                Console.WriteLine($"Error: Context is null for {PropertyName}");
                return;
            }

            LogCollector.Add(PropertyName, $"[{PropertyName}] Processing mod: {context.ModKey}");

            if (propertyContext is not FlagPropertyContext<TFlag> flagPropertyContext)
            {
                throw new InvalidOperationException($"Error: Property context is not a flag property context for {PropertyName}");
            }

            // Add timing measurement for property access
            var recordFlags = GetValue(context.Record);

            var forwardFlagContexts = flagPropertyContext.ForwardFlagContexts;
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

            // Get all possible flags
            var allFlags = GetAllFlags();

            // Process each individual flag
            foreach (var flag in allFlags)
            {
                var isFlagSetInRecord = IsFlagSet(recordFlags, flag);
                var existingFlagContext = forwardFlagContexts.FirstOrDefault(fc => fc.Flag.Equals(flag));

                if (existingFlagContext == null)
                {
                    // New flag context - add it
                    var newFlagContext = new FlagPropertyValueContext<TFlag>(flag, isFlagSetInRecord, context.ModKey.ToString());
                    forwardFlagContexts.Add(newFlagContext);
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Adding flag {flag} = {isFlagSetInRecord} Success");
                }
                else
                {
                    // Existing flag context - check if it needs updating
                    if (existingFlagContext.IsSet != isFlagSetInRecord)
                    {
                        // Flag state has changed
                        var canModify = recordMod.MasterReferences.Any(m => m.Master.ToString() == existingFlagContext.OwnerMod);

                        if (canModify)
                        {
                            var oldState = existingFlagContext.IsSet;
                            existingFlagContext.IsSet = isFlagSetInRecord;
                            existingFlagContext.OwnerMod = context.ModKey.ToString();
                            LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Updating flag {flag} {oldState} -> {isFlagSetInRecord} Success");
                        }
                        else
                        {
                            LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Updating flag {flag} {existingFlagContext.IsSet} -> {isFlagSetInRecord} Permission denied");
                        }
                    }
                    else
                    {
                        LogCollector.Add(PropertyName, $"[{PropertyName}] [{context.ModKey}] No change needed for flag: {flag}");
                    }
                }
            }

            // Update the state
            flagPropertyContext.ForwardFlagContexts = forwardFlagContexts;
        }

        public virtual void InitializeContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> originalContext,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPropertyContext propertyContext)
        {
            if (propertyContext is not FlagPropertyContext<TFlag> flagPropertyContext)
            {
                throw new InvalidOperationException($"Error: Property context is not a flag property context for {PropertyName}");
            }

            var allFlags = GetAllFlags();

            var originalFlags = GetValue(originalContext.Record);
            var winningFlags = GetValue(winningContext.Record);

            // Initialize original flag contexts
            flagPropertyContext.OriginalFlagContexts = allFlags
                .Select(flag => new FlagPropertyValueContext<TFlag>(
                    flag,
                    IsFlagSet(originalFlags, flag),
                    originalContext.ModKey.ToString()))
                .ToList();

            // Initialize forward flag contexts
            flagPropertyContext.ForwardFlagContexts = allFlags
                .Select(flag => new FlagPropertyValueContext<TFlag>(
                    flag,
                    IsFlagSet(winningFlags, flag),
                    winningContext.ModKey.ToString()))
                .ToList();

            flagPropertyContext.IsResolved = false;
        }

        /// <summary>
        /// Format the flag for display in the log.
        /// </summary>
        /// <param name="flag">The flag to format</param>
        /// <returns>The formatted flag</returns>
        protected virtual string FormatFlag(TFlag flag)
        {
            return flag.ToString();
        }

        // Non-generic interface implementations
        void IPropertyHandler.SetValue(IMajorRecord record, object? value)
        {
            SetValue(record, (TFlag)value!);
        }

        object? IPropertyHandler.GetValue(IMajorRecordGetter record)
        {
            return GetValue(record);
        }

        bool IPropertyHandler.AreValuesEqual(object? value1, object? value2)
        {
            return AreValuesEqual((TFlag)value1!, (TFlag)value2!);
        }

        // Non-generic interface implementation for context creation
        IPropertyContext IPropertyHandler.CreatePropertyContext()
        {
            return new FlagPropertyContext<TFlag>();
        }
    }
}