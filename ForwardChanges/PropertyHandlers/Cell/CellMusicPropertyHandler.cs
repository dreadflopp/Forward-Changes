using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class CellMusicPropertyHandler : AbstractPropertyHandler<IFormLinkNullableGetter<IMusicTypeGetter>>
    {
        public override string PropertyName => "Music";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<IMusicTypeGetter>? value)
        {
            if (record is ICell cell)
            {
                if (value != null)
                {
                    cell.Music.SetTo(value.FormKey);
                }
                else
                {
                    cell.Music.SetTo(null);
                }
            }
        }

        public override IFormLinkNullableGetter<IMusicTypeGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ICellGetter cell)
            {
                return cell.Music;
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<IMusicTypeGetter>? value1, IFormLinkNullableGetter<IMusicTypeGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}