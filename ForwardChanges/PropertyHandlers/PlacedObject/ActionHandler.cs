using System;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class ActionHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.PlacedObject.ActionFlag>
    {
        public override string PropertyName => "Action";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.PlacedObject.ActionFlag value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                placedObjectRecord.Action = value;
            }
        }

        public override Mutagen.Bethesda.Skyrim.PlacedObject.ActionFlag GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.Action ?? default;
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return default;
        }

        protected override Mutagen.Bethesda.Skyrim.PlacedObject.ActionFlag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.PlacedObject.ActionFlag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.PlacedObject.ActionFlag flags, Mutagen.Bethesda.Skyrim.PlacedObject.ActionFlag flag)
        {
            return flags.HasFlag(flag);
        }

        protected override Mutagen.Bethesda.Skyrim.PlacedObject.ActionFlag SetFlag(Mutagen.Bethesda.Skyrim.PlacedObject.ActionFlag flags, Mutagen.Bethesda.Skyrim.PlacedObject.ActionFlag flag, bool value)
        {
            return value ? flags | flag : flags & ~flag;
        }
    }
}