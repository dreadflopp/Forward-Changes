using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class NpcDeathItemPropertyHandler : AbstractPropertyHandler<IFormLinkGetter<ILeveledItemGetter>>
    {
        public override string PropertyName => "DeathItem";

        public override void SetValue(IMajorRecord record, IFormLinkGetter<ILeveledItemGetter>? value)
        {
            if (record is INpc npc)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    npc.DeathItem = new FormLinkNullable<ILeveledItemGetter>(value.FormKey);
                }
                else
                {
                    npc.DeathItem.Clear();
                }
            }
            else
            {
                Console.WriteLine($"Error: Record is not an NPC for {PropertyName}");
            }
        }

        public override IFormLinkGetter<ILeveledItemGetter>? GetValue(
            IMajorRecordGetter record)
        {
            if (record is INpcGetter npc)
            {
                return npc.DeathItem;
            }
            else
            {
                Console.WriteLine($"Error: Record is not an NPC for {PropertyName}");
                return null;
            }
        }

        public override bool AreValuesEqual(IFormLinkGetter<ILeveledItemGetter>? value1, IFormLinkGetter<ILeveledItemGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}