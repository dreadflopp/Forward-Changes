using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Faction
{
    public class FlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Faction.FactionFlag>
    {
        public override string PropertyName => "Flags";

        public override Mutagen.Bethesda.Skyrim.Faction.FactionFlag GetValue(IMajorRecordGetter record)
        {
            if (record is IFactionGetter factionRecord)
            {
                return factionRecord.Flags;
            }

            Console.WriteLine($"Error: Record does not implement IFactionGetter for {PropertyName}");
            return Mutagen.Bethesda.Skyrim.Faction.FactionFlag.HiddenFromPC; // Default value
        }

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Faction.FactionFlag value)
        {
            if (record is IFaction factionRecord)
            {
                factionRecord.Flags = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IFaction for {PropertyName}");
            }
        }

        protected override Mutagen.Bethesda.Skyrim.Faction.FactionFlag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.Faction.FactionFlag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Faction.FactionFlag flags, Mutagen.Bethesda.Skyrim.Faction.FactionFlag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Mutagen.Bethesda.Skyrim.Faction.FactionFlag SetFlag(Mutagen.Bethesda.Skyrim.Faction.FactionFlag flags, Mutagen.Bethesda.Skyrim.Faction.FactionFlag flag, bool value)
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

        protected override string FormatFlag(Mutagen.Bethesda.Skyrim.Faction.FactionFlag flag)
        {
            return flag.ToString();
        }
    }
}

