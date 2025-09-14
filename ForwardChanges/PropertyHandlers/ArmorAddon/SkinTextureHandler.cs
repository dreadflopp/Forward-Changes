using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Noggog;

namespace ForwardChanges.PropertyHandlers.ArmorAddon
{
    public class SkinTextureHandler : AbstractPropertyHandler<IGenderedItemGetter<IFormLinkNullableGetter<ITextureSetGetter>>>
    {
        public override string PropertyName => "SkinTexture";

        public override void SetValue(IMajorRecord record, IGenderedItemGetter<IFormLinkNullableGetter<ITextureSetGetter>>? value)
        {
            var armorAddonRecord = TryCastRecord<IArmorAddon>(record, PropertyName);
            if (armorAddonRecord != null)
            {
                if (value == null)
                {
                    // SkinTexture might not accept null, create a default gendered item with null form links
                    var nullFormLink = new FormLinkNullable<ITextureSetGetter>();
                    armorAddonRecord.SkinTexture = new GenderedItem<IFormLinkNullableGetter<ITextureSetGetter>>(nullFormLink, nullFormLink);
                }
                else
                {
                    // Create a new GenderedItem with the values from the getter
                    var newGenderedItem = new GenderedItem<IFormLinkNullableGetter<ITextureSetGetter>>(value.Male, value.Female);
                    armorAddonRecord.SkinTexture = newGenderedItem;
                }
            }
        }

        public override IGenderedItemGetter<IFormLinkNullableGetter<ITextureSetGetter>>? GetValue(IMajorRecordGetter record)
        {
            var armorAddonRecord = TryCastRecord<IArmorAddonGetter>(record, PropertyName);
            if (armorAddonRecord != null)
            {
                return armorAddonRecord.SkinTexture;
            }
            return null;
        }


    }
}