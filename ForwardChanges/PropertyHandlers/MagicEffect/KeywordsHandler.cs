using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class KeywordsHandler : AbstractListPropertyHandler<IFormLinkGetter<IKeywordGetter>>
    {
        public override string PropertyName => "Keywords";

        public override List<IFormLinkGetter<IKeywordGetter>>? GetValue(IMajorRecordGetter record)
        {
            if (record is IMagicEffectGetter magicEffect)
            {
                return magicEffect.Keywords?.ToList();
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, List<IFormLinkGetter<IKeywordGetter>>? value)
        {
            if (record is IMagicEffect magicEffect)
            {
                if (value == null)
                {
                    magicEffect.Keywords = null;
                    return;
                }

                // Create a new list and copy all keywords
                var newKeywords = new ExtendedList<IFormLinkGetter<IKeywordGetter>>();
                foreach (var keyword in value)
                {
                    if (keyword == null) continue;
                    newKeywords.Add(keyword);
                }

                magicEffect.Keywords = newKeywords;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffect for {PropertyName}");
            }
        }

        protected override bool IsItemEqual(IFormLinkGetter<IKeywordGetter>? item1, IFormLinkGetter<IKeywordGetter>? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Compare FormKeys instead of using Equals on complex FormLink objects
            return item1.FormKey == item2.FormKey;
        }

        protected override string FormatItem(IFormLinkGetter<IKeywordGetter>? item)
        {
            if (item == null) return "null";
            return $"Keyword({item.FormKey})";
        }
    }
}
