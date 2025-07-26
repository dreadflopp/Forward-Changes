using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Noggog;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class ObjectBoundsPropertyHandler : AbstractPropertyHandler<IObjectBoundsGetter>
    {
        public override string PropertyName => "ObjectBounds";

        public override void SetValue(IMajorRecord record, IObjectBoundsGetter? value)
        {
            if (record is IObjectBounded objectBoundedRecord)
            {
                if (value == null)
                {
                    // ObjectBounds cannot be null, create a default object bounds
                    var defaultObjectBounds = new ObjectBounds();
                    objectBoundedRecord.ObjectBounds = defaultObjectBounds;
                }
                else
                {
                    // Create a deep copy of the object bounds
                    var newObjectBounds = new ObjectBounds();
                    newObjectBounds.First = value.First;
                    newObjectBounds.Second = value.Second;
                    objectBoundedRecord.ObjectBounds = newObjectBounds;
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IObjectBounded for {PropertyName}");
            }
        }

        public override IObjectBoundsGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IObjectBoundedGetter objectBoundedRecord)
            {
                return objectBoundedRecord.ObjectBounds;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IObjectBoundedGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IObjectBoundsGetter? value1, IObjectBoundsGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare the First and Second properties
            return value1.First.Equals(value2.First) && value1.Second.Equals(value2.Second);
        }
    }
}