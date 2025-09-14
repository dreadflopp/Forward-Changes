using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.Contexts.Interfaces;
using System.Runtime.InteropServices;

namespace ForwardChanges.PropertyHandlers.Abstracts
{
    public abstract class AbstractPropertyHandler<T> : IPropertyHandler<T>
    {
        public abstract string PropertyName { get; }
        public bool RequiresFullLoadOrderProcessing => true;

        public abstract void SetValue(IMajorRecord record, T? value);
        public abstract T? GetValue(IMajorRecordGetter record);

        public virtual bool AreValuesEqual(T? value1, T? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return Equals(value1, value2);
        }

        // Helper methods to reduce boilerplate
        protected static TRecord? TryCastRecord<TRecord>(IMajorRecordGetter record, string propertyName) where TRecord : class
        {
            if (record is TRecord typedRecord)
            {
                return typedRecord;
            }
            Console.WriteLine($"Error: Record does not implement {typeof(TRecord).Name} for {propertyName}");
            return null;
        }

        protected static TRecord? TryCastRecord<TRecord>(IMajorRecord record, string propertyName) where TRecord : class
        {
            if (record is TRecord typedRecord)
            {
                return typedRecord;
            }
            Console.WriteLine($"Error: Record does not implement {typeof(TRecord).Name} for {propertyName}");
            return null;
        }

        // Non-generic interface implementations
        void IPropertyHandler.SetValue(IMajorRecord record, object? value)
        {
            SetValue(record, (T?)value);
        }

        object? IPropertyHandler.GetValue(IMajorRecordGetter record)
        {
            return GetValue(record);
        }

        bool IPropertyHandler.AreValuesEqual(object? value1, object? value2)
        {
            return AreValuesEqual((T?)value1, (T?)value2);
        }

        // Non-generic interface implementation for context creation
        IPropertyContext IPropertyHandler.CreatePropertyContext()
        {
            return new SimplePropertyContext<T>();
        }

        /// <summary>
        /// Format a value for display in logs. Override this method to provide custom formatting.
        /// </summary>
        /// <param name="value">The value to format</param>
        /// <returns>The formatted value</returns>
        public virtual string FormatValue(object? value)
        {
            return value?.ToString() ?? "null";
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

            if (propertyContext is not SimplePropertyContext<T> simplePropertyContext)
            {
                throw new InvalidOperationException($"Property context is not a simple property context for {PropertyName}");
            }

            var recordValue = GetValue(context.Record);
            if (simplePropertyContext.OriginalValueContext == null || simplePropertyContext.ForwardValueContext == null)
            {
                Console.WriteLine($"Error: Property context is not properly initialized for {PropertyName}");
                return;
            }

            // Addition: value is different from both original and current proposal to forward value
            T? originalValue = simplePropertyContext.OriginalValueContext.Value;
            T? forwardValue = simplePropertyContext.ForwardValueContext.Value;

            if (!AreValuesEqual(recordValue, originalValue) &&
                !AreValuesEqual(recordValue, forwardValue))
            {
                T? previousForwardValue = forwardValue;
                simplePropertyContext.ForwardValueContext.Value = recordValue;
                simplePropertyContext.ForwardValueContext.OwnerMod = context.ModKey.ToString();
                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Addition: {FormatValue(previousForwardValue)} -> {FormatValue(recordValue)} Success");
                return;
            }

            // Reversion: value equals original but different from current proposal to forward value
            else if (AreValuesEqual(recordValue, originalValue) &&
                     !AreValuesEqual(recordValue, forwardValue))
            {
                var currentMod = state.LoadOrder[context.ModKey].Mod;
                var canModify = currentMod?.MasterReferences.Any(m => m.Master.ToString() == simplePropertyContext.ForwardValueContext.OwnerMod) == true;

                if (canModify)
                {
                    // Reversion: value equals original but different from current proposal to forward value, ie we are reverting to the original value
                    T? previousForwardValue = forwardValue;
                    simplePropertyContext.ForwardValueContext.Value = recordValue;
                    simplePropertyContext.ForwardValueContext.OwnerMod = context.ModKey.ToString();
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Reversion: {FormatValue(previousForwardValue)} -> {FormatValue(recordValue)} Success");
                }
                else
                {
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Reversion: {FormatValue(forwardValue)} -> {FormatValue(recordValue)} Permission denied");
                }
            }
            else
            {
                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: No change to value: {FormatValue(forwardValue)}");
            }
        }

        public void InitializeContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> originalContext,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPropertyContext propertyContext)
        {
            if (propertyContext is not SimplePropertyContext<T> simplePropertyContext)
            {
                throw new InvalidOperationException($"Property context is not a simple property context for {PropertyName}");
            }
            simplePropertyContext.OriginalValueContext = new SimplePropertyValueContext<T>(GetValue(originalContext.Record), originalContext.ModKey.ToString());
            simplePropertyContext.ForwardValueContext = new SimplePropertyValueContext<T>(GetValue(winningContext.Record), winningContext.ModKey.ToString());
            simplePropertyContext.IsResolved = false;
        }
    }
}