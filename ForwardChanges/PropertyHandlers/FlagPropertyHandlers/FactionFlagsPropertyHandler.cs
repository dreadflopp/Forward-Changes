using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.FlagPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.FlagPropertyHandlers
{
    public class FactionFlagsPropertyHandler : AbstractFlagPropertyHandler<Faction.FactionFlag>
    {
        public override string PropertyName => "Flags";

        public override Faction.FactionFlag GetValue(IMajorRecordGetter record)
        {
            if (record is IFactionGetter factionRecord)
            {
                return factionRecord.Flags;
            }

            Console.WriteLine($"Error: Record does not implement IFactionGetter for {PropertyName}");
            return Faction.FactionFlag.HiddenFromPC; // Default value
        }

        public override void SetValue(IMajorRecord record, Faction.FactionFlag value)
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

        protected override Faction.FactionFlag[] GetAllFlags()
        {
            return new[]
            {
                Faction.FactionFlag.HiddenFromPC,
                Faction.FactionFlag.SpecialCombat,
                Faction.FactionFlag.TrackCrime,
                Faction.FactionFlag.IgnoreMurder,
                Faction.FactionFlag.IgnoreAssault,
                Faction.FactionFlag.IgnoreStealing,
                Faction.FactionFlag.IgnoreTrespass,
                Faction.FactionFlag.DoNotReportCrimesAgainstMembers,
                Faction.FactionFlag.CrimeGoldUseDefaults,
                Faction.FactionFlag.IgnorePickpocket,
                Faction.FactionFlag.Vendor,
                Faction.FactionFlag.CanBeOwner,
                Faction.FactionFlag.IgnoreWerewolf
            };
        }

        protected override bool IsFlagSet(Faction.FactionFlag flags, Faction.FactionFlag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Faction.FactionFlag SetFlag(Faction.FactionFlag flags, Faction.FactionFlag flag, bool value)
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

        protected override string FormatFlag(Faction.FactionFlag flag)
        {
            return flag.ToString();
        }
    }
}