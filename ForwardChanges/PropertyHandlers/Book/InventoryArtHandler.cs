using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Book
{
    public class InventoryArtHandler : AbstractFormLinkPropertyHandler<IBook, IBookGetter, IStaticGetter>
    {
        public override string PropertyName => "InventoryArt";

        protected override IFormLinkNullableGetter<IStaticGetter>? GetFormLinkValue(IBookGetter record)
        {
            return record.InventoryArt;
        }

        protected override void SetFormLinkValue(IBook record, IFormLinkNullableGetter<IStaticGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.InventoryArt = new FormLinkNullable<IStaticGetter>(value.FormKey);
            }
            else
            {
                record.InventoryArt.Clear();
            }
        }
    }
}