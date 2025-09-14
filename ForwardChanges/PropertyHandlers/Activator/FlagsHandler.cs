using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Activator
{
    public class FlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Activator.Flag>
    {
        public override string PropertyName => "Flags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Activator.Flag value)
        {
            if (record is IActivator activator)
            {
                activator.Flags = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IActivator for {PropertyName}");
            }
        }

        public override Mutagen.Bethesda.Skyrim.Activator.Flag GetValue(IMajorRecordGetter record)
        {
            if (record is IActivatorGetter activator)
            {
                return activator.Flags ?? default(Mutagen.Bethesda.Skyrim.Activator.Flag);
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IActivatorGetter for {PropertyName}");
            }
            return default(Mutagen.Bethesda.Skyrim.Activator.Flag);
        }


        protected override Mutagen.Bethesda.Skyrim.Activator.Flag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.Activator.Flag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Activator.Flag flags, Mutagen.Bethesda.Skyrim.Activator.Flag flag)
        {
            return flags.HasFlag(flag);
        }

        protected override Mutagen.Bethesda.Skyrim.Activator.Flag SetFlag(Mutagen.Bethesda.Skyrim.Activator.Flag flags, Mutagen.Bethesda.Skyrim.Activator.Flag flag, bool value)
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
