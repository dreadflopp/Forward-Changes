using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Ingestible
{
    public class MajorFlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Ingestible.MajorFlag>
    {
        public override string PropertyName => "MajorFlags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Ingestible.MajorFlag value)
        {
            if (record is IIngestible ingestible)
            {
                ingestible.MajorFlags = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IIngestible for {PropertyName}");
            }
        }

        public override Mutagen.Bethesda.Skyrim.Ingestible.MajorFlag GetValue(IMajorRecordGetter record)
        {
            if (record is IIngestibleGetter ingestible)
            {
                return ingestible.MajorFlags;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IIngestibleGetter for {PropertyName}");
            }
            return default(Mutagen.Bethesda.Skyrim.Ingestible.MajorFlag);
        }

        protected override Mutagen.Bethesda.Skyrim.Ingestible.MajorFlag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.Ingestible.MajorFlag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Ingestible.MajorFlag flags, Mutagen.Bethesda.Skyrim.Ingestible.MajorFlag flag)
        {
            return flags.HasFlag(flag);
        }

        protected override Mutagen.Bethesda.Skyrim.Ingestible.MajorFlag SetFlag(Mutagen.Bethesda.Skyrim.Ingestible.MajorFlag flags, Mutagen.Bethesda.Skyrim.Ingestible.MajorFlag flag, bool value)
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

