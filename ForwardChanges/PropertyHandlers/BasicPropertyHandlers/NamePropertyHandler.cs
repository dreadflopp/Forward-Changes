using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Mutagen.Bethesda.Strings;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class NamePropertyHandler : AbstractPropertyHandler<string>
    {
        public override string PropertyName => "Name";

        public override void SetValue(IMajorRecord record, string? value)
        {
            if (record is INpc npc)
            {
                if (value != null)
                {
                    var translatedString = new TranslatedString(Language.English);
                    translatedString.String = value;
                    npc.Name = translatedString;
                }
                else
                {
                    npc.Name = null;
                }
            }
        }

        public override string? GetValue(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (context.Record is INpcGetter npc)
            {
                return npc.Name?.String;
            }
            return null;
        }
    }
}