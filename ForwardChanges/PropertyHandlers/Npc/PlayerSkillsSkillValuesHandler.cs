using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class PlayerSkillsSkillValuesHandler : AbstractPropertyHandler<IReadOnlyDictionary<Skill, byte>>
    {
        public override string PropertyName => "PlayerSkills.SkillValues";

        public override IReadOnlyDictionary<Skill, byte>? GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npcRecord)
            {
                return npcRecord.PlayerSkills?.SkillValues;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpcGetter for {PropertyName}");
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, IReadOnlyDictionary<Skill, byte>? value)
        {
            if (record is INpc npcRecord)
            {
                if (npcRecord.PlayerSkills == null)
                {
                    // Create new PlayerSkills if it doesn't exist
                    npcRecord.PlayerSkills = new PlayerSkills();
                }

                if (value == null)
                {
                    // Clear skill values
                    npcRecord.PlayerSkills.SkillValues.Clear();
                }
                else
                {
                    // Clear existing values and add new ones
                    npcRecord.PlayerSkills.SkillValues.Clear();
                    foreach (var kvp in value)
                    {
                        npcRecord.PlayerSkills.SkillValues[kvp.Key] = kvp.Value;
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpc for {PropertyName}");
            }
        }

        public override bool AreValuesEqual(IReadOnlyDictionary<Skill, byte>? value1, IReadOnlyDictionary<Skill, byte>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            if (value1.Count != value2.Count) return false;

            foreach (var kvp in value1)
            {
                if (!value2.TryGetValue(kvp.Key, out var value2Value) || kvp.Value != value2Value)
                {
                    return false;
                }
            }

            return true;
        }
    }
}