using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class EditorIDPropertyHandler : AbstractPropertyHandler<string>, IPropertyHandler<object>
    {
        public override string PropertyName => "EditorID";

        public override void SetValue(IMajorRecord record, string? value)
        {
            if (record is INpc npc)
            {
                npc.EditorID = value;
            }
        }

        public override string? GetValue(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (context.Record is INpcGetter npc)
            {
                return npc.EditorID;
            }
            return null;
        }

        public override bool AreValuesEqual(string? value1, string? value2)
        {
            return string.Equals(value1, value2, StringComparison.OrdinalIgnoreCase);
        }

        // IPropertyHandler<object> implementation
        void IPropertyHandler<object>.SetValue(IMajorRecord record, object? value) => SetValue(record, (string?)value);
        object? IPropertyHandler<object>.GetValue(IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context) => GetValue(context);
        bool IPropertyHandler<object>.AreValuesEqual(object? value1, object? value2) => AreValuesEqual((string?)value1, (string?)value2);
    }
}