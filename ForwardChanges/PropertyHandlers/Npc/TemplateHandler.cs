using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class TemplateHandler : AbstractFormLinkPropertyHandler<INpc, INpcGetter, INpcSpawnGetter>
    {
        public override string PropertyName => "Template";

        protected override IFormLinkNullableGetter<INpcSpawnGetter>? GetFormLinkValue(INpcGetter record)
        {
            return record.Template;
        }

        protected override void SetFormLinkValue(INpc record, IFormLinkNullableGetter<INpcSpawnGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.Template = new FormLinkNullable<INpcSpawnGetter>(value.FormKey);
            }
            else
            {
                record.Template.Clear();
            }
        }
    }
}