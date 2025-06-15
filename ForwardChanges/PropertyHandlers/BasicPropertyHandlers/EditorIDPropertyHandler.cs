using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class EditorIDPropertyHandler : AbstractPropertyHandler
    {
        public override string PropertyName => "EditorID";

        public override object? GetValue(IMajorRecordGetter record)
        {
            return record.EditorID;
        }

        public override void SetValue(IMajorRecord record, object? value)
        {
            if (value is string editorId)
            {
                record.EditorID = editorId;
            }
        }

        public override object? GetValueFromContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            return context.Record.EditorID;
        }

        public override bool AreValuesEqual(object? value1, object? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1 is string str1 && value2 is string str2)
            {
                return string.Equals(str1, str2);
            }
            return false;
        }
    }
}