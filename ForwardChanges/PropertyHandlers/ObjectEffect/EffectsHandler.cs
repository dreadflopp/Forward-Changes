using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.ObjectEffect
{
    public class EffectsHandler : AbstractEffectsHandler<IObjectEffectGetter, IObjectEffect>
    {
        protected override IEnumerable<IEffectGetter>? GetEffects(IObjectEffectGetter record)
        {
            return record.Effects;
        }

        protected override IEnumerable<IEffectGetter>? GetEffects(IObjectEffect record)
        {
            return record.Effects;
        }

        protected override void UpdateEffectsCollection(IObjectEffect record, List<IEffectGetter> effects)
        {
            // Clear the existing effects and add the new ones
            record.Effects.Clear();
            foreach (var effect in effects)
            {
                if (effect is Effect concreteEffect)
                {
                    record.Effects.Add(concreteEffect);
                }
                else
                {
                    // Convert IEffectGetter to Effect
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
                    record.Effects.Add(newEffect);
                }
            }
        }
    }
}

