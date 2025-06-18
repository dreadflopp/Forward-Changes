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

        // Non-generic InitializeContext implementation
        void IPropertyHandlerBase.InitializeContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> originalContext,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            PropertyContext propertyContext)
        {
            InitializeContext(originalContext, winningContext, (PropertyContext<TItem>)propertyContext);
        }

        // Generic InitializeContext implementation
        public virtual void InitializeContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> originalContext,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            PropertyContext<TItem> propertyContext)
        {
            var originalValue = GetValue(originalContext);
            propertyContext.OriginalValue = new ItemContext<TItem>(originalValue, originalContext.ModKey.ToString());
            propertyContext.ForwardValue = new ItemContext<TItem>(originalValue, winningContext.ModKey.ToString());
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

            var value = GetValue(context);
            var forwardItemContext = propertyContext.ForwardValue as ItemContext<object?>;
            var originalItemContext = propertyContext.OriginalValue as ItemContext<object?>;
            if (forwardItemContext == null || originalItemContext == null)
            {
                Console.WriteLine($"Error: Property context is not properly initialized for {PropertyName}");
                return;
            }

            // Addition: value is different from both original and current proposal to forward value
            if (!AreValuesEqual(value, (TItem?)originalItemContext.Item) &&
                !AreValuesEqual(value, (TItem?)forwardItemContext.Item))
            {
                var previousForwardValue = forwardItemContext.Item;
                forwardItemContext.Item = value;
                forwardItemContext.OwnerMod = context.ModKey.ToString();
                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Addition: {previousForwardValue} -> {value} Success");
                return;
            }

            // Reversion: value equals original but different from current proposal to forward value
            else if (AreValuesEqual(value, (TItem?)originalItemContext.Item) &&
                     !AreValuesEqual(value, (TItem?)forwardItemContext.Item))
            {
                var currentMod = state.LoadOrder[context.ModKey].Mod;
                var canModify = currentMod?.MasterReferences.Any(m => m.Master.ToString() == forwardItemContext.OwnerMod) == true;

                if (canModify)
                {
                    var previousForwardValue = forwardItemContext.Item;
                    forwardItemContext.Item = value;
                    forwardItemContext.OwnerMod = context.ModKey.ToString();
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Reversion: {previousForwardValue} -> {value} Success");
                }
                else
                {
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Reversion: {forwardItemContext.Item} -> {value} Permission denied");
                }
            }
            else
            {
                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: No change to value: {forwardItemContext.Item}");
            }
        }
    }
}