using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class MajorFlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.PlacedNpc.MajorFlag>
    {
        public override string PropertyName => "MajorFlags";

        public override Mutagen.Bethesda.Skyrim.PlacedNpc.MajorFlag GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedNpcGetter placedNpc)
            {
                return placedNpc.MajorFlags;
            }
            return default;
        }

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.PlacedNpc.MajorFlag value)
        {
            if (record is IPlacedNpc placedNpc)
            {
                placedNpc.MajorFlags = value;
            }
        }


        protected override Mutagen.Bethesda.Skyrim.PlacedNpc.MajorFlag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.PlacedNpc.MajorFlag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.PlacedNpc.MajorFlag flags, Mutagen.Bethesda.Skyrim.PlacedNpc.MajorFlag flag)
        {
            return flags.HasFlag(flag);
        }

        protected override Mutagen.Bethesda.Skyrim.PlacedNpc.MajorFlag SetFlag(Mutagen.Bethesda.Skyrim.PlacedNpc.MajorFlag flags, Mutagen.Bethesda.Skyrim.PlacedNpc.MajorFlag flag, bool value)
        {
            if (value)
                return flags | flag;
            else
                return flags & ~flag;
        }
    }
}
