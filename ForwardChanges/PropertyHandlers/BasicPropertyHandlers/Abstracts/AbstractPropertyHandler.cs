using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using ForwardChanges.Contexts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts
{
    public abstract class AbstractPropertyHandler : IPropertyHandler
    {
        public abstract string PropertyName { get; }
                public bool IsListHandler => false;
        public abstract void SetValue(IMajorRecord record, object? value);
        public abstract object? GetValue(IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context);

        public virtual bool AreValuesEqual(object? value1, object? value2)
        {
            return Equals(value1, value2);
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
            if (!AreValuesEqual(value, propertyContext.OriginalValue.Item) &&
                !AreValuesEqual(value, propertyContext.ForwardValue.Item))
            {
                var previousForwardValue = propertyContext.ForwardValue.Item;
                propertyContext.ForwardValue.Item = value;
                propertyContext.ForwardValue.OwnerMod = context.ModKey.ToString();
                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Addition: {previousForwardValue} -> {value} Success");
                return;
            }

            // Reversion: value equals original but different from current proposal to final value
            else if (AreValuesEqual(value, propertyContext.OriginalValue.Item) &&
                     !AreValuesEqual(value, propertyContext.ForwardValue.Item))
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