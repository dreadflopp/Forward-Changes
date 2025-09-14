using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class DistantLodMultiplierHandler : AbstractPropertyHandler<float?>
    {
        public override string PropertyName => "DistantLodMultiplier";

        public override void SetValue(IMajorRecord record, float? value)
        {
            var worldspaceRecord = TryCastRecord<IWorldspace>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                worldspaceRecord.DistantLodMultiplier = value;
            }
        }

        public override float? GetValue(IMajorRecordGetter record)
        {
            var worldspaceRecord = TryCastRecord<IWorldspaceGetter>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                return worldspaceRecord.DistantLodMultiplier;
            }
            return null;
        }
    }
}