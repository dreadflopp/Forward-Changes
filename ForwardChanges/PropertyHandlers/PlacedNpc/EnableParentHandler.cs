using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class EnableParentHandler : AbstractPropertyHandler<IEnableParentGetter?>
    {
        public override string PropertyName => "EnableParent";

        public override void SetValue(IMajorRecord record, IEnableParentGetter? value)
        {
            if (record is IPlacedNpc placedNpcRecord)
            {
                if (value == null)
                {
                    placedNpcRecord.EnableParent = null;
                    return;
                }

                // Create a deep copy, ignoring the ReadOnlyMemorySlice<byte> Unknown property
                var newEnableParent = new EnableParent
                {
                    Versioning = value.Versioning,
                    Reference = new FormLink<ILinkedReferenceGetter>(value.Reference.FormKey),
                    Flags = value.Flags
                    // Note: Unknown property (ReadOnlyMemorySlice<byte>) is intentionally skipped
                    // as per documentation - complex binary data should be avoided
                };

                placedNpcRecord.EnableParent = newEnableParent;
            }
        }

        public override IEnableParentGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedNpcGetter placedNpcRecord)
            {
                return placedNpcRecord.EnableParent;
            }
            return null;
        }

        public override bool AreValuesEqual(IEnableParentGetter? value1, IEnableParentGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            return value1.Versioning == value2.Versioning &&
                   value1.Reference.FormKey == value2.Reference.FormKey &&
                   value1.Flags == value2.Flags;
            // Note: Unknown property comparison is intentionally skipped
        }

        public override string FormatValue(object? value)
        {
            if (value is not IEnableParentGetter enableParent)
            {
                return value?.ToString() ?? "null";
            }

            return $"Versioning: {enableParent.Versioning}, Reference: {enableParent.Reference.FormKey}, Flags: {enableParent.Flags}";
        }
    }
}