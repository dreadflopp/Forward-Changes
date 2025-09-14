using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Location
{
    public class MusicHandler : AbstractFormLinkPropertyHandler<ILocation, ILocationGetter, IMusicTypeGetter>
    {
        public override string PropertyName => "Music";

        protected override IFormLinkNullableGetter<IMusicTypeGetter>? GetFormLinkValue(ILocationGetter record)
        {
            return record.Music as IFormLinkNullableGetter<IMusicTypeGetter>;
        }

        protected override void SetFormLinkValue(ILocation record, IFormLinkNullableGetter<IMusicTypeGetter>? value)
        {
            record.Music = value != null ? new FormLinkNullable<IMusicTypeGetter>(value.FormKey) : new FormLinkNullable<IMusicTypeGetter>();
        }
    }
}

