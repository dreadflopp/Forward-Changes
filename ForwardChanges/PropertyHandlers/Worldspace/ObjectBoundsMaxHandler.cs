using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class ObjectBoundsMaxHandler : AbstractPropertyHandler<P2Float>
    {
        public override string PropertyName => "ObjectBoundsMax";

        public override void SetValue(IMajorRecord record, P2Float value)
        {
            var worldspaceRecord = TryCastRecord<IWorldspace>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                worldspaceRecord.ObjectBoundsMax = value;
            }
        }

        public override P2Float GetValue(IMajorRecordGetter record)
        {
            var worldspaceRecord = TryCastRecord<IWorldspaceGetter>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                return worldspaceRecord.ObjectBoundsMax;
            }
            return default;
        }

        public override bool AreValuesEqual(P2Float value1, P2Float value2)
        {
            // Use P2Float's built-in equality
            return value1.Equals(value2);
        }
    }
}

