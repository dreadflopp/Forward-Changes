using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ListPropertyHandlers
{
    public class FactionRelationsListPropertyHandler : AbstractListPropertyHandler<IRelationGetter>
    {
        public override string PropertyName => "Relations";

        public override List<IRelationGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IFactionGetter factionRecord)
            {
                return factionRecord.Relations?.ToList();
            }

            Console.WriteLine($"Error: Record does not implement IFactionGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, List<IRelationGetter>? value)
        {
            if (record is IFaction factionRecord)
            {
                if (factionRecord.Relations != null)
                {
                    factionRecord.Relations.Clear();
                    if (value != null)
                    {
                        foreach (var item in value)
                        {
                            if (item is Relation castItem)
                            {
                                factionRecord.Relations.Add(castItem);
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