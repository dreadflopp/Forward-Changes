using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class LandDefaultsHandler : AbstractPropertyHandler<IWorldspaceLandDefaultsGetter?>
    {
        public override string PropertyName => "LandDefaults";

        public override void SetValue(IMajorRecord record, IWorldspaceLandDefaultsGetter? value)
        {
            var worldspaceRecord = TryCastRecord<IWorldspace>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                if (value == null)
                {
                    worldspaceRecord.LandDefaults = null;
                }
                else
                {
                    // Deep copy
                    var newLandDefaults = new WorldspaceLandDefaults();
                    newLandDefaults.DeepCopyIn(value);
                    worldspaceRecord.LandDefaults = newLandDefaults;
                }
            }
        }

        public override IWorldspaceLandDefaultsGetter? GetValue(IMajorRecordGetter record)
        {
            var worldspaceRecord = TryCastRecord<IWorldspaceGetter>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                return worldspaceRecord.LandDefaults;
            }
            return null;
        }

        public override bool AreValuesEqual(IWorldspaceLandDefaultsGetter? value1, IWorldspaceLandDefaultsGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            // Use Mutagen's built-in equality
            return value1.Equals(value2);
        }

        public override string FormatValue(object? value)
        {
            if (value is not IWorldspaceLandDefaultsGetter landDefaults)
            {
                return value?.ToString() ?? "null";
            }

            return $"DefaultLandHeight: {landDefaults.DefaultLandHeight}, DefaultWaterHeight: {landDefaults.DefaultWaterHeight}";
        }
    }
}