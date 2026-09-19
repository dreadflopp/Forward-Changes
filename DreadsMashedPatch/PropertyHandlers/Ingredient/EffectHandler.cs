using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Skyrim;

namespace ForwardChanges.PropertyHandlers.Ingredient;

public class EffectHandler : AbstractEffectsHandler<IIngredientGetter, IIngredient>
{
    protected override IEnumerable<IEffectGetter>? GetEffects(IIngredientGetter record)
        => record.Effects;

    protected override IEnumerable<IEffectGetter>? GetEffects(IIngredient record)
        => record.Effects;

    protected override void UpdateEffectsCollection(IIngredient record, List<IEffectGetter> effects)
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
