using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Faction
{
    public class ExteriorJailMarkerHandler : AbstractPropertyHandler<IFormLinkNullable<IPlacedObjectGetter>>
    {
        public override string PropertyName => "ExteriorJailMarker";

        public override IFormLinkNullable<IPlacedObjectGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IFactionGetter factionRecord)
            {
                return factionRecord.ExteriorJailMarker as IFormLinkNullable<IPlacedObjectGetter>;
            }

            Console.WriteLine($"Error: Record does not implement IFactionGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, IFormLinkNullable<IPlacedObjectGetter>? value)
        {
            if (record is IFaction factionRecord)
            {
                factionRecord.ExteriorJailMarker = value ?? new FormLinkNullable<IPlacedObjectGetter>();
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IFaction for {PropertyName}");
            }
        }
    }
}

