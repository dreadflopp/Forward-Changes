using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Ingestible
{
    public class EquipmentTypeHandler : AbstractFormLinkPropertyHandler<IIngestible, IIngestibleGetter, IEquipTypeGetter>
    {
        public override string PropertyName => "EquipmentType";

        protected override IFormLinkNullableGetter<IEquipTypeGetter>? GetFormLinkValue(IIngestibleGetter record)
        {
            return record.EquipmentType;
        }

        protected override void SetFormLinkValue(IIngestible record, IFormLinkNullableGetter<IEquipTypeGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.EquipmentType = new FormLinkNullable<IEquipTypeGetter>(value.FormKey);
            }
            else
            {
                record.EquipmentType.Clear();
            }
        }
    }
}