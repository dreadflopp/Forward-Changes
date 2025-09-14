using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ArmorAddon
{
    public class BodyTemplateFirstPersonFlagsHandler : AbstractFlagPropertyHandler<BipedObjectFlag>
    {
        public override string PropertyName => "BodyTemplateFirstPersonFlags";

        public override void SetValue(IMajorRecord record, BipedObjectFlag value)
        {
            if (record is IArmorAddon armorAddonRecord && armorAddonRecord.BodyTemplate != null)
            {
                armorAddonRecord.BodyTemplate.FirstPersonFlags = value;
            }
        }

        public override BipedObjectFlag GetValue(IMajorRecordGetter record)
        {
            if (record is IArmorAddonGetter armorAddonRecord && armorAddonRecord.BodyTemplate != null)
            {
                return armorAddonRecord.BodyTemplate.FirstPersonFlags;
            }
            return BipedObjectFlag.Head;
        }

        protected override BipedObjectFlag[] GetAllFlags()
        {
            return Enum.GetValues<BipedObjectFlag>();
        }

        protected override bool IsFlagSet(BipedObjectFlag flags, BipedObjectFlag flag)
        {
            return (flags & flag) == flag;
        }

        protected override BipedObjectFlag SetFlag(BipedObjectFlag flags, BipedObjectFlag flag, bool value)
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