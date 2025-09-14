using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class TintLayersHandler : AbstractPropertyHandler<IReadOnlyList<ITintLayerGetter>>
    {
        public override string PropertyName => "TintLayers";

        public override IReadOnlyList<ITintLayerGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npcRecord)
            {
                return npcRecord.TintLayers;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpcGetter for {PropertyName}");
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, IReadOnlyList<ITintLayerGetter>? value)
        {
            if (record is INpc npcRecord)
            {
                if (value == null)
                {
                    npcRecord.TintLayers.Clear();
                    return;
                }

                // Clear existing tint layers and add new ones
                npcRecord.TintLayers.Clear();
                foreach (var tintLayer in value)
                {
                    if (tintLayer == null) continue;

                    // Create a new TintLayer instance and copy all properties
                    var newTintLayer = new TintLayer
                    {
                        Index = tintLayer.Index,
                        Color = tintLayer.Color,
                        InterpolationValue = tintLayer.InterpolationValue,
                        Preset = tintLayer.Preset
                    };

                    npcRecord.TintLayers.Add(newTintLayer);
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpc for {PropertyName}");
            }
        }

        public override bool AreValuesEqual(IReadOnlyList<ITintLayerGetter>? value1, IReadOnlyList<ITintLayerGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            if (value1.Count != value2.Count) return false;

            // Compare each tint layer
            for (int i = 0; i < value1.Count; i++)
            {
                var layer1 = value1[i];
                var layer2 = value2[i];

                if (layer1 == null && layer2 == null) continue;
                if (layer1 == null || layer2 == null) return false;

                // Compare all properties
                if (layer1.Index != layer2.Index) return false;
                if (layer1.Color?.ToArgb() != layer2.Color?.ToArgb()) return false;
                if (Math.Abs((layer1.InterpolationValue ?? 0f) - (layer2.InterpolationValue ?? 0f)) >= 0.001f) return false;
                if (layer1.Preset != layer2.Preset) return false;
            }

            return true;
        }
    }
} 