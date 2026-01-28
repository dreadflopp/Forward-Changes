using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.LeveledItem
{
    public class GlobalHandler : AbstractFormLinkPropertyHandler<ILeveledItem, ILeveledItemGetter, IGlobalGetter>
    {
        public override string PropertyName => "Global";

        protected override IFormLinkNullableGetter<IGlobalGetter>? GetFormLinkValue(ILeveledItemGetter record)
        {
            return record.Global;
        }

        protected override void SetFormLinkValue(ILeveledItem record, IFormLinkNullableGetter<IGlobalGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.Global = new FormLinkNullable<IGlobalGetter>(value.FormKey);
            }
            else
            {
                record.Global.Clear();
            }
        }
    }
}
