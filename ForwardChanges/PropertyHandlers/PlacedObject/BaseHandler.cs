using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class BaseHandler : AbstractFormLinkPropertyHandler<IPlacedObject, IPlacedObjectGetter, IPlaceableObjectGetter>
    {
        public override string PropertyName => "Base";

        protected override IFormLinkNullableGetter<IPlaceableObjectGetter>? GetFormLinkValue(IPlacedObjectGetter record)
        {
            return record.Base;
        }

        protected override void SetFormLinkValue(IPlacedObject record, IFormLinkNullableGetter<IPlaceableObjectGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.Base = new FormLinkNullable<IPlaceableObjectGetter>(value.FormKey);
            }
            else
            {
                record.Base.Clear();
            }
        }
    }
}

