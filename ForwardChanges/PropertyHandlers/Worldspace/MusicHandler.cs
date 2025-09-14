using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class MusicHandler : AbstractFormLinkPropertyHandler<IWorldspace, IWorldspaceGetter, IMusicTypeGetter>
    {
        public override string PropertyName => "Music";

        protected override IFormLinkNullableGetter<IMusicTypeGetter>? GetFormLinkValue(IWorldspaceGetter record)
        {
            return record.Music;
        }

        protected override void SetFormLinkValue(IWorldspace record, IFormLinkNullableGetter<IMusicTypeGetter>? value)
        {
            if (value != null)
            {
                record.Music.SetTo(value.FormKey);
            }
            else
            {
                record.Music.SetTo(null);
            }
        }
    }
}

