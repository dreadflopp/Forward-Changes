using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Faction
{
    public class RanksHandler : AbstractListPropertyHandler<IRankGetter>
    {
        public override string PropertyName => "Ranks";

        public override List<IRankGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IFactionGetter factionRecord)
            {
                return factionRecord.Ranks?.ToList();
            }

            Console.WriteLine($"Error: Record does not implement IFactionGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, List<IRankGetter>? value)
        {
            if (record is IFaction factionRecord)
            {
                if (factionRecord.Ranks != null)
                {
                    factionRecord.Ranks.Clear();
                    if (value != null)
                    {
                        foreach (var item in value)
                        {
                            if (item is Rank castItem)
                            {
                                factionRecord.Ranks.Add(castItem);
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IFaction for {PropertyName}");
            }
        }
    }
}

