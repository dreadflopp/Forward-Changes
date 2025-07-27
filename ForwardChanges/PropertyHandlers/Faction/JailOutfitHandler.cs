using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Faction
{
    public class JailOutfitHandler : AbstractPropertyHandler<IFormLinkNullable<IOutfitGetter>>
    {
        public override string PropertyName => "JailOutfit";

        public override IFormLinkNullable<IOutfitGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IFactionGetter factionRecord)
            {
                return factionRecord.JailOutfit as IFormLinkNullable<IOutfitGetter>;
            }

            Console.WriteLine($"Error: Record does not implement IFactionGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, IFormLinkNullable<IOutfitGetter>? value)
        {
            if (record is IFaction factionRecord)
            {
                factionRecord.JailOutfit = value ?? new FormLinkNullable<IOutfitGetter>();
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IFaction for {PropertyName}");
            }
        }
    }
}

