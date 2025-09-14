using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class MusicHandler : AbstractFormLinkPropertyHandler<ICell, ICellGetter, IMusicTypeGetter>
    {
        public override string PropertyName => "Music";

        protected override IFormLinkNullableGetter<IMusicTypeGetter>? GetFormLinkValue(ICellGetter record)
        {
            return record.Music;
        }

        protected override void SetFormLinkValue(ICell record, IFormLinkNullableGetter<IMusicTypeGetter>? value)
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

