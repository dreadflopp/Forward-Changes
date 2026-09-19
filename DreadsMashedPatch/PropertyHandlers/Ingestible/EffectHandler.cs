using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Skyrim;

namespace ForwardChanges.PropertyHandlers.Ingestible;

public class EffectHandler : AbstractEffectsHandler<IIngestibleGetter, IIngestible>
{
    protected override IEnumerable<IEffectGetter>? GetEffects(IIngestibleGetter record)
        => record.Effects;

    protected override IEnumerable<IEffectGetter>? GetEffects(IIngestible record)
        => record.Effects;

    protected override void UpdateEffectsCollection(IIngestible record, List<IEffectGetter> effects)
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
