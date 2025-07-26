using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class WorldspaceMapDataPropertyHandler : AbstractPropertyHandler<WorldspaceMap?>
    {
        public override string PropertyName => "MapData";

        public override void SetValue(IMajorRecord record, WorldspaceMap? value)
        {
            if (record is IWorldspace worldspaceRecord)
            {
                if (value == null)
                {
                    worldspaceRecord.MapData = null;
                }
                else
                {
                    // Deep copy
                    var newMapData = new WorldspaceMap();
                    newMapData.DeepCopyIn(value);
                    worldspaceRecord.MapData = newMapData;
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IWorldspace for {PropertyName}");
            }
        }

        public override WorldspaceMap? GetValue(IMajorRecordGetter record)
        {
            if (record is IWorldspaceGetter worldspaceRecord)
            {
                return worldspaceRecord.MapData as WorldspaceMap;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IWorldspaceGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(WorldspaceMap? value1, WorldspaceMap? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            // Use Mutagen's built-in equality
            return value1.Equals(value2);
        }
    }
}