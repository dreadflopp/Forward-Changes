using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Location
{
    public class ParentLocationHandler : AbstractFormLinkPropertyHandler<ILocation, ILocationGetter, ILocationGetter>
    {
        public override string PropertyName => "ParentLocation";

        protected override IFormLinkNullableGetter<ILocationGetter>? GetFormLinkValue(ILocationGetter record)
        {
            return record.ParentLocation as IFormLinkNullableGetter<ILocationGetter>;
        }

        protected override void SetFormLinkValue(ILocation record, IFormLinkNullableGetter<ILocationGetter>? value)
        {
            record.ParentLocation = value != null ? new FormLinkNullable<ILocationGetter>(value.FormKey) : new FormLinkNullable<ILocationGetter>();
        }
    }
}

