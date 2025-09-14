using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class LocationHandler : AbstractFormLinkPropertyHandler<IWorldspace, IWorldspaceGetter, ILocationGetter>
    {
        public override string PropertyName => "Location";

        protected override IFormLinkNullableGetter<ILocationGetter>? GetFormLinkValue(IWorldspaceGetter record)
        {
            return record.Location;
        }

        protected override void SetFormLinkValue(IWorldspace record, IFormLinkNullableGetter<ILocationGetter>? value)
        {
            if (value != null)
            {
                record.Location.SetTo(value.FormKey);
            }
            else
            {
                record.Location.SetTo(null);
            }
        }
    }
}

