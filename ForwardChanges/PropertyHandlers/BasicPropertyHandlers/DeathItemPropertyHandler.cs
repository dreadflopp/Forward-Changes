using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class DeathItemPropertyHandler : AbstractPropertyHandler<IFormLinkGetter<ILeveledItemGetter>>, IPropertyHandler<object>
    {
        public override string PropertyName => "DeathItem";

        public override void SetValue(IMajorRecord record, IFormLinkGetter<ILeveledItemGetter>? value)
        {
            if (record is INpc npc)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    npc.DeathItem = new FormLinkNullable<ILeveledItemGetter>(value.FormKey);
                }
                else
                {
                    npc.DeathItem.Clear();
                }
            }
        }

        public override IFormLinkGetter<ILeveledItemGetter>? GetValue(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (context.Record is INpcGetter npc)
            {
                return npc.DeathItem;
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkGetter<ILeveledItemGetter>? value1, IFormLinkGetter<ILeveledItemGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }

        // IPropertyHandler<object> implementation
        void IPropertyHandler<object>.SetValue(IMajorRecord record, object? value) => SetValue(record, (IFormLinkGetter<ILeveledItemGetter>?)value);
        object? IPropertyHandler<object>.GetValue(IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context) => GetValue(context);
        bool IPropertyHandler<object>.AreValuesEqual(object? value1, object? value2) => AreValuesEqual((IFormLinkGetter<ILeveledItemGetter>?)value1, (IFormLinkGetter<ILeveledItemGetter>?)value2);
    }
}