using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Aspects;
using Noggog;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ListPropertyHandlers
{
    public class KeywordListPropertyHandler : AbstractListPropertyHandler<IFormLinkGetter<IKeywordGetter>>
    {
        public override string PropertyName => "Keywords";

        public override void SetValue(IMajorRecord record, List<IFormLinkGetter<IKeywordGetter>>? value)
        {
            if (record is IKeyworded<IKeywordGetter> keyworded)
            {
                if (value != null)
                {
                    keyworded.Keywords = new ExtendedList<IFormLinkGetter<IKeywordGetter>>(value);
                }
                else
                {
                    keyworded.Keywords = null;
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IKeyworded<IKeywordGetter> for {PropertyName}");
            }
        }

        public override List<IFormLinkGetter<IKeywordGetter>>? GetValue(IMajorRecordGetter record)
        {
            if (record is IKeywordedGetter<IKeywordGetter> keyworded)
            {
                return keyworded.Keywords?.ToList();
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IKeywordedGetter<IKeywordGetter> for {PropertyName}");
            }
            return null;
        }

        protected override bool IsItemEqual(IFormLinkGetter<IKeywordGetter>? item1, IFormLinkGetter<IKeywordGetter>? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;
            return item1.FormKey.Equals(item2.FormKey);
        }

        protected override string FormatItem(IFormLinkGetter<IKeywordGetter>? item)
        {
            return item?.FormKey.ToString() ?? "null";
        }
    }
}