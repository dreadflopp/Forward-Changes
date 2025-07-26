using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.SoundDescriptor
{
    public class SoundDescriptorPriorityPropertyHandler : AbstractPropertyHandler<sbyte>
    {
        public override string PropertyName => "Priority";

        public override void SetValue(IMajorRecord record, sbyte value)
        {
            if (record is ISoundDescriptor soundDescriptor)
            {
                soundDescriptor.Priority = value;
            }
        }

        public override sbyte GetValue(IMajorRecordGetter record)
        {
            if (record is ISoundDescriptorGetter soundDescriptor)
            {
                return soundDescriptor.Priority;
            }
            return 0;
        }

        public override bool AreValuesEqual(sbyte value1, sbyte value2)
        {
            return value1.Equals(value2);
        }
    }
}