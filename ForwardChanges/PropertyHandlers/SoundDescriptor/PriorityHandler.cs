using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.SoundDescriptor
{
    public class PriorityHandler : AbstractPropertyHandler<sbyte>
    {
        public override string PropertyName => "Priority";

        public override void SetValue(IMajorRecord record, sbyte value)
        {
            var soundDescriptor = TryCastRecord<ISoundDescriptor>(record, PropertyName);
            if (soundDescriptor != null)
            {
                soundDescriptor.Priority = value;
            }
        }

        public override sbyte GetValue(IMajorRecordGetter record)
        {
            var soundDescriptor = TryCastRecord<ISoundDescriptorGetter>(record, PropertyName);
            if (soundDescriptor != null)
            {
                return soundDescriptor.Priority;
            }
            return 0;
        }


    }
}

