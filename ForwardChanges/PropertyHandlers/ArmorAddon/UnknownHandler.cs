using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ArmorAddon
{
    public class UnknownHandler : AbstractPropertyHandler<ushort>
    {
        public override string PropertyName => "Unknown";

        public override void SetValue(IMajorRecord record, ushort value)
        {
            var armorAddonRecord = TryCastRecord<IArmorAddon>(record, PropertyName);
            if (armorAddonRecord != null)
            {
                armorAddonRecord.Unknown = value;
            }
        }

        public override ushort GetValue(IMajorRecordGetter record)
        {
            var armorAddonRecord = TryCastRecord<IArmorAddonGetter>(record, PropertyName);
            return armorAddonRecord?.Unknown ?? 0;
        }
    }
}