using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Faction
{
    public class FactionVendorBuySellListPropertyHandler : AbstractPropertyHandler<IFormLinkNullable<IFormListGetter>>
    {
        public override string PropertyName => "VendorBuySellList";

        public override IFormLinkNullable<IFormListGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IFactionGetter factionRecord)
            {
                return factionRecord.VendorBuySellList as IFormLinkNullable<IFormListGetter>;
            }

            Console.WriteLine($"Error: Record does not implement IFactionGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, IFormLinkNullable<IFormListGetter>? value)
        {
            if (record is IFaction factionRecord)
            {
                factionRecord.VendorBuySellList = value ?? new FormLinkNullable<IFormListGetter>();
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IFaction for {PropertyName}");
            }
        }
    }
}