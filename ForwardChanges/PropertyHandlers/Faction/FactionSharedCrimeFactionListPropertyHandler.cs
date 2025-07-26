using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Faction
{
    public class FactionSharedCrimeFactionListPropertyHandler : AbstractPropertyHandler<IFormLinkNullable<IFormListGetter>>
    {
        public override string PropertyName => "SharedCrimeFactionList";

        public override IFormLinkNullable<IFormListGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IFactionGetter factionRecord)
            {
                return factionRecord.SharedCrimeFactionList as IFormLinkNullable<IFormListGetter>;
            }

            Console.WriteLine($"Error: Record does not implement IFactionGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, IFormLinkNullable<IFormListGetter>? value)
        {
            if (record is IFaction factionRecord)
            {
                factionRecord.SharedCrimeFactionList = value ?? new FormLinkNullable<IFormListGetter>();
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IFaction for {PropertyName}");
            }
        }
    }
}