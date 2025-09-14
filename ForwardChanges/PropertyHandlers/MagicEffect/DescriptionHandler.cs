using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Strings;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class DescriptionHandler : AbstractPropertyHandler<string>
    {
        public override string PropertyName => "Description";

        public override void SetValue(IMajorRecord record, string? value)
        {
            if (record is IMagicEffect magicEffect)
            {
                if (value != null)
                {
                    var translatedString = new TranslatedString(Language.English);
                    translatedString.String = value;
                    magicEffect.Description = translatedString;
                }
                else
                {
                    magicEffect.Description = null;
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffect for {PropertyName}");
            }
        }

        public override string? GetValue(IMajorRecordGetter record)
        {
            if (record is IMagicEffectGetter magicEffect)
            {
                return magicEffect.Description?.String;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
            }
            return null;
        }
    }
}
