using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class CombatStyleHandler : AbstractFormLinkPropertyHandler<INpc, INpcGetter, ICombatStyleGetter>
    {
        public override string PropertyName => "CombatStyle";

        protected override IFormLinkNullableGetter<ICombatStyleGetter>? GetFormLinkValue(INpcGetter record)
        {
            return record.CombatStyle;
        }

        protected override void SetFormLinkValue(INpc record, IFormLinkNullableGetter<ICombatStyleGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.CombatStyle = new FormLinkNullable<ICombatStyleGetter>(value.FormKey);
            }
            else
            {
                record.CombatStyle.Clear();
            }
        }
    }
}