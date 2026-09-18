using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Skyrim;

namespace ForwardChanges.PropertyHandlers.Scroll;

public class EffectsHandler : AbstractEffectsHandler<IScrollGetter, IScroll>
{
    protected override IEnumerable<IEffectGetter>? GetEffects(IScrollGetter record)
        => record.Effects;

    protected override IEnumerable<IEffectGetter>? GetEffects(IScroll record)
        => record.Effects;

    protected override void UpdateEffectsCollection(IScroll record, List<IEffectGetter> effects)
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
