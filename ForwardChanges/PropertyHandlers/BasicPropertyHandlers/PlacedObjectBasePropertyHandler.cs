using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class PlacedObjectBasePropertyHandler : AbstractPropertyHandler<IFormLinkNullableGetter<IPlaceableObjectGetter>>
    {
        public override string PropertyName => "Base";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<IPlaceableObjectGetter>? value)
        {
            if (record is IPlacedObject placedObject)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    placedObject.Base = new FormLinkNullable<IPlaceableObjectGetter>(value.FormKey);
                }
                else
                {
                    placedObject.Base.Clear();
                }
            }
            else
            {
                Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            }
        }

        public override IFormLinkNullableGetter<IPlaceableObjectGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObject)
            {
                return placedObject.Base;
            }
            else
            {
                Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<IPlaceableObjectGetter>? value1, IFormLinkNullableGetter<IPlaceableObjectGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}