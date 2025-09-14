using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Abstracts
{
    public abstract class AbstractFormLinkPropertyHandler<TRecord, TRecordGetter, TTarget> : AbstractPropertyHandler<IFormLinkNullableGetter<TTarget>>
        where TRecord : class, IMajorRecord
        where TRecordGetter : class, IMajorRecordGetter
        where TTarget : class, IMajorRecordGetter
    {
        public override IFormLinkNullableGetter<TTarget>? GetValue(IMajorRecordGetter record)
        {
            var typedRecord = TryCastRecord<TRecordGetter>(record, PropertyName);
            if (typedRecord != null)
            {
                return GetFormLinkValue(typedRecord);
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<TTarget>? value)
        {
            var typedRecord = TryCastRecord<TRecord>(record, PropertyName);
            if (typedRecord != null)
            {
                SetFormLinkValue(typedRecord, value);
            }
        }

        protected abstract IFormLinkNullableGetter<TTarget>? GetFormLinkValue(TRecordGetter record);
        protected abstract void SetFormLinkValue(TRecord record, IFormLinkNullableGetter<TTarget>? value);

        public override bool AreValuesEqual(IFormLinkNullableGetter<TTarget>? value1, IFormLinkNullableGetter<TTarget>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}