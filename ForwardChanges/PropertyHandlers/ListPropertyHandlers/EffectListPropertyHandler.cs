using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using Noggog;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.ListPropertyHandlers
{
    public class EffectListPropertyHandler : AbstractListPropertyHandler<IEffectGetter>
    {
        public override string PropertyName => "Effects";

        public override void SetValue(IMajorRecord record, List<IEffectGetter>? value)
        {
            if (record is IIngestible ingestible)
            {
                var effectsList = ingestible.Effects;
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
                Console.WriteLine($"Error: Record does not implement IIngestible for {PropertyName}");
            }
        }

        public override List<IEffectGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IIngestibleGetter ingestible)
            {
                return ingestible.Effects?.ToList();
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IIngestibleGetter for {PropertyName}");
            }
            return null;
        }

        protected override bool IsItemEqual(IEffectGetter? item1, IEffectGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Compare by BaseEffect FormKey
            return item1.BaseEffect.FormKey.Equals(item2.BaseEffect.FormKey);
        }

        protected override string FormatItem(IEffectGetter? item)
        {
            return item?.BaseEffect.FormKey.ToString() ?? "null";
        }

        protected override void ProcessHandlerSpecificLogic(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            ListPropertyContext<IEffectGetter> listPropertyContext,
            List<IEffectGetter> recordItems,
            List<ListPropertyValueContext<IEffectGetter>> currentForwardItems)
        {
            var recordMod = state.LoadOrder[context.ModKey].Mod;
            if (recordMod == null) return;

            // Process EffectData changes for effects that we have permission to modify
            foreach (var forwardItem in currentForwardItems.Where(i => !i.IsRemoved))
            {
                var matchingRecordItem = recordItems.FirstOrDefault(r => IsItemEqual(r, forwardItem.Value));
                if (matchingRecordItem == null) continue;

                // Check if we have permission to modify this effect
                var canModify = recordMod.MasterReferences.Any(m => m.Master.ToString() == forwardItem.OwnerMod);
                if (!canModify) continue;

                // If the BaseEffect is the same but EffectData is different, update the EffectData
                if (matchingRecordItem.Data != null && forwardItem.Value.Data != null)
                {
                    var recordData = matchingRecordItem.Data;
                    var forwardData = forwardItem.Value.Data;

                    // Check if any EffectData properties have changed
                    if (Math.Abs(recordData.Magnitude - forwardData.Magnitude) > 0.001f ||
                        recordData.Area != forwardData.Area ||
                        recordData.Duration != forwardData.Duration)
                    {
                        LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: Updating EffectData for effect {FormatItem(matchingRecordItem)} - Magnitude: {recordData.Magnitude}->{forwardData.Magnitude}, Area: {recordData.Area}->{forwardData.Area}, Duration: {recordData.Duration}->{forwardData.Duration}");

                        // Update the forward item's EffectData to match the record
                        if (forwardItem.Value is Effect effect)
                        {
                            if (effect.Data == null)
                            {
                                effect.Data = new EffectData();
                            }
                            effect.Data.Magnitude = recordData.Magnitude;
                            effect.Data.Area = recordData.Area;
                            effect.Data.Duration = recordData.Duration;
                        }
                    }
                }
            }
        }
    }
}