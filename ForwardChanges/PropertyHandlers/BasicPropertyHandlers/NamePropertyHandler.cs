using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Aspects;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class NamePropertyHandler : AbstractPropertyHandler
    {
        public override string PropertyName => "Name";

        public override object? GetValue(IMajorRecordGetter record)
        {
            if (record is INamedGetter named)
                return named.Name;
            return null;
        }

        public override void SetValue(IMajorRecord record, object? value)
        {
            if (record is INamed named && value is string name)
            {
                named.Name = name;
            }
        }

        public override object? GetValueFromContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (context.Record is INamedGetter named)
            {
                return named.Name;
            }
            return null;
        }

        public override bool AreValuesEqual(object? value1, object? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.Equals(value2);
        }
    }
}