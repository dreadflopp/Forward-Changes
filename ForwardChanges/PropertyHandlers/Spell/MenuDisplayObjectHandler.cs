using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Spell
{
    public class MenuDisplayObjectHandler : AbstractFormLinkPropertyHandler<ISpell, ISpellGetter, IStaticGetter>
    {
        public override string PropertyName => "MenuDisplayObject";

        protected override IFormLinkNullableGetter<IStaticGetter>? GetFormLinkValue(ISpellGetter record)
        {
            return record.MenuDisplayObject;
        }

        protected override void SetFormLinkValue(ISpell record, IFormLinkNullableGetter<IStaticGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.MenuDisplayObject = new FormLinkNullable<IStaticGetter>(value.FormKey);
            }
            else
            {
                record.MenuDisplayObject.Clear();
            }
        }
    }
}

