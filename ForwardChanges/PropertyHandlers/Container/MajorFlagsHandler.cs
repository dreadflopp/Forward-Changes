using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Container
{
    public class MajorFlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Container.MajorFlag>
    {
        public override string PropertyName => "MajorFlags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Container.MajorFlag value)
        {
            if (record is IContainer container)
            {
                container.MajorFlags = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IContainer for {PropertyName}");
            }
        }

        public override Mutagen.Bethesda.Skyrim.Container.MajorFlag GetValue(IMajorRecordGetter record)
        {
            if (record is IContainerGetter container)
            {
                return container.MajorFlags;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IContainerGetter for {PropertyName}");
            }
            return default(Mutagen.Bethesda.Skyrim.Container.MajorFlag);
        }

        protected override Mutagen.Bethesda.Skyrim.Container.MajorFlag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.Container.MajorFlag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Container.MajorFlag flags, Mutagen.Bethesda.Skyrim.Container.MajorFlag flag)
        {
            return flags.HasFlag(flag);
        }

        protected override Mutagen.Bethesda.Skyrim.Container.MajorFlag SetFlag(Mutagen.Bethesda.Skyrim.Container.MajorFlag flags, Mutagen.Bethesda.Skyrim.Container.MajorFlag flag, bool value)
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