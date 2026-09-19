using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using DreadsMashedPatch.PropertyHandlers.Abstracts;

namespace DreadsMashedPatch.PropertyHandlers.MaterialObject
{
    public class DNAMsHandler : AbstractPropertyHandler<List<ReadOnlyMemorySlice<byte>>?>
    {
        public override string PropertyName => "DNAMs";

        public override List<ReadOnlyMemorySlice<byte>>? GetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecordGetter record)
        {
            if (record is IMaterialObjectGetter materialObject)
            {
                return materialObject.DNAMs?.ToList();
            }

            return null;
        }

        public override void SetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecord record, List<ReadOnlyMemorySlice<byte>>? value)
        {
            if (record is not IMaterialObject materialObject)
            {
                return;
            }

            materialObject.DNAMs.Clear();
            if (value == null)
            {
                return;
            }

            foreach (var slice in value)
            {
                materialObject.DNAMs.Add(new MemorySlice<byte>(slice.ToArray()));
            }
        }

        public override bool AreValuesEqual(List<ReadOnlyMemorySlice<byte>>? value1, List<ReadOnlyMemorySlice<byte>>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1.Count != value2.Count) return false;

            for (var i = 0; i < value1.Count; i++)
            {
                if (!value1[i].Span.SequenceEqual(value2[i].Span))
                {
                    return false;
                }
            }

            return true;
        }
    }
}