using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Mutagen.Bethesda.Plugins;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class ClassPropertyHandler : AbstractPropertyHandler<IFormLinkGetter<IClassGetter>>
    {
        public override string PropertyName => "Class";

        public override void SetValue(IMajorRecord record, IFormLinkGetter<IClassGetter>? value)
        {
            if (record is INpc npc)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    npc.Class = new FormLinkNullable<IClassGetter>(value.FormKey);
                }
                else
                {
                    npc.Class.Clear();
                }
            }
        }

        public override IFormLinkGetter<IClassGetter>? GetValue(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (context.Record is INpcGetter npc)
            {
                return npc.Class;
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkGetter<IClassGetter>? value1, IFormLinkGetter<IClassGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}