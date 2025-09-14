using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Strings;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class ShortNameHandler : AbstractPropertyHandler<ITranslatedStringGetter?>
    {
        public override string PropertyName => "ShortName";

        public override void SetValue(IMajorRecord record, ITranslatedStringGetter? value)
        {
            var npcRecord = TryCastRecord<INpc>(record, PropertyName);
            if (npcRecord != null)
            {
                if (value == null)
                {
                    npcRecord.ShortName = null;
                }
                else
                {
                    // Deep copy the translated string
                    var newShortName = new TranslatedString(Mutagen.Bethesda.Strings.Language.English);
                    newShortName.String = value.String;
                    npcRecord.ShortName = newShortName;
                }
            }
        }

        public override ITranslatedStringGetter? GetValue(IMajorRecordGetter record)
        {
            var npcRecord = TryCastRecord<INpcGetter>(record, PropertyName);
            if (npcRecord != null)
            {
                return npcRecord.ShortName;
            }
            return null;
        }

        public override bool AreValuesEqual(ITranslatedStringGetter? value1, ITranslatedStringGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            return value1.String == value2.String;
        }

        public override string FormatValue(object? value)
        {
            if (value is not ITranslatedStringGetter translatedString)
            {
                return value?.ToString() ?? "null";
            }

            var text = translatedString.String ?? "null";
            return text.Length > 50 ? $"{text.Substring(0, 50)}..." : text;
        }
    }
}