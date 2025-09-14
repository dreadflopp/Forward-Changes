using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class MenuDisplayObjectHandler : AbstractPropertyHandler<IFormLinkGetter<IStaticGetter>>
    {
        public override string PropertyName => "MenuDisplayObject";

        public override void SetValue(IMajorRecord record, IFormLinkGetter<IStaticGetter>? value)
        {
            if (record is IMagicEffect magicEffect)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    magicEffect.MenuDisplayObject = new FormLinkNullable<IStaticGetter>(value.FormKey);
                }
                else
                {
                    magicEffect.MenuDisplayObject.Clear();
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffect for {PropertyName}");
            }
        }

        public override IFormLinkGetter<IStaticGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IMagicEffectGetter magicEffect)
            {
                return magicEffect.MenuDisplayObject;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
            }
            return null;
        }
    }
}
