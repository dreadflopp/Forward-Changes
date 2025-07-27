using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Spell
{
    public class FlagHandler : AbstractFlagPropertyHandler<SpellDataFlag>
    {
        public override string PropertyName => "Flags";

        public override void SetValue(IMajorRecord record, SpellDataFlag value)
        {
            if (record is ISpell spell)
            {
                spell.Flags = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ISpell for {PropertyName}");
            }
        }

        public override SpellDataFlag GetValue(IMajorRecordGetter record)
        {
            if (record is ISpellGetter spell)
            {
                return spell.Flags;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ISpellGetter for {PropertyName}");
            }
            return SpellDataFlag.ManualCostCalc;
        }

        protected override SpellDataFlag[] GetAllFlags()
        {
            return new[]
            {
                SpellDataFlag.ManualCostCalc,
                SpellDataFlag.PCStartSpell,
                SpellDataFlag.AreaEffectIgnoresLOS,
                SpellDataFlag.IgnoreResistance,
                SpellDataFlag.NoAbsorbOrReflect,
                SpellDataFlag.NoDualCastModification
            };
        }

        protected override bool IsFlagSet(SpellDataFlag flags, SpellDataFlag flag)
        {
            return (flags & flag) == flag;
        }

        protected override SpellDataFlag SetFlag(SpellDataFlag flags, SpellDataFlag flag, bool value)
        {
            if (value)
            {
                return flags | flag;
            }
            else
            {
                return flags & ~flag;
            }
        }

        protected override string FormatFlag(SpellDataFlag flag)
        {
            return flag.ToString();
        }
    }
}

