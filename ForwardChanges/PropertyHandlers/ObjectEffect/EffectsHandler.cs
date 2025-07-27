using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.ObjectEffect
{
    public class EffectsHandler : AbstractPropertyHandler<IReadOnlyList<IEffectGetter>>
    {
        public override string PropertyName => "Effects";

        public override void SetValue(IMajorRecord record, IReadOnlyList<IEffectGetter>? value)
        {
            if (record is IObjectEffect objectEffect)
            {
                var effectsList = objectEffect.Effects;
                effectsList.Clear();
                if (value != null)
                {
                    foreach (var effect in value)
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
                        effectsList.Add(newEffect);
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IObjectEffect for {PropertyName}");
            }
        }

        public override IReadOnlyList<IEffectGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IObjectEffectGetter objectEffect)
            {
                return objectEffect.Effects;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IObjectEffectGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IReadOnlyList<IEffectGetter>? value1, IReadOnlyList<IEffectGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1.Count != value2.Count) return false;

            // Compare each effect in order
            for (int i = 0; i < value1.Count; i++)
            {
                if (!AreEffectsEqual(value1[i], value2[i])) return false;
            }
            return true;
        }

        private bool AreEffectsEqual(IEffectGetter? effect1, IEffectGetter? effect2)
        {
            if (effect1 == null && effect2 == null) return true;
            if (effect1 == null || effect2 == null) return false;

            // Compare BaseEffect
            if (!effect1.BaseEffect.Equals(effect2.BaseEffect)) return false;

            // Compare Data
            if (effect1.Data == null && effect2.Data == null) return true;
            if (effect1.Data == null || effect2.Data == null) return false;

            if (effect1.Data.Magnitude != effect2.Data.Magnitude ||
                effect1.Data.Area != effect2.Data.Area ||
                effect1.Data.Duration != effect2.Data.Duration)
            {
                return false;
            }

            // Compare Conditions
            if (effect1.Conditions.Count != effect2.Conditions.Count) return false;
            for (int i = 0; i < effect1.Conditions.Count; i++)
            {
                if (!AreConditionsEqual(effect1.Conditions[i], effect2.Conditions[i])) return false;
            }

            return true;
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

