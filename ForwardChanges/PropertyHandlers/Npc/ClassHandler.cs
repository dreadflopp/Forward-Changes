using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Mutagen.Bethesda.Plugins;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class ClassHandler : AbstractFormLinkPropertyHandler<INpc, INpcGetter, IClassGetter>
    {
        public override string PropertyName => "Class";

        protected override IFormLinkNullableGetter<IClassGetter>? GetFormLinkValue(INpcGetter record)
        {
            return record.Class as IFormLinkNullableGetter<IClassGetter>;
        }

        protected override void SetFormLinkValue(INpc record, IFormLinkNullableGetter<IClassGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.Class = new FormLinkNullable<IClassGetter>(value.FormKey);
            }
            else
            {
                record.Class.Clear();
            }
        }
    }
}

