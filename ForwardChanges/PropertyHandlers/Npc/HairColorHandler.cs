using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class HairColorHandler : AbstractFormLinkPropertyHandler<INpc, INpcGetter, IColorRecordGetter>
    {
        public override string PropertyName => "HairColor";

        protected override IFormLinkNullableGetter<IColorRecordGetter>? GetFormLinkValue(INpcGetter record)
        {
            return record.HairColor;
        }

        protected override void SetFormLinkValue(INpc record, IFormLinkNullableGetter<IColorRecordGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.HairColor = new FormLinkNullable<IColorRecordGetter>(value.FormKey);
            }
            else
            {
                record.HairColor.Clear();
            }
        }
    }
}