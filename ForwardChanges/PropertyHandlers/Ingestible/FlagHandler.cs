using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Ingestible
{
    public class FlagHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Ingestible.Flag>
    {
        public override string PropertyName => "Flags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Ingestible.Flag value)
        {
            if (record is IIngestible ingestible)
            {
                ingestible.Flags = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IIngestible for {PropertyName}");
            }
        }

        public override Mutagen.Bethesda.Skyrim.Ingestible.Flag GetValue(IMajorRecordGetter record)
        {
            if (record is IIngestibleGetter ingestible)
            {
                return ingestible.Flags;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IIngestibleGetter for {PropertyName}");
            }
            return Mutagen.Bethesda.Skyrim.Ingestible.Flag.NoAutoCalc;
        }

        protected override Mutagen.Bethesda.Skyrim.Ingestible.Flag[] GetAllFlags()
        {
            return new[]
            {
                Mutagen.Bethesda.Skyrim.Ingestible.Flag.NoAutoCalc,
                Mutagen.Bethesda.Skyrim.Ingestible.Flag.FoodItem,
                Mutagen.Bethesda.Skyrim.Ingestible.Flag.Medicine,
                Mutagen.Bethesda.Skyrim.Ingestible.Flag.Poison
            };
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Ingestible.Flag flags, Mutagen.Bethesda.Skyrim.Ingestible.Flag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Mutagen.Bethesda.Skyrim.Ingestible.Flag SetFlag(Mutagen.Bethesda.Skyrim.Ingestible.Flag flags, Mutagen.Bethesda.Skyrim.Ingestible.Flag flag, bool value)
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

        protected override string FormatFlag(Mutagen.Bethesda.Skyrim.Ingestible.Flag flag)
        {
            return flag.ToString();
        }
    }
}

