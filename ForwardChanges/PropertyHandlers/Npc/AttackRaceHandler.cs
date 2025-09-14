using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class AttackRaceHandler : AbstractFormLinkPropertyHandler<INpc, INpcGetter, IRaceGetter>
    {
        public override string PropertyName => "AttackRace";

        protected override IFormLinkNullableGetter<IRaceGetter>? GetFormLinkValue(INpcGetter record)
        {
            return record.AttackRace;
        }

        protected override void SetFormLinkValue(INpc record, IFormLinkNullableGetter<IRaceGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.AttackRace = new FormLinkNullable<IRaceGetter>(value.FormKey);
            }
            else
            {
                record.AttackRace.Clear();
            }
        }
    }
}