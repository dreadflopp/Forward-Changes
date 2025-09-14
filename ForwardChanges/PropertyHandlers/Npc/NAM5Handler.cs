using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class NAM5Handler : AbstractPropertyHandler<ushort>
    {
        public override string PropertyName => "NAM5";

        public override void SetValue(IMajorRecord record, ushort value)
        {
            var npcRecord = TryCastRecord<INpc>(record, PropertyName);
            if (npcRecord != null)
            {
                npcRecord.NAM5 = value;
            }
        }

        public override ushort GetValue(IMajorRecordGetter record)
        {
            var npcRecord = TryCastRecord<INpcGetter>(record, PropertyName);
            if (npcRecord != null)
            {
                return npcRecord.NAM5;
            }
            return 0;
        }
    }
}