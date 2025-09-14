using System.Drawing;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class LinkedReferenceColorHandler : AbstractPropertyHandler<ILinkedReferenceColorGetter?>
    {
        public override string PropertyName => "LinkedReferenceColor";

        public override void SetValue(IMajorRecord record, ILinkedReferenceColorGetter? value)
        {
            if (record is IPlacedNpc placedNpcRecord)
            {
                if (value == null)
                {
                    placedNpcRecord.LinkedReferenceColor = null;
                    return;
                }

                // Create a deep copy
                var newLinkedReferenceColor = new LinkedReferenceColor
                {
                    Start = value.Start,
                    End = value.End
                };

                placedNpcRecord.LinkedReferenceColor = newLinkedReferenceColor;
            }
        }

        public override ILinkedReferenceColorGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedNpcGetter placedNpcRecord)
            {
                return placedNpcRecord.LinkedReferenceColor;
            }
            return null;
        }

        public override bool AreValuesEqual(ILinkedReferenceColorGetter? value1, ILinkedReferenceColorGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            return value1.Start == value2.Start && value1.End == value2.End;
        }

        public override string FormatValue(object? value)
        {
            if (value is not ILinkedReferenceColorGetter linkedReferenceColor)
            {
                return value?.ToString() ?? "null";
            }

            return $"Start: {linkedReferenceColor.Start}, End: {linkedReferenceColor.End}";
        }
    }
}