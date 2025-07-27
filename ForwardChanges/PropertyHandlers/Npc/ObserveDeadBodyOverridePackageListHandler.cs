using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Mutagen.Bethesda.Plugins;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class ObserveDeadBodyOverridePackageListHandler : AbstractPropertyHandler<IFormLinkNullableGetter<IFormListGetter>>
    {
        public override string PropertyName => "ObserveDeadBodyOverridePackageList";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<IFormListGetter>? value)
        {
            if (record is INpc npc)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    npc.ObserveDeadBodyOverridePackageList = new FormLinkNullable<IFormListGetter>(value.FormKey);
                }
                else
                {
                    npc.ObserveDeadBodyOverridePackageList.Clear();
                }
            }
            else
            {
                Console.WriteLine($"Error: Record is not an NPC for {PropertyName}");
            }
        }

        public override IFormLinkNullableGetter<IFormListGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npc)
            {
                return npc.ObserveDeadBodyOverridePackageList;
            }
            else
            {
                Console.WriteLine($"Error: Record is not an NPC for {PropertyName}");
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
