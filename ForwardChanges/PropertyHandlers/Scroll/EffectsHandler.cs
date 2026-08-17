using System.Collections.Generic;
using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Scroll
{
    public class EffectsHandler : AbstractEffectsHandler<IScrollGetter, IScroll>
    {
        protected override IEnumerable<IEffectGetter>? GetEffects(IScrollGetter record)
        {
            return record.Effects;
        }

        protected override IEnumerable<IEffectGetter>? GetEffects(IScroll record)
        {
            return record.Effects;
        }

        protected override void UpdateEffectsCollection(IScroll record, List<IEffectGetter> effects)
        {
            record.Effects.Clear();
            foreach (var effect in effects)
            {
                if (effect is Effect concreteEffect)
                {
                    record.Effects.Add(concreteEffect);
                    continue;
                }

                var newEffect = new Effect
                {
                    BaseEffect = new Mutagen.Bethesda.Plugins.FormLinkNullable<IMagicEffectGetter>(effect.BaseEffect.FormKey),
                    Data = effect.Data != null ? new EffectData
                    {
                        Magnitude = effect.Data.Magnitude,
                        Area = effect.Data.Area,
                        Duration = effect.Data.Duration
                    } : null,
                    Conditions = new Noggog.ExtendedList<Condition>(effect.Conditions.Select(c => c.DeepCopy()))
                };

                record.Effects.Add(newEffect);
            }
        }
    }
}