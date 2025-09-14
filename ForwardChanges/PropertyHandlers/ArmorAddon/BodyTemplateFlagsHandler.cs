using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ArmorAddon
{
    public class BodyTemplateFlagsHandler : AbstractFlagPropertyHandler<BodyTemplate.Flag>
    {
        public override string PropertyName => "BodyTemplateFlags";

        public override void SetValue(IMajorRecord record, BodyTemplate.Flag value)
        {
            if (record is IArmorAddon armorAddonRecord && armorAddonRecord.BodyTemplate != null)
            {
                armorAddonRecord.BodyTemplate.Flags = value;
            }
        }

        public override BodyTemplate.Flag GetValue(IMajorRecordGetter record)
        {
            if (record is IArmorAddonGetter armorAddonRecord && armorAddonRecord.BodyTemplate != null)
            {
                return armorAddonRecord.BodyTemplate.Flags;
            }
            return BodyTemplate.Flag.ModulatesVoice;
        }

        protected override BodyTemplate.Flag[] GetAllFlags()
        {
            return Enum.GetValues<BodyTemplate.Flag>();
        }

        protected override bool IsFlagSet(BodyTemplate.Flag flags, BodyTemplate.Flag flag)
        {
            return (flags & flag) == flag;
        }

        protected override BodyTemplate.Flag SetFlag(BodyTemplate.Flag flags, BodyTemplate.Flag flag, bool value)
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