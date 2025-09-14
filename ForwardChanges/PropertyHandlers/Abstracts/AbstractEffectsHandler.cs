using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;
using ForwardChanges.Contexts.Interfaces;
using System.Linq;
using Noggog;

namespace ForwardChanges.PropertyHandlers.Abstracts
{
    public abstract class AbstractEffectsHandler<TRecordGetter, TRecord> : AbstractListPropertyHandler<IEffectGetter>
        where TRecordGetter : class, IMajorRecordGetter
        where TRecord : class, IMajorRecord
    {
        public override string PropertyName => "Effects";

        public override List<IEffectGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is TRecordGetter typedRecord)
            {
                var effects = GetEffects(typedRecord);
                var effectsList = effects?.ToList();
                return effectsList;
            }

            return null;
        }

        public override void SetValue(IMajorRecord record, List<IEffectGetter>? value)
        {
            if (record is TRecord typedRecord)
            {
                var effectsEnumerable = GetEffects(typedRecord);

                if (effectsEnumerable == null)
                {
                    return;
                }

                // Convert to a list that we can modify
                var effects = effectsEnumerable.ToList();
                effects.Clear();

                if (value != null)
                {
                    foreach (var effect in value)
                    {
                        if (effect == null)
                        {
                            continue;
                        }

                        try
                        {
                            var newEffect = new Effect
                            {
                                BaseEffect = new FormLinkNullable<IMagicEffectGetter>(effect.BaseEffect.FormKey),
                                Data = effect.Data != null ? new EffectData
                                {
                                    Magnitude = effect.Data.Magnitude,
                                    Area = effect.Data.Area,
                                    Duration = effect.Data.Duration
                                } : null,
                                Conditions = new ExtendedList<Condition>(effect.Conditions.Select(c => c.DeepCopy()))
                            };
                            effects.Add(newEffect);
                        }
                        catch
                        {
                            // Handle error silently or log if needed
                        }
                    }
                }

                // Update the record's effects collection
                UpdateEffectsCollection(typedRecord, effects);
            }
        }

        protected abstract void UpdateEffectsCollection(TRecord record, List<IEffectGetter> effects);

        protected abstract IEnumerable<IEffectGetter>? GetEffects(TRecordGetter record);
        protected abstract IEnumerable<IEffectGetter>? GetEffects(TRecord record);

        protected override bool IsItemEqual(IEffectGetter? item1, IEffectGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Compare BaseEffect using FormKey (value-based comparison)
            var baseEffectEqual = item1.BaseEffect.FormKey == item2.BaseEffect.FormKey;
            var dataEqual = AreEffectDataEqual(item1.Data, item2.Data);
            var conditionsEqual = AreConditionsListsEqual(item1.Conditions, item2.Conditions);

            var result = baseEffectEqual && dataEqual && conditionsEqual;

            // Only log when there's a mismatch or when comparing specific effects
            if (!result)
            {
                // LogCollector.Add(PropertyName, $"[{PropertyName}] DEBUG IsItemEqual: MISMATCH - {item1.BaseEffect.FormKey}({item1.Data?.Magnitude ?? 0}) vs {item2.BaseEffect.FormKey}({item2.Data?.Magnitude ?? 0})");
                // LogCollector.Add(PropertyName, $"[{PropertyName}] DEBUG IsItemEqual: BaseEffect: {baseEffectEqual}, Data: {dataEqual}, Conditions: {conditionsEqual}");
            }

            return result;
        }

        private bool AreEffectDataEqual(IEffectDataGetter? data1, IEffectDataGetter? data2)
        {
            if (data1 == null && data2 == null) return true;
            if (data1 == null || data2 == null) return false;

            return data1.Magnitude == data2.Magnitude &&
                   data1.Area == data2.Area &&
                   data1.Duration == data2.Duration;
        }

        private bool AreConditionsListsEqual(IReadOnlyList<IConditionGetter> conditions1, IReadOnlyList<IConditionGetter> conditions2)
        {
            if (conditions1.Count != conditions2.Count) return false;
            for (int i = 0; i < conditions1.Count; i++)
            {
                if (!AreConditionsEqual(conditions1[i], conditions2[i])) return false;
            }
            return true;
        }

        protected override string FormatItem(IEffectGetter? item)
        {
            if (item == null) return "null";

            try
            {
                var baseEffect = item.BaseEffect.FormKey.ToString();
                var data = item.Data != null
                    ? $"Magnitude:{item.Data.Magnitude}, Area:{item.Data.Area}, Duration:{item.Data.Duration}"
                    : "null";
                var conditionsCount = item.Conditions.Count;

                return $"Effect(BaseEffect:{baseEffect}, Data:{{{data}}}, Conditions:{conditionsCount})";
            }
            catch (Exception ex)
            {
                return $"Effect({item.GetType().Name}) - Error: {ex.Message}";
            }
        }

        private bool AreConditionsEqual(IConditionGetter? condition1, IConditionGetter? condition2)
        {
            if (condition1 == null && condition2 == null) return true;
            if (condition1 == null || condition2 == null) return false;

            // For now, use a simple comparison. This could be enhanced to compare specific condition properties
            return condition1.GetType() == condition2.GetType() &&
                   condition1.ToString() == condition2.ToString();
        }
    }
}