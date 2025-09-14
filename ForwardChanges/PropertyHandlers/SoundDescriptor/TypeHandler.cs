using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.SoundDescriptor
{
    public class TypeHandler : AbstractPropertyHandler<Mutagen.Bethesda.Skyrim.SoundDescriptor.DescriptorType?>
    {
        public override string PropertyName => "Type";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.SoundDescriptor.DescriptorType? value)
        {
            var soundDescriptor = TryCastRecord<ISoundDescriptor>(record, PropertyName);
            if (soundDescriptor != null)
            {
                soundDescriptor.Type = value;
            }
        }

        public override Mutagen.Bethesda.Skyrim.SoundDescriptor.DescriptorType? GetValue(IMajorRecordGetter record)
        {
            var soundDescriptor = TryCastRecord<ISoundDescriptorGetter>(record, PropertyName);
            if (soundDescriptor != null)
            {
                return soundDescriptor.Type;
            }
            return null;
        }
    }
}