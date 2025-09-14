using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.EncounterZone
{
    public class FlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.EncounterZone.Flag>
    {
        public override string PropertyName => "Flags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.EncounterZone.Flag value)
        {
            if (record is IEncounterZone encounterZoneRecord)
            {
                encounterZoneRecord.Flags = value;
            }
        }

        public override Mutagen.Bethesda.Skyrim.EncounterZone.Flag GetValue(IMajorRecordGetter record)
        {
            if (record is IEncounterZoneGetter encounterZoneRecord)
            {
                return encounterZoneRecord.Flags;
            }
            return 0;
        }

        protected override Mutagen.Bethesda.Skyrim.EncounterZone.Flag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.EncounterZone.Flag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.EncounterZone.Flag flags, Mutagen.Bethesda.Skyrim.EncounterZone.Flag flag)
        {
            return flags.HasFlag(flag);
        }

        protected override Mutagen.Bethesda.Skyrim.EncounterZone.Flag SetFlag(Mutagen.Bethesda.Skyrim.EncounterZone.Flag flags, Mutagen.Bethesda.Skyrim.EncounterZone.Flag flag, bool value)
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
    }
}