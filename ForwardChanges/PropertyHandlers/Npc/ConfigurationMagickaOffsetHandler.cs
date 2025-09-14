using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class ConfigurationMagickaOffsetHandler : AbstractPropertyHandler<short>
    {
        public override string PropertyName => "Configuration.MagickaOffset";

        public override void SetValue(IMajorRecord record, short value)
        {
            var npc = TryCastRecord<INpc>(record, PropertyName);
            if (npc != null)
            {
                npc.Configuration.MagickaOffset = value;
            }
        }

        public override short GetValue(IMajorRecordGetter record)
        {
            var npc = TryCastRecord<INpcGetter>(record, PropertyName);
            if (npc != null)
            {
                return npc.Configuration.MagickaOffset;
            }
            return 0;
        }


    }
}

