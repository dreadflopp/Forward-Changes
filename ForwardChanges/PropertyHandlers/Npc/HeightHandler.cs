using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class HeightHandler : AbstractPropertyHandler<float>
    {
        public override string PropertyName => "Height";

        public override float GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npcRecord)
            {
                return npcRecord.Height;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpcGetter for {PropertyName}");
            }
            return 0f;
        }

        public override void SetValue(IMajorRecord record, float value)
        {
            if (record is INpc npcRecord)
            {
                npcRecord.Height = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpc for {PropertyName}");
            }
        }

        public override bool AreValuesEqual(float value1, float value2)
        {
            return Math.Abs(value1 - value2) < 0.001f;
        }
    }
} 