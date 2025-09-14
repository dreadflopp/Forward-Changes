using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class FixedDimensionsCenterCellHandler : AbstractPropertyHandler<P2Int16?>
    {
        public override string PropertyName => "FixedDimensionsCenterCell";

        public override void SetValue(IMajorRecord record, P2Int16? value)
        {
            var worldspaceRecord = TryCastRecord<IWorldspace>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                worldspaceRecord.FixedDimensionsCenterCell = value;
            }
        }

        public override P2Int16? GetValue(IMajorRecordGetter record)
        {
            var worldspaceRecord = TryCastRecord<IWorldspaceGetter>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                return worldspaceRecord.FixedDimensionsCenterCell;
            }
            return null;
        }

        public override bool AreValuesEqual(P2Int16? value1, P2Int16? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.Value.Equals(value2.Value);
        }
    }
}