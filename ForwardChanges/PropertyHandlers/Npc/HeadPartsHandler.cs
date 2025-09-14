using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class HeadPartsHandler : AbstractPropertyHandler<IReadOnlyList<IFormLinkGetter<IHeadPartGetter>>>
    {
        public override string PropertyName => "HeadParts";

        public override IReadOnlyList<IFormLinkGetter<IHeadPartGetter>>? GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npcRecord)
            {
                return npcRecord.HeadParts;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpcGetter for {PropertyName}");
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, IReadOnlyList<IFormLinkGetter<IHeadPartGetter>>? value)
        {
            if (record is INpc npcRecord)
            {
                if (value == null)
                {
                    npcRecord.HeadParts.Clear();
                    return;
                }

                // Clear existing head parts and add new ones
                npcRecord.HeadParts.Clear();
                foreach (var headPart in value)
                {
                    if (headPart != null && !headPart.FormKey.IsNull)
                    {
                        npcRecord.HeadParts.Add(new FormLink<IHeadPartGetter>(headPart.FormKey));
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpc for {PropertyName}");
            }
        }

        public override bool AreValuesEqual(IReadOnlyList<IFormLinkGetter<IHeadPartGetter>>? value1, IReadOnlyList<IFormLinkGetter<IHeadPartGetter>>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            if (value1.Count != value2.Count) return false;

            // Compare each head part by FormKey
            for (int i = 0; i < value1.Count; i++)
            {
                var headPart1 = value1[i];
                var headPart2 = value2[i];

                if (headPart1 == null && headPart2 == null) continue;
                if (headPart1 == null || headPart2 == null) return false;

                if (!headPart1.FormKey.Equals(headPart2.FormKey))
                {
                    return false;
                }
            }

            return true;
        }
    }
}