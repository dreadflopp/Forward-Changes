using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Aspects;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Mutagen.Bethesda.Strings;

namespace ForwardChanges.PropertyHandlers.General
{
    public class NameHandler : AbstractPropertyHandler<string>
    {
        public override string PropertyName => "Name";

        public override void SetValue(IMajorRecord record, string? value)
        {
            if (record is INamed named)
            {
                if (value != null)
                {
                    var translatedString = new TranslatedString(Language.English);
                    translatedString.String = value;
                    named.Name = translatedString;
                }
                else
                {
                    named.Name = null;
                }
            }
            else if (record is INamedRequired namedRequired)
            {
                namedRequired.Name = value ?? string.Empty;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INamed for {PropertyName}");
            }
        }

        public override string? GetValue(IMajorRecordGetter record)
        {
            if (record is INamedGetter named)
            {
                return named.Name;
            }
            else if (record is INamedRequiredGetter namedRequired)
            {
                return namedRequired.Name;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INamedGetter for {PropertyName}");
            }
            return null;
        }
    }
}
