using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.SoundDescriptor
{
    public class VarianceHandler : AbstractPropertyHandler<sbyte>
    {
        public override string PropertyName => "Variance";

        public override void SetValue(IMajorRecord record, sbyte value)
        {
            var soundDescriptor = TryCastRecord<ISoundDescriptor>(record, PropertyName);
            if (soundDescriptor != null)
            {
                soundDescriptor.Variance = value;
            }
        }

        public override sbyte GetValue(IMajorRecordGetter record)
        {
            var soundDescriptor = TryCastRecord<ISoundDescriptorGetter>(record, PropertyName);
            if (soundDescriptor != null)
            {
                return soundDescriptor.Variance;
            }
            return 0;
        }


    }
}

