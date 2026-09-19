using System;
using System.Collections.Generic;
using System.Linq;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;

namespace DreadsMashedPatch.PropertyHandlers.Weather
{
    public class CloudTexturesHandler : AbstractPropertyHandler<IReadOnlyList<IAssetLinkGetter<SkyrimTextureAssetType>?>>
    {
        public override string PropertyName => "CloudTextures";

        public override IReadOnlyList<IAssetLinkGetter<SkyrimTextureAssetType>?>? GetValue(IMajorRecordGetter record)
        {
            if (record is IWeatherGetter weatherRecord)
            {
                return weatherRecord.CloudTextures;
            }

            Console.WriteLine($"Error: Record does not implement IWeatherGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, IReadOnlyList<IAssetLinkGetter<SkyrimTextureAssetType>?>? value)
        {
            if (record is not IWeather weatherRecord)
            {
                Console.WriteLine($"Error: Record does not implement IWeather for {PropertyName}");
                return;
            }

            var target = weatherRecord.CloudTextures;
            for (var i = 0; i < target.Length; i++)
            {
                if (value != null && i < value.Count && value[i] != null)
                {
                    target[i] = AssetPathHelper.Copy(value[i]);
                }
                else
                {
                    target[i] = null;
                }
            }
        }

        public override bool AreValuesEqual(IReadOnlyList<IAssetLinkGetter<SkyrimTextureAssetType>?>? value1, IReadOnlyList<IAssetLinkGetter<SkyrimTextureAssetType>?>? value2)
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

                if (!AssetPathHelper.AreEqual(v1, v2))
                {
                    return false;
                }
            }

            return true;
        }

        public override string FormatValue(object? value)
        {
            if (value is IReadOnlyList<IAssetLinkGetter<SkyrimTextureAssetType>?> list)
            {
                if (list.Count == 0)
                {
                    return "Empty";
                }

                return string.Join(", ", list.Select((item, idx) => $"{idx}={AssetPathHelper.Format(item)}"));
            }

            return value?.ToString() ?? "null";
        }
    }
}
