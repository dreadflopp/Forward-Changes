using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class AttacksHandler : AbstractListPropertyHandler<IAttackGetter>
    {
        public override string PropertyName => "Attacks";

        public override List<IAttackGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npcRecord)
            {
                return npcRecord.Attacks?.ToList();
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpcGetter for {PropertyName}");
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, List<IAttackGetter>? value)
        {
            if (record is INpc npcRecord)
            {
                if (value == null)
                {
                    npcRecord.Attacks.Clear();
                    return;
                }

                // Clear existing attacks and add new ones
                npcRecord.Attacks.Clear();
                foreach (var attack in value)
                {
                    if (attack != null)
                    {
                        // Create a deep copy of the attack
                        var newAttack = new Attack
                        {
                            AttackEvent = attack.AttackEvent
                        };

                        // Deep copy AttackData if it exists
                        if (attack.AttackData != null)
                        {
                            var newAttackData = new AttackData
                            {
                                DamageMult = attack.AttackData.DamageMult,
                                Chance = attack.AttackData.Chance,
                                Spell = new FormLink<ISpellRecordGetter>(attack.AttackData.Spell.FormKey),
                                Flags = attack.AttackData.Flags,
                                AttackAngle = attack.AttackData.AttackAngle,
                                StrikeAngle = attack.AttackData.StrikeAngle,
                                Stagger = attack.AttackData.Stagger,
                                AttackType = new FormLink<IKeywordGetter>(attack.AttackData.AttackType.FormKey),
                                Knockdown = attack.AttackData.Knockdown,
                                RecoveryTime = attack.AttackData.RecoveryTime,
                                StaminaMult = attack.AttackData.StaminaMult
                            };
                            newAttack.AttackData = newAttackData;
                        }

                        npcRecord.Attacks.Add(newAttack);
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpc for {PropertyName}");
            }
        }

        protected override bool IsItemEqual(IAttackGetter? item1, IAttackGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Compare AttackEvent
            if (item1.AttackEvent != item2.AttackEvent) return false;

            // Compare AttackData
            if (item1.AttackData == null && item2.AttackData == null) return true;
            if (item1.AttackData == null || item2.AttackData == null) return false;

            var data1 = item1.AttackData;
            var data2 = item2.AttackData;

            return data1.DamageMult == data2.DamageMult &&
                   data1.Chance == data2.Chance &&
                   data1.Spell.FormKey == data2.Spell.FormKey &&
                   data1.Flags == data2.Flags &&
                   data1.AttackAngle == data2.AttackAngle &&
                   data1.StrikeAngle == data2.StrikeAngle &&
                   data1.Stagger == data2.Stagger &&
                   data1.AttackType.FormKey == data2.AttackType.FormKey &&
                   data1.Knockdown == data2.Knockdown &&
                   data1.RecoveryTime == data2.RecoveryTime &&
                   data1.StaminaMult == data2.StaminaMult;
        }

        protected override string FormatItem(IAttackGetter? item)
        {
            if (item == null) return "null";

            var eventText = item.AttackEvent ?? "NoEvent";
            var dataText = item.AttackData != null ? $"DamageMult: {item.AttackData.DamageMult}, Chance: {item.AttackData.Chance}" : "NoData";

            return $"Attack({eventText}, {dataText})";
        }
    }
}