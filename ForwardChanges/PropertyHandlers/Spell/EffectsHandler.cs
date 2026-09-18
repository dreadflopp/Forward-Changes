using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Skyrim;

namespace ForwardChanges.PropertyHandlers.Spell;

public class EffectsHandler : AbstractEffectsHandler<ISpellGetter, ISpell>
{
    protected override IEnumerable<IEffectGetter>? GetEffects(ISpellGetter record)
        => record.Effects;

    protected override IEnumerable<IEffectGetter>? GetEffects(ISpell record)
        => record.Effects;

    protected override void UpdateEffectsCollection(ISpell record, List<IEffectGetter> effects)
    {
        record.Effects.Clear();
        foreach (var effect in effects)
        {
            record.Effects.Add(effect is Effect concreteEffect
                ? concreteEffect
                : effect.DeepCopy());
        }
    }
}
