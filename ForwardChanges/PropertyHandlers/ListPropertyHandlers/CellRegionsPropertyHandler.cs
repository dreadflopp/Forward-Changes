using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers.Abstracts;
using ForwardChanges.Contexts;
using Noggog;

namespace ForwardChanges.PropertyHandlers.ListPropertyHandlers
{
    public class CellRegionsPropertyHandler : AbstractListPropertyHandler<IFormLinkGetter<IRegionGetter>>
    {
        public override string PropertyName => "Regions";

        public override void SetValue(IMajorRecord record, List<IFormLinkGetter<IRegionGetter>>? value)
        {
            if (record is ICell cell)
            {
                if (value != null)
                {
                    cell.Regions = new ExtendedList<IFormLinkGetter<IRegionGetter>>(value);
                }
                else
                {
                    cell.Regions = null;
                }
            }
        }

        public override List<IFormLinkGetter<IRegionGetter>>? GetValue(IMajorRecordGetter record)
        {
            if (record is ICellGetter cell)
            {
                return cell.Regions?.ToList();
            }
            return null;
        }

        protected override bool IsItemEqual(IFormLinkGetter<IRegionGetter>? item1, IFormLinkGetter<IRegionGetter>? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;
            return item1.FormKey.Equals(item2.FormKey);
        }

        protected override string FormatItem(IFormLinkGetter<IRegionGetter>? item)
        {
            return item?.FormKey.ToString() ?? "null";
        }
    }
}