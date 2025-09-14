using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class LockListHandler : AbstractFormLinkPropertyHandler<ICell, ICellGetter, ILockListGetter>
    {
        public override string PropertyName => "LockList";

        protected override IFormLinkNullableGetter<ILockListGetter>? GetFormLinkValue(ICellGetter record)
        {
            return record.LockList;
        }

        protected override void SetFormLinkValue(ICell record, IFormLinkNullableGetter<ILockListGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.LockList = new FormLinkNullable<ILockListGetter>(value.FormKey);
            }
            else
            {
                record.LockList.Clear();
            }
        }
    }
}