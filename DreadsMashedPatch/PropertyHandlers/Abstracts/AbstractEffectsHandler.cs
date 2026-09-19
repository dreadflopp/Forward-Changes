using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;

namespace ForwardChanges.PropertyHandlers.Abstracts
{
    public abstract class AbstractEffectsHandler<TRecordGetter, TRecord> : AbstractListPropertyHandler<IEffectGetter>
        where TRecordGetter : class, IMajorRecordGetter
        where TRecord : class, IMajorRecord
    {
        // Skyrim xEdit defines the outer Effects collection as a plain wbRArray of
        // wbRStruct entries with no outer StructSK. Its rows therefore have no
        // stable value key and are compared by ordinal position.
        public override ListSemantics Semantics => ListSemantics.ExactOrdered;

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

                        effects.Add(effect.DeepCopy());
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

            return item1.Equals(item2);
        }

        protected override IEffectGetter CopyItemForForwardContext(IEffectGetter item)
            => item.DeepCopy();

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
    }
}
