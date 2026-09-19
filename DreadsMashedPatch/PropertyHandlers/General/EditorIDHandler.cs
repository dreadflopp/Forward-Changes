using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;

namespace DreadsMashedPatch.PropertyHandlers.General
{
    public class EditorIDHandler : AbstractPropertyHandler<string>
    {
        public override string PropertyName => "EditorID";

        public override void SetValue(IMajorRecord record, string? value)
        {
            record.EditorID = value;
        }

        public override string? GetValue(IMajorRecordGetter record)
        {
            return record.EditorID;
        }
    }
}
