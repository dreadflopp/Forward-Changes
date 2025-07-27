using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class OwnerHandler : AbstractPropertyHandler<IFormLinkNullableGetter<IOwnerGetter>>
    {
        public override string PropertyName => "Owner";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<IOwnerGetter>? value)
        {
            if (record is IPlacedObject placedObject)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    placedObject.Owner = new FormLinkNullable<IOwnerGetter>(value.FormKey);
                }
                else
                {
                    placedObject.Owner.Clear();
                }
            }
            else
            {
                Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            }
        }

        public override IFormLinkNullableGetter<IOwnerGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObject)
            {
                return placedObject.Owner;
            }
            else
            {
                Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<IOwnerGetter>? value1, IFormLinkNullableGetter<IOwnerGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}

