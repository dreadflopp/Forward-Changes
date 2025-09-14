using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class MapDataHandler : AbstractPropertyHandler<IWorldspaceMapGetter?>
    {
        public override string PropertyName => "MapData";

        public override void SetValue(IMajorRecord record, IWorldspaceMapGetter? value)
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

        public override IWorldspaceMapGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IWorldspaceGetter worldspaceRecord)
            {
                return worldspaceRecord.MapData;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IWorldspaceGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IWorldspaceMapGetter? value1, IWorldspaceMapGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare all properties using value-based comparison
            return value1.Versioning == value2.Versioning &&
                   value1.UsableDimensions.Equals(value2.UsableDimensions) &&
                   value1.NorthwestCellCoords.Equals(value2.NorthwestCellCoords) &&
                   value1.SoutheastCellCoords.Equals(value2.SoutheastCellCoords) &&
                   value1.CameraMinHeight == value2.CameraMinHeight &&
                   value1.CameraMaxHeight == value2.CameraMaxHeight &&
                   value1.CameraInitialPitch == value2.CameraInitialPitch;
        }

        public override string FormatValue(object? value)
        {
            if (value is not IWorldspaceMapGetter mapData)
            {
                return value?.ToString() ?? "null";
            }

            return $"Versioning: {mapData.Versioning}, " +
                   $"UsableDimensions: {mapData.UsableDimensions}, " +
                   $"NorthwestCellCoords: {mapData.NorthwestCellCoords}, " +
                   $"SoutheastCellCoords: {mapData.SoutheastCellCoords}, " +
                   $"CameraMinHeight: {mapData.CameraMinHeight}, " +
                   $"CameraMaxHeight: {mapData.CameraMaxHeight}, " +
                   $"CameraInitialPitch: {mapData.CameraInitialPitch}";
        }
    }
}

