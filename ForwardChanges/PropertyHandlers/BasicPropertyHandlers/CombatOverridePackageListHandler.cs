using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Mutagen.Bethesda.Plugins;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class CombatOverridePackageListHandler : AbstractPropertyHandler<IFormLinkNullableGetter<IFormListGetter>>
    {
        public override string PropertyName => "CombatOverridePackageList";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<IFormListGetter>? value)
        {
            if (record is INpc npc)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    npc.CombatOverridePackageList = new FormLinkNullable<IFormListGetter>(value.FormKey);
                }
                else
                {
                    npc.CombatOverridePackageList.Clear();
                }
            }
        }

        public override IFormLinkNullableGetter<IFormListGetter>? GetValue(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (context.Record is INpcGetter npc)
            {
                return npc.CombatOverridePackageList;
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<IFormListGetter>? value1, IFormLinkNullableGetter<IFormListGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}