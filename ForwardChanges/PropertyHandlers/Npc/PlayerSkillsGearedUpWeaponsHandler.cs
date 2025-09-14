using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class PlayerSkillsGearedUpWeaponsHandler : AbstractPropertyHandler<byte>
    {
        public override string PropertyName => "PlayerSkills.GearedUpWeapons";

        public override byte GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npcRecord)
            {
                return npcRecord.PlayerSkills?.GearedUpWeapons ?? 0;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpcGetter for {PropertyName}");
            }
            return 0;
        }

        public override void SetValue(IMajorRecord record, byte value)
        {
            if (record is INpc npcRecord)
            {
                if (npcRecord.PlayerSkills == null)
                {
                    // Create new PlayerSkills if it doesn't exist
                    npcRecord.PlayerSkills = new PlayerSkills();
                }
                npcRecord.PlayerSkills.GearedUpWeapons = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpc for {PropertyName}");
            }
        }

        public override bool AreValuesEqual(byte value1, byte value2)
        {
            return value1 == value2;
        }
    }
}