using System;
using System.Collections.Generic;
using System.Linq;
using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace ForwardChanges.PropertyHandlers.Race
{
    public class RaceBipedObjectNamesHandler : AbstractPropertyHandler<IReadOnlyDictionary<BipedObject, string>>
    {
        public override string PropertyName => "BipedObjectNames";

        public override IReadOnlyDictionary<BipedObject, string>? GetValue(IMajorRecordGetter record)
        {
            if (record is IRaceGetter raceRecord)
            {
                return raceRecord.BipedObjectNames;
            }

            Console.WriteLine($"Error: Record does not implement IRaceGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, IReadOnlyDictionary<BipedObject, string>? value)
        {
            if (record is not IRace raceRecord)
            {
                Console.WriteLine($"Error: Record does not implement IRace for {PropertyName}");
                return;
            }

            raceRecord.BipedObjectNames.Clear();
            if (value == null)
            {
                return;
            }

            foreach (var kvp in value)
            {
                raceRecord.BipedObjectNames[kvp.Key] = kvp.Value;
            }
        }

        public override bool AreValuesEqual(IReadOnlyDictionary<BipedObject, string>? value1, IReadOnlyDictionary<BipedObject, string>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1.Count != value2.Count) return false;

            foreach (var kvp in value1)
            {
                if (!value2.TryGetValue(kvp.Key, out var rhsValue))
                {
                    return false;
                }

                if (!StringComparisonHelper.EqualsNormalized(kvp.Value, rhsValue))
                {
                    return false;
                }
            }

            return true;
        }

        public override string FormatValue(object? value)
        {
            if (value is IReadOnlyDictionary<BipedObject, string> dict)
            {
                if (dict.Count == 0)
                {
                    return "Empty";
                }

                return string.Join(", ", dict.OrderBy(x => x.Key).Select(x => $"{x.Key}={x.Value}"));
            }

            return value?.ToString() ?? "null";
        }
    }
}
