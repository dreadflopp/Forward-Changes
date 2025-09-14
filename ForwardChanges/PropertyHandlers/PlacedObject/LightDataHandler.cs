using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class LightDataHandler : AbstractPropertyHandler<ILightDataGetter?>
    {
        public override string PropertyName => "LightData";

        public override void SetValue(IMajorRecord record, ILightDataGetter? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                if (value != null)
                {
                    // Create new LightData and copy properties
                    var newLightData = new LightData
                    {
                        Versioning = value.Versioning,
                        FovOffset = value.FovOffset,
                        FadeOffset = value.FadeOffset,
                        EndDistanceCap = value.EndDistanceCap,
                        ShadowDepthBias = value.ShadowDepthBias,
                        Unknown = value.Unknown
                    };
                    placedObjectRecord.LightData = newLightData;
                }
                else
                {
                    placedObjectRecord.LightData = null;
                }
            }
        }

        public override ILightDataGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.LightData;
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return null;
        }

        public override bool AreValuesEqual(ILightDataGetter? value1, ILightDataGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare all properties
            if (value1.Versioning != value2.Versioning) return false;
            if (value1.FovOffset != value2.FovOffset) return false;
            if (value1.FadeOffset != value2.FadeOffset) return false;
            if (value1.EndDistanceCap != value2.EndDistanceCap) return false;
            if (value1.ShadowDepthBias != value2.ShadowDepthBias) return false;
            if (value1.Unknown != value2.Unknown) return false;

            return true;
        }

        public override string FormatValue(object? value)
        {
            if (value is not ILightDataGetter lightData)
            {
                return value?.ToString() ?? "null";
            }

            return $"FovOffset: {lightData.FovOffset}, FadeOffset: {lightData.FadeOffset}, EndDistanceCap: {lightData.EndDistanceCap}, ShadowDepthBias: {lightData.ShadowDepthBias}";
        }
    }
}