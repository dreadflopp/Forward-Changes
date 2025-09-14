using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class PlayerSkillsMagickaHandler : AbstractPropertyHandler<ushort>
    {
        public override string PropertyName => "PlayerSkills.Magicka";

        public override ushort GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npcRecord)
            {
                return npcRecord.PlayerSkills?.Magicka ?? 0;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpcGetter for {PropertyName}");
            }
            return 0;
        }

        public override void SetValue(IMajorRecord record, ushort value)
        {
            if (record is INpc npcRecord)
            {
                if (npcRecord.PlayerSkills == null)
                {
                    // Create new PlayerSkills if it doesn't exist
                    npcRecord.PlayerSkills = new PlayerSkills();
                }
                npcRecord.PlayerSkills.Magicka = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpc for {PropertyName}");
            }
        }

        public override bool AreValuesEqual(ushort value1, ushort value2)
        {
            return value1 == value2;
        }
    }
}