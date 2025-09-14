using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class WorldMapCellOffsetHandler : AbstractPropertyHandler<P3Float>
    {
        public override string PropertyName => "WorldMapCellOffset";

        public override void SetValue(IMajorRecord record, P3Float value)
        {
            var worldspaceRecord = TryCastRecord<IWorldspace>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                worldspaceRecord.WorldMapCellOffset = value;
            }
        }

        public override P3Float GetValue(IMajorRecordGetter record)
        {
            var worldspaceRecord = TryCastRecord<IWorldspaceGetter>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                return worldspaceRecord.WorldMapCellOffset;
            }
            return default;
        }

        public override bool AreValuesEqual(P3Float value1, P3Float value2)
        {
            // Use P3Float's built-in equality
            return value1.Equals(value2);
        }
    }
}