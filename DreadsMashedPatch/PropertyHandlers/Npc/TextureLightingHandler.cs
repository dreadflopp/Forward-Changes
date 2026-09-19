using System;
using System.Drawing;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class TextureLightingHandler : AbstractPropertyHandler<Color?>
    {
        public override string PropertyName => "TextureLighting";

        public override Color? GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npcRecord)
            {
                return npcRecord.TextureLighting;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpcGetter for {PropertyName}");
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, Color? value)
        {
            if (record is INpc npcRecord)
            {
                npcRecord.TextureLighting = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpc for {PropertyName}");
            }
        }

        public override bool AreValuesEqual(Color? value1, Color? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare Color properties
            return value1.Value.ToArgb() == value2.Value.ToArgb();
        }
    }
}