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
                LogCollector.Add(PropertyName, $"Error: Record does not implement INpcGetter for {PropertyName}");
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
                LogCollector.Add(PropertyName, $"Error: Record does not implement INpc for {PropertyName}");
            }
        }

        public override bool AreValuesEqual(IReadOnlyList<IFormLinkGetter<IHeadPartGetter>>? value1, IReadOnlyList<IFormLinkGetter<IHeadPartGetter>>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            if (value1.Count != value2.Count) return false;

            // Order doesn't matter for HeadParts - compare as sets of FormKeys
            var formKeys1 = value1
                .Where(hp => hp != null && !hp.FormKey.IsNull)
                .Select(hp => hp.FormKey)
                .OrderBy(fk => fk.ToString())
                .ToList();

            var formKeys2 = value2
                .Where(hp => hp != null && !hp.FormKey.IsNull)
                .Select(hp => hp.FormKey)
                .OrderBy(fk => fk.ToString())
                .ToList();

            if (formKeys1.Count != formKeys2.Count) return false;

            // Compare sorted FormKeys
            for (int i = 0; i < formKeys1.Count; i++)
            {
                if (formKeys1[i] != formKeys2[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override string FormatValue(object? value)
        {
            if (value is IReadOnlyList<IFormLinkGetter<IHeadPartGetter>> list)
            {
                if (list == null || list.Count == 0)
                    return "Empty";
                return string.Join(", ", list.Select(hp => hp?.FormKey.ToString() ?? "null"));
            }
            return value?.ToString() ?? "null";
        }
    }
}