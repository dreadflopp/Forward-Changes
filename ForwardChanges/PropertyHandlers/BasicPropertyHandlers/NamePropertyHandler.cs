using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Mutagen.Bethesda.Strings;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class NamePropertyHandler : AbstractPropertyHandler<ITranslatedStringGetter>, IPropertyHandler<object>
    {
        public override string PropertyName => "Name";

        public override void SetValue(IMajorRecord record, ITranslatedStringGetter? value)
        {
            if (record is INpc npc)
            {
                if (value != null)
                {
                    var translatedString = new TranslatedString(Language.English);
                    translatedString.String = value.String;
                    npc.Name = translatedString;
                }
                else
                {
                    npc.Name = null;
                }
            }
        }

        public override ITranslatedStringGetter? GetValue(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (context.Record is INpcGetter npc)
            {
                return npc.Name;
            }
            return null;
        }

        public override bool AreValuesEqual(ITranslatedStringGetter? value1, ITranslatedStringGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return string.Equals(value1.String, value2.String, StringComparison.OrdinalIgnoreCase);
        }

        // IPropertyHandler<object> implementation
        void IPropertyHandler<object>.SetValue(IMajorRecord record, object? value) => SetValue(record, (ITranslatedStringGetter?)value);
        object? IPropertyHandler<object>.GetValue(IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context) => GetValue(context);
        bool IPropertyHandler<object>.AreValuesEqual(object? value1, object? value2) => AreValuesEqual((ITranslatedStringGetter?)value1, (ITranslatedStringGetter?)value2);
    }
}