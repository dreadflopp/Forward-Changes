using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Strings;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class SpellDescriptionPropertyHandler : AbstractPropertyHandler<ITranslatedStringGetter>
    {
        public override string PropertyName => "Description";

        public override void SetValue(IMajorRecord record, ITranslatedStringGetter? value)
        {
            if (record is ISpell spellRecord)
            {
                if (value == null)
                {
                    // Create an empty translated string since Description is not nullable
                    var emptyDescription = new TranslatedString(Language.English);
                    emptyDescription.String = "";
                    spellRecord.Description = emptyDescription;
                }
                else
                {
                    // Create a deep copy of the translated string
                    var newDescription = new TranslatedString(Language.English);
                    newDescription.String = value.String;
                    spellRecord.Description = newDescription;
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ISpell for {PropertyName}");
            }
        }

        public override ITranslatedStringGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is ISpellGetter spellRecord)
            {
                return spellRecord.Description;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ISpellGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(ITranslatedStringGetter? value1, ITranslatedStringGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare the string values
            return value1.String == value2.String;
        }
    }
}