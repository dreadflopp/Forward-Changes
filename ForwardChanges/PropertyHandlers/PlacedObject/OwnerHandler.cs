using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class OwnerHandler : AbstractFormLinkPropertyHandler<IPlacedObject, IPlacedObjectGetter, IOwnerGetter>
    {
        public override string PropertyName => "Owner";

        protected override IFormLinkNullableGetter<IOwnerGetter>? GetFormLinkValue(IPlacedObjectGetter record)
        {
            return record.Owner;
        }

        protected override void SetFormLinkValue(IPlacedObject record, IFormLinkNullableGetter<IOwnerGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.Owner = new FormLinkNullable<IOwnerGetter>(value.FormKey);
            }
            else
            {
                record.Owner.Clear();
            }
        }
    }
}

