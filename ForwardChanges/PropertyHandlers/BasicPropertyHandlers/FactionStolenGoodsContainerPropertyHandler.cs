using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class FactionStolenGoodsContainerPropertyHandler : AbstractPropertyHandler<IFormLinkNullable<IPlacedObjectGetter>>
    {
        public override string PropertyName => "StolenGoodsContainer";

        public override IFormLinkNullable<IPlacedObjectGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IFactionGetter factionRecord)
            {
                return factionRecord.StolenGoodsContainer as IFormLinkNullable<IPlacedObjectGetter>;
            }

            Console.WriteLine($"Error: Record does not implement IFactionGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, IFormLinkNullable<IPlacedObjectGetter>? value)
        {
            if (record is IFaction factionRecord)
            {
                factionRecord.StolenGoodsContainer = value ?? new FormLinkNullable<IPlacedObjectGetter>();
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IFaction for {PropertyName}");
            }
        }
    }
}