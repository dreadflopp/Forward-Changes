using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using ForwardChanges.PropertyStates;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts
{
    public abstract class AbstractPropertyHandler : IPropertyHandler
    {
        public abstract string PropertyName { get; }

        public abstract object? GetValue(IMajorRecordGetter record);
        public abstract void SetValue(IMajorRecord record, object? value);

        public virtual PropertyState CreateState(string lastChangedByMod, object? originalValue = null) => new()
        {
            OriginalValue = originalValue,
            FinalValue = originalValue,
            LastChangedByMod = lastChangedByMod
        };

        public virtual bool IsListHandler => false;

        public virtual void UpdatePropertyState(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            PropertyState propertyState)
        {
            if (context == null)
                return;

            if (propertyState.OriginalValue == null || string.IsNullOrEmpty(propertyState.LastChangedByMod))
            {
                Console.WriteLine($"Error: Property state for {PropertyName} not properly initialized");
                return;
            }

            var value = GetValue(context.Record);

            // Addition: value is different from both original and current proposal to final value
            if (!AreValuesEqual(value, (object?)propertyState.OriginalValue) &&
                !AreValuesEqual(value, (object?)propertyState.FinalValue))
            {
                var previousFinalValue = propertyState.FinalValue;
                propertyState.ShouldForward = true;
                propertyState.FinalValue = value;
                propertyState.LastChangedByMod = context.ModKey.ToString();
                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Addition: {previousFinalValue} -> {value} Success");
                return;
            }

            // Reversion: value equals original but different from current proposal to final value
            else if (AreValuesEqual(value, (object?)propertyState.OriginalValue) &&
                     !AreValuesEqual(value, (object?)propertyState.FinalValue))
            {
                var currentMod = state.LoadOrder[context.ModKey].Mod;
                var canModify = currentMod?.MasterReferences.Any(m => m.Master.ToString() == propertyState.LastChangedByMod) == true;

                if (canModify)
                {
                    var previousFinalValue = propertyState.FinalValue;
                    propertyState.ShouldForward = true;
                    propertyState.FinalValue = value;
                    propertyState.LastChangedByMod = context.ModKey.ToString();
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Reversion: {previousFinalValue} -> {value} Success");
                }
                else
                {
                    LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Reversion: {propertyState.FinalValue} -> {value} Permission denied");
                }
            }
            else
            {
                LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: No change to value: {propertyState.FinalValue}");
            }
        }

        public virtual object? GetValueFromContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            return GetValue(context.Record);
        }

        public virtual bool AreValuesEqual(object? value1, object? value2)
        {
            return Equals(value1, value2);
        }
    }
}