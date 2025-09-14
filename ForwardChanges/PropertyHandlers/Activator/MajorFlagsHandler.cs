using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Activator
{
    public class MajorFlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Activator.MajorFlag>
    {
        public override string PropertyName => "MajorFlags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Activator.MajorFlag value)
        {
            if (record is IActivator activator)
            {
                activator.MajorFlags = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IActivator for {PropertyName}");
            }
        }

        public override Mutagen.Bethesda.Skyrim.Activator.MajorFlag GetValue(IMajorRecordGetter record)
        {
            if (record is IActivatorGetter activator)
            {
                return activator.MajorFlags;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IActivatorGetter for {PropertyName}");
            }
            return default(Mutagen.Bethesda.Skyrim.Activator.MajorFlag);
        }

        protected override Mutagen.Bethesda.Skyrim.Activator.MajorFlag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.Activator.MajorFlag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Activator.MajorFlag flags, Mutagen.Bethesda.Skyrim.Activator.MajorFlag flag)
        {
            return flags.HasFlag(flag);
        }

        protected override Mutagen.Bethesda.Skyrim.Activator.MajorFlag SetFlag(Mutagen.Bethesda.Skyrim.Activator.MajorFlag flags, Mutagen.Bethesda.Skyrim.Activator.MajorFlag flag, bool value)
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
