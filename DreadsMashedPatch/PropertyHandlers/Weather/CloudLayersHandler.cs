using System;
using System.Collections.Generic;
using System.Linq;
using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace ForwardChanges.PropertyHandlers.Weather
{
    public class CloudLayersHandler : AbstractPropertyHandler<IReadOnlyList<ICloudLayerGetter>>
    {
        public override string PropertyName => "Clouds";

        public override IReadOnlyList<ICloudLayerGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IWeatherGetter weatherRecord)
            {
                return weatherRecord.Clouds;
            }

            Console.WriteLine($"Error: Record does not implement IWeatherGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, IReadOnlyList<ICloudLayerGetter>? value)
        {
            if (record is not IWeather weatherRecord)
            {
                Console.WriteLine($"Error: Record does not implement IWeather for {PropertyName}");
                return;
            }

            var target = weatherRecord.Clouds;
            for (var i = 0; i < target.Length; i++)
            {
                if (value != null && i < value.Count && value[i] != null)
                {
                    target[i] = value[i].DeepCopy();
                }
                else
                {
                    target[i] = new CloudLayer();
                }
            }
        }

        public override bool AreValuesEqual(IReadOnlyList<ICloudLayerGetter>? value1, IReadOnlyList<ICloudLayerGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1.Count != value2.Count) return false;

            for (var i = 0; i < value1.Count; i++)
            {
                var v1 = value1[i];
                var v2 = value2[i];

                if (v1 == null && v2 == null)
                {
                    continue;
                }

                if (v1 == null || v2 == null)
                {
                    return false;
                }

                if (!v1.Equals(v2))
                {
                    return false;
                }
            }

            return true;
        }

        public override string FormatValue(object? value)
        {
            if (value is IReadOnlyList<ICloudLayerGetter> list)
            {
                return $"Count={list.Count}";
            }

            return value?.ToString() ?? "null";
        }
    }
}
