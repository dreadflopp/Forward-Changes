using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Activator
{
    public class ActivateTextOverrideHandler : AbstractPropertyHandler<ITranslatedStringGetter?>
    {
        public override string PropertyName => "ActivateTextOverride";

        public override void SetValue(IMajorRecord record, ITranslatedStringGetter? value)
        {
            if (record is IActivator activator)
            {
                if (value == null)
                {
                    activator.ActivateTextOverride = null;
                }
                else
                {
                    // Create a deep copy
                    var newText = new TranslatedString(Language.English);
                    newText.String = value.String;
                    activator.ActivateTextOverride = newText;
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IActivator for {PropertyName}");
            }
        }

        public override ITranslatedStringGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IActivatorGetter activator)
            {
                return activator.ActivateTextOverride;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IActivatorGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(ITranslatedStringGetter? value1, ITranslatedStringGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare target language and default string
            if (value1.TargetLanguage != value2.TargetLanguage ||
                value1.String != value2.String ||
                value1.NumLanguages != value2.NumLanguages)
            {
                return false;
            }

            // Compare all language mappings
            foreach (var kvp1 in value1)
            {
                if (!value2.TryLookup(kvp1.Key, out var str2) || str2 != kvp1.Value)
                {
                    return false;
                }
            }

            return true;
        }

        public override string FormatValue(object? value)
        {
            if (value is ITranslatedStringGetter translatedString)
            {
                return $"\"{translatedString.String}\"";
            }
            return value?.ToString() ?? "null";
        }
    }
}
