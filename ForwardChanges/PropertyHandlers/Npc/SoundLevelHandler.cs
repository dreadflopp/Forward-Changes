using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class SoundLevelHandler : AbstractPropertyHandler<SoundLevel>
    {
        public override string PropertyName => "SoundLevel";

        public override void SetValue(IMajorRecord record, SoundLevel value)
        {
            var npcRecord = TryCastRecord<INpc>(record, PropertyName);
            if (npcRecord != null)
            {
                npcRecord.SoundLevel = value;
            }
        }

        public override SoundLevel GetValue(IMajorRecordGetter record)
        {
            var npcRecord = TryCastRecord<INpcGetter>(record, PropertyName);
            if (npcRecord != null)
            {
                return npcRecord.SoundLevel;
            }
            return default(SoundLevel);
        }
    }
}