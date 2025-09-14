using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Location
{
    public class UnreportedCrimeFactionHandler : AbstractFormLinkPropertyHandler<ILocation, ILocationGetter, IFactionGetter>
    {
        public override string PropertyName => "UnreportedCrimeFaction";

        protected override IFormLinkNullableGetter<IFactionGetter>? GetFormLinkValue(ILocationGetter record)
        {
            return record.UnreportedCrimeFaction as IFormLinkNullableGetter<IFactionGetter>;
        }

        protected override void SetFormLinkValue(ILocation record, IFormLinkNullableGetter<IFactionGetter>? value)
        {
            record.UnreportedCrimeFaction = value != null ? new FormLinkNullable<IFactionGetter>(value.FormKey) : new FormLinkNullable<IFactionGetter>();
        }
    }
}

