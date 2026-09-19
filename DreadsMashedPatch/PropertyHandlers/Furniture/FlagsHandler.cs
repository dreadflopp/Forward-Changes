using System;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using DreadsMashedPatch.PropertyHandlers.Abstracts;

namespace DreadsMashedPatch.PropertyHandlers.Furniture
{
    public class FlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.Furniture.Flag>
    {
        public override string PropertyName => "Flags";

        public override Mutagen.Bethesda.Skyrim.Furniture.Flag GetValue(IMajorRecordGetter record)
        {
            if (record is IFurnitureGetter furniture)
            {
                return furniture.Flags ?? 0;
            }

            return 0;
        }

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.Furniture.Flag value)
        {
            if (record is IFurniture furniture)
            {
                furniture.Flags = value;
            }
        }

        protected override Mutagen.Bethesda.Skyrim.Furniture.Flag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.Furniture.Flag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.Furniture.Flag flags, Mutagen.Bethesda.Skyrim.Furniture.Flag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Mutagen.Bethesda.Skyrim.Furniture.Flag SetFlag(Mutagen.Bethesda.Skyrim.Furniture.Flag flags, Mutagen.Bethesda.Skyrim.Furniture.Flag flag, bool value)
        {
            if (value)
            {
                return flags | flag;
            }

            return flags & ~flag;
        }
    }
}
