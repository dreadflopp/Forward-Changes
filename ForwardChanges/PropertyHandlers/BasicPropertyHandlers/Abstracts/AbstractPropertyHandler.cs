using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts
{
    public abstract class AbstractPropertyHandler<TItem> : IPropertyHandler<TItem>
    {
        public abstract string PropertyName { get; }
        public bool IsListHandler => false;

        public abstract void SetValue(IMajorRecord record, TItem? value);
        public abstract TItem? GetValue(IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context);

        public virtual bool AreValuesEqual(TItem? value1, TItem? value2)
        {
            return Equals(value1, value2);
        }

        // IPropertyHandlerBase implementation
        void IPropertyHandlerBase.SetValue(IMajorRecord record, object? value)
        {
            try
            {
                SetValue(record, (TItem?)value);
            }
            catch (InvalidCastException)
            {
                Console.WriteLine($"[{PropertyName}] SetValue failed: Expected type {typeof(TItem)}, got {value?.GetType() ?? typeof(object)}");
                throw;
            }
        }

        object? IPropertyHandlerBase.GetValue(IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            try
            {
                return GetValue(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{PropertyName}] GetValue failed: {ex.Message}");
                throw;
            }
        }

        bool IPropertyHandlerBase.AreValuesEqual(object? value1, object? value2)
        {
            try
            {
                return AreValuesEqual((TItem?)value1, (TItem?)value2);
            }
            catch (InvalidCastException)
            {
                Console.WriteLine($"[{PropertyName}] AreValuesEqual failed: Expected type {typeof(TItem)}, got {value1?.GetType() ?? typeof(object)} and {value2?.GetType() ?? typeof(object)}");
                throw;
            }
        }

        public virtual void UpdatePropertyContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            PropertyContext propertyContext)
        {
            if (context == null)
            {
                Console.WriteLine($"Error: Context is null for {PropertyName}");
                return;
            }

            if (string.IsNullOrEmpty(propertyContext.OriginalValue.OwnerMod))
            {
                Console.WriteLine($"Error: Property state for {PropertyName} not properly initialized");
                return;
            }

            var value = GetValue(context);

            // Addition: value is different from both original and current proposal to fowrward value
            if (!AreValuesEqual(value, (TItem?)propertyContext.OriginalValue.Item) &&
                !AreValuesEqual(value, (TItem?)propertyContext.ForwardValue.Item))
            {
                var previousForwardValue = propertyContext.ForwardValue.Item;
                propertyContext.ForwardValue.Item = value;
                propertyContext.ForwardValue.OwnerMod = context.ModKey.ToString();
                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Addition: {previousForwardValue} -> {value} Success");
                return;
            }

            // Reversion: value equals original but different from current proposal to forward value
            else if (AreValuesEqual(value, (TItem?)propertyContext.OriginalValue.Item) &&
                     !AreValuesEqual(value, (TItem?)propertyContext.ForwardValue.Item))
            {
                var currentMod = state.LoadOrder[context.ModKey].Mod;
                var canModify = currentMod?.MasterReferences.Any(m => m.Master.ToString() == propertyContext.ForwardValue.OwnerMod) == true;

                if (canModify)
                {
                    var previousForwardValue = propertyContext.ForwardValue.Item;
                    propertyContext.ForwardValue.Item = value;
                    propertyContext.ForwardValue.OwnerMod = context.ModKey.ToString();
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Reversion: {previousForwardValue} -> {value} Success");
                }
                else
                {
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Reversion: {propertyContext.ForwardValue.Item} -> {value} Permission denied");
                }
            }
            else
            {
                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: No change to value: {propertyContext.ForwardValue.Item}");
            }
        }
    }
}