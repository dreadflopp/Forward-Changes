using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class DeathItemPropertyHandler : AbstractPropertyHandler
    {
        public override string PropertyName => "DeathItem";

        public override void SetValue(IMajorRecord record, object? value)
        {
            if (record is INpc npc)
            {
                if (value is IFormLinkNullableGetter<ILeveledItemGetter> item && !item.FormKey.IsNull)
                {
                    npc.DeathItem = new FormLinkNullable<ILeveledItemGetter>(item.FormKey);
                }
                else
                {
                    npc.DeathItem.Clear();
                }
            }
        }

        public override object? GetValue(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (context.Record is INpcGetter npc)
            {
                return npc.DeathItem;
            }
            return null;
        }

        public override bool AreValuesEqual(object? value1, object? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1 is IFormLinkNullableGetter<ILeveledItemGetter> item1 &&
                value2 is IFormLinkNullableGetter<ILeveledItemGetter> item2)
            {
                return item1.FormKey.Equals(item2.FormKey);
            }
            return false;
        }
    }
}