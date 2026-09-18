using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Shout
{
    public class WordsOfPowerHandler : AbstractPropertyHandler<List<IShoutWordGetter>>
    {
        public override string PropertyName => "WordsOfPower";

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

        public override bool AreValuesEqual(List<IShoutWordGetter>? value1, List<IShoutWordGetter>? value2)
        {
            if (ReferenceEquals(value1, value2)) return true;
            if (value1 == null || value2 == null || value1.Count != value2.Count) return false;
            return value1.Zip(value2).All(pair =>
                pair.First.Word.FormKey == pair.Second.Word.FormKey
                && pair.First.Spell.FormKey == pair.Second.Spell.FormKey
                && pair.First.RecoveryTime.Equals(pair.Second.RecoveryTime));
        }
    }
}
