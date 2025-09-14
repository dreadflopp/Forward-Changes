using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class FarAwayModelHandler : AbstractFormLinkPropertyHandler<INpc, INpcGetter, IArmorGetter>
    {
        public override string PropertyName => "FarAwayModel";

        protected override IFormLinkNullableGetter<IArmorGetter>? GetFormLinkValue(INpcGetter record)
        {
            return record.FarAwayModel;
        }

        protected override void SetFormLinkValue(INpc record, IFormLinkNullableGetter<IArmorGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.FarAwayModel = new FormLinkNullable<IArmorGetter>(value.FormKey);
            }
            else
            {
                record.FarAwayModel.Clear();
            }
        }
    }
}