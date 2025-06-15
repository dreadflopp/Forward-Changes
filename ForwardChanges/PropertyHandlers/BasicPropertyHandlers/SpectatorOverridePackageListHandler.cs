using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class SpectatorOverridePackageListHandler : AbstractPropertyHandler
    {
        public override string PropertyName => "SpectatorOverridePackageList";

        public override object? GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npc)
                return npc.SpectatorOverridePackageList;
            return null;
        }

        public override void SetValue(IMajorRecord record, object? value)
        {
            if (record is INpc npc)
            {
                if (value is IFormLinkNullableGetter<IFormListGetter> item && !item.FormKey.IsNull)
                {
                    npc.SpectatorOverridePackageList = new FormLinkNullable<IFormListGetter>(item.FormKey);
                }
                else
                {
                    npc.SpectatorOverridePackageList.Clear();
                }
            }
        }

        public override object? GetValueFromContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (context.Record is INpcGetter npc)
            {
                return npc.SpectatorOverridePackageList;
            }
            return null;
        }

        public override bool AreValuesEqual(object? value1, object? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1 is IFormLinkNullableGetter<IFormListGetter> item1 &&
                value2 is IFormLinkNullableGetter<IFormListGetter> item2)
            {
                return item1.FormKey.Equals(item2.FormKey);
            }
            return false;
        }
    }
}