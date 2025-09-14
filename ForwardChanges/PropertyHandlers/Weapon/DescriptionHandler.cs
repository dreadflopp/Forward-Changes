using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Strings;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class DescriptionHandler : AbstractPropertyHandler<ITranslatedStringGetter?>
    {
        public override string PropertyName => "Description";

        public override void SetValue(IMajorRecord record, ITranslatedStringGetter? value)
        {
            var weaponRecord = TryCastRecord<IWeapon>(record, PropertyName);
            if (weaponRecord != null)
            {
                if (value == null)
                {
                    weaponRecord.Description = null;
                }
                else
                {
                    // Deep copy the translated string
                    var newDescription = new TranslatedString(Mutagen.Bethesda.Strings.Language.English);
                    newDescription.String = value.String;
                    weaponRecord.Description = newDescription;
                }
            }
        }

        public override ITranslatedStringGetter? GetValue(IMajorRecordGetter record)
        {
            var weaponRecord = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weaponRecord != null)
            {
                return weaponRecord.Description;
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