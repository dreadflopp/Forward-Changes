using System;
using System.Collections.Generic;
using System.Linq;
using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace ForwardChanges.PropertyHandlers.Package
{
    public class PackageDataDictionaryHandler : AbstractPropertyHandler<IReadOnlyDictionary<sbyte, IAPackageDataGetter>>
    {
        public override string PropertyName => "Data";

        public override IReadOnlyDictionary<sbyte, IAPackageDataGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IPackageGetter packageRecord)
            {
                return packageRecord.Data;
            }

            Console.WriteLine($"Error: Record does not implement IPackageGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, IReadOnlyDictionary<sbyte, IAPackageDataGetter>? value)
        {
            if (record is not IPackage packageRecord)
            {
                Console.WriteLine($"Error: Record does not implement IPackage for {PropertyName}");
                return;
            }

            packageRecord.Data.Clear();
            if (value == null)
            {
                return;
            }

            foreach (var kvp in value)
            {
                if (kvp.Value == null)
                {
                    continue;
                }

                packageRecord.Data[kvp.Key] = kvp.Value.DeepCopy();
            }
        }

        public override bool AreValuesEqual(IReadOnlyDictionary<sbyte, IAPackageDataGetter>? value1, IReadOnlyDictionary<sbyte, IAPackageDataGetter>? value2)
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

                if (kvp.Value == null && rhsValue == null)
                {
                    continue;
                }

                if (kvp.Value == null || rhsValue == null)
                {
                    return false;
                }

                if (!kvp.Value.Equals(rhsValue))
                {
                    return false;
                }
            }

            return true;
        }

        public override string FormatValue(object? value)
        {
            if (value is IReadOnlyDictionary<sbyte, IAPackageDataGetter> dict)
            {
                if (dict.Count == 0)
                {
                    return "Empty";
                }

                return string.Join(", ", dict.OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value?.GetType().Name ?? "null"}"));
            }

            return value?.ToString() ?? "null";
        }
    }
}
