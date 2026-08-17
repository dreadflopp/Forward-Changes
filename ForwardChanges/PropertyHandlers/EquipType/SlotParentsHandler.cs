using System.Collections.Generic;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.EquipType
{
    public class SlotParentsHandler : AbstractListPropertyHandler<IFormLinkGetter<IEquipTypeGetter>>
    {
        public override string PropertyName => "SlotParents";

        public override List<IFormLinkGetter<IEquipTypeGetter>>? GetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecordGetter record)
        {
            if (record is IEquipTypeGetter equipType)
            {
                return equipType.SlotParents?.ToList();
            }

            return null;
        }

        public override void SetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecord record, List<IFormLinkGetter<IEquipTypeGetter>>? value)
        {
            if (record is not IEquipType equipType)
            {
                return;
            }

            if (value == null)
            {
                equipType.SlotParents = null;
                return;
            }

            equipType.SlotParents ??= new ExtendedList<IFormLinkGetter<IEquipTypeGetter>>();
            equipType.SlotParents.Clear();

            foreach (var link in value)
            {
                if (link == null || link.FormKey.IsNull) continue;
                equipType.SlotParents.Add(new FormLink<IEquipTypeGetter>(link.FormKey));
            }
        }
    }
}
