using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class NpcConfigurationMagickaOffsetPropertyHandler : AbstractPropertyHandler<short>
    {
        public override string PropertyName => "Configuration.MagickaOffset";

        public override void SetValue(IMajorRecord record, short value)
        {
            if (record is INpc npc)
            {
                npc.Configuration.MagickaOffset = value;
            }
            else
            {
                Console.WriteLine($"Error: Record is not an NPC for {PropertyName}");
            }
        }

        public override short GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npc)
            {
                return npc.Configuration.MagickaOffset;
            }
            else
            {
                Console.WriteLine($"Error: Record is not an NPC for {PropertyName}");
            }
            return 0;
        }

        public override bool AreValuesEqual(short value1, short value2)
        {
            return value1 == value2;
        }
    }
}