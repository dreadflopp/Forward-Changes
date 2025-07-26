using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.SoundDescriptor
{
    public class SoundDescriptorStaticAttenuationPropertyHandler : AbstractPropertyHandler<float>
    {
        public override string PropertyName => "StaticAttenuation";

        public override void SetValue(IMajorRecord record, float value)
        {
            if (record is ISoundDescriptor soundDescriptor)
            {
                soundDescriptor.StaticAttenuation = value;
            }
        }

        public override float GetValue(IMajorRecordGetter record)
        {
            if (record is ISoundDescriptorGetter soundDescriptor)
            {
                return soundDescriptor.StaticAttenuation;
            }
            return 0.0f;
        }

        public override bool AreValuesEqual(float value1, float value2)
        {
            return value1.Equals(value2);
        }
    }
}