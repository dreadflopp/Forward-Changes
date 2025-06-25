using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.FlagPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.FlagPropertyHandlers
{
    public class IngestibleFlagsPropertyHandler : AbstractFlagPropertyHandler<Ingestible.Flag>
    {
        public override string PropertyName => "Flags";

        public override void SetValue(IMajorRecord record, Ingestible.Flag value)
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

        public override Ingestible.Flag GetValue(IMajorRecordGetter record)
        {
            if (record is IIngestibleGetter ingestible)
            {
                return ingestible.Flags;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IIngestibleGetter for {PropertyName}");
            }
            return Ingestible.Flag.NoAutoCalc;
        }

        protected override Ingestible.Flag[] GetAllFlags()
        {
            return new[]
            {
                Ingestible.Flag.NoAutoCalc,
                Ingestible.Flag.FoodItem,
                Ingestible.Flag.Medicine,
                Ingestible.Flag.Poison
            };
        }

        protected override bool IsFlagSet(Ingestible.Flag flags, Ingestible.Flag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Ingestible.Flag SetFlag(Ingestible.Flag flags, Ingestible.Flag flag, bool value)
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

        protected override string FormatFlag(Ingestible.Flag flag)
        {
            return flag.ToString();
        }
    }
}