using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ArmorAddon
{
    public class Unknown2Handler : AbstractPropertyHandler<byte>
    {
        public override string PropertyName => "Unknown2";

        public override void SetValue(IMajorRecord record, byte value)
        {
            var armorAddonRecord = TryCastRecord<IArmorAddon>(record, PropertyName);
            if (armorAddonRecord != null)
            {
                armorAddonRecord.Unknown2 = value;
            }
        }

        public override byte GetValue(IMajorRecordGetter record)
        {
            var armorAddonRecord = TryCastRecord<IArmorAddonGetter>(record, PropertyName);
            return armorAddonRecord?.Unknown2 ?? 0;
        }
    }
}