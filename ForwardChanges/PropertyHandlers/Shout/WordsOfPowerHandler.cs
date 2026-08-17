using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Shout
{
    public class WordsOfPowerHandler : AbstractListPropertyHandler<IShoutWordGetter>
    {
        public override string PropertyName => "WordsOfPower";

        protected override ListOrdering Ordering => ListOrdering.None;

        public override void SetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecord record, List<IShoutWordGetter>? value)
        {
            if (record is not IShout shout)
            {
                return;
            }

            shout.WordsOfPower.Clear();
            if (value == null)
            {
                return;
            }

            foreach (var word in value)
            {
                var newWord = new ShoutWord
                {
                    Word = new FormLink<IWordOfPowerGetter>(word.Word.FormKey),
                    Spell = new FormLink<ISpellGetter>(word.Spell.FormKey),
                    RecoveryTime = word.RecoveryTime
                };
                shout.WordsOfPower.Add(newWord);
            }
        }

        public override List<IShoutWordGetter>? GetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecordGetter record)
        {
            return (record as IShoutGetter)?.WordsOfPower?.ToList();
        }

        protected override bool IsItemEqual(IShoutWordGetter? item1, IShoutWordGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            return item1.Word.FormKey == item2.Word.FormKey
                && item1.Spell.FormKey == item2.Spell.FormKey
                && item1.RecoveryTime.Equals(item2.RecoveryTime);
        }
    }
}