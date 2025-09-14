using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.SoundDescriptor
{
    public class StaticAttenuationHandler : AbstractPropertyHandler<float>
    {
        public override string PropertyName => "StaticAttenuation";

        public override void SetValue(IMajorRecord record, float value)
        {
            var soundDescriptor = TryCastRecord<ISoundDescriptor>(record, PropertyName);
            if (soundDescriptor != null)
            {
                soundDescriptor.StaticAttenuation = value;
            }
        }

        public override float GetValue(IMajorRecordGetter record)
        {
            var soundDescriptor = TryCastRecord<ISoundDescriptorGetter>(record, PropertyName);
            if (soundDescriptor != null)
            {
                return soundDescriptor.StaticAttenuation;
            }
            return 0.0f;
        }


    }
}

