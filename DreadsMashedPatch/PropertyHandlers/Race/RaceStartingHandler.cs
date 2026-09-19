using System;
using System.Collections.Generic;
using System.Linq;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace DreadsMashedPatch.PropertyHandlers.Race
{
    public class RaceStartingHandler : AbstractPropertyHandler<IReadOnlyDictionary<BasicStat, float>>
    {
        public override string PropertyName => "Starting";

        public override IReadOnlyDictionary<BasicStat, float>? GetValue(IMajorRecordGetter record)
        {
            if (record is IRaceGetter raceRecord)
            {
                return raceRecord.Starting;
            }

            Console.WriteLine($"Error: Record does not implement IRaceGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, IReadOnlyDictionary<BasicStat, float>? value)
        {
            if (record is not IRace raceRecord)
            {
                Console.WriteLine($"Error: Record does not implement IRace for {PropertyName}");
                return;
            }

            raceRecord.Starting.Clear();
            if (value == null)
            {
                return;
            }

            foreach (var kvp in value)
            {
                raceRecord.Starting[kvp.Key] = kvp.Value;
            }
        }

        public override bool AreValuesEqual(IReadOnlyDictionary<BasicStat, float>? value1, IReadOnlyDictionary<BasicStat, float>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1.Count != value2.Count) return false;

            foreach (var kvp in value1)
            {
                if (!value2.TryGetValue(kvp.Key, out var rhsValue) || kvp.Value != rhsValue)
                {
                    return false;
                }
            }

            return true;
        }

        public override string FormatValue(object? value)
        {
            if (value is IReadOnlyDictionary<BasicStat, float> dict)
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
