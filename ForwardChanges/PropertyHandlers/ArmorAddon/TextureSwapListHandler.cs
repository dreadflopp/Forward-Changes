using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Noggog;

namespace ForwardChanges.PropertyHandlers.ArmorAddon
{
    public class TextureSwapListHandler : AbstractPropertyHandler<IGenderedItemGetter<IFormLinkNullableGetter<IFormListGetter>>>
    {
        public override string PropertyName => "TextureSwapList";

        public override void SetValue(IMajorRecord record, IGenderedItemGetter<IFormLinkNullableGetter<IFormListGetter>>? value)
        {
            var armorAddonRecord = TryCastRecord<IArmorAddon>(record, PropertyName);
            if (armorAddonRecord != null)
            {
                if (value == null)
                {
                    // TextureSwapList might not accept null, create a default gendered item with null form links
                    var nullFormLink = new FormLinkNullable<IFormListGetter>();
                    armorAddonRecord.TextureSwapList = new GenderedItem<IFormLinkNullableGetter<IFormListGetter>>(nullFormLink, nullFormLink);
                }
                else
                {
                    // Create a new GenderedItem with the values from the getter
                    var newGenderedItem = new GenderedItem<IFormLinkNullableGetter<IFormListGetter>>(value.Male, value.Female);
                    armorAddonRecord.TextureSwapList = newGenderedItem;
                }
            }
        }

        public override IGenderedItemGetter<IFormLinkNullableGetter<IFormListGetter>>? GetValue(IMajorRecordGetter record)
        {
            var armorAddonRecord = TryCastRecord<IArmorAddonGetter>(record, PropertyName);
            if (armorAddonRecord != null)
            {
                return armorAddonRecord.TextureSwapList;
            }
            return null;
        }


    }
}