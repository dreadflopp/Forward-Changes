using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class CrimeFactionHandler : AbstractFormLinkPropertyHandler<INpc, INpcGetter, IFactionGetter>
    {
        public override string PropertyName => "CrimeFaction";

        protected override IFormLinkNullableGetter<IFactionGetter>? GetFormLinkValue(INpcGetter record)
        {
            return record.CrimeFaction;
        }

        protected override void SetFormLinkValue(INpc record, IFormLinkNullableGetter<IFactionGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.CrimeFaction = new FormLinkNullable<IFactionGetter>(value.FormKey);
            }
            else
            {
                record.CrimeFaction.Clear();
            }
        }
    }
}