using DreadsMashedPatch.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Skyrim;

namespace DreadsMashedPatch.PropertyHandlers.ObjectEffect;

public class EffectsHandler : AbstractEffectsHandler<IObjectEffectGetter, IObjectEffect>
{
    protected override IEnumerable<IEffectGetter>? GetEffects(IObjectEffectGetter record)
        => record.Effects;

    protected override IEnumerable<IEffectGetter>? GetEffects(IObjectEffect record)
        => record.Effects;

    protected override void UpdateEffectsCollection(IObjectEffect record, List<IEffectGetter> effects)
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
