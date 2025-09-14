using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class PersistentLocationHandler : AbstractFormLinkPropertyHandler<IPlacedObject, IPlacedObjectGetter, ILocationGetter>
    {
        public override string PropertyName => "PersistentLocation";

        protected override IFormLinkNullableGetter<ILocationGetter> GetFormLinkValue(IPlacedObjectGetter record)
        {
            return record.PersistentLocation;
        }

        protected override void SetFormLinkValue(IPlacedObject record, IFormLinkNullableGetter<ILocationGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.PersistentLocation = new FormLinkNullable<ILocationGetter>(value.FormKey);
            }
            else
            {
                record.PersistentLocation.Clear();
            }
        }
    }
}