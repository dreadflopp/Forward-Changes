using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Spell
{
    public class MenuDisplayObjectHandler : AbstractPropertyHandler<IFormLinkNullableGetter<IStaticGetter>>
    {
        public override string PropertyName => "MenuDisplayObject";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<IStaticGetter>? value)
        {
            if (record is ISpell spell)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    spell.MenuDisplayObject = new FormLinkNullable<IStaticGetter>(value.FormKey);
                }
                else
                {
                    spell.MenuDisplayObject.Clear();
                }
            }
            else
            {
                Console.WriteLine($"Error: Record is not a Spell for {PropertyName}");
            }
        }

        public override IFormLinkNullableGetter<IStaticGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ISpellGetter spell)
            {
                return spell.MenuDisplayObject;
            }
            else
            {
                Console.WriteLine($"Error: Record is not a Spell for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<IStaticGetter>? value1, IFormLinkNullableGetter<IStaticGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}

