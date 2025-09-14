using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class TeleportMessageBoxHandler : AbstractFormLinkPropertyHandler<IPlacedObject, IPlacedObjectGetter, IMessageGetter>
    {
        public override string PropertyName => "TeleportMessageBox";

        protected override IFormLinkNullableGetter<IMessageGetter> GetFormLinkValue(IPlacedObjectGetter record)
        {
            return record.TeleportMessageBox;
        }

        protected override void SetFormLinkValue(IPlacedObject record, IFormLinkNullableGetter<IMessageGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.TeleportMessageBox = new FormLinkNullable<IMessageGetter>(value.FormKey);
            }
            else
            {
                record.TeleportMessageBox.Clear();
            }
        }
    }
}