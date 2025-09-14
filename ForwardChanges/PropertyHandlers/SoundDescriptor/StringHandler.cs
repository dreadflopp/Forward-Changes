using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.SoundDescriptor
{
    public class StringHandler : AbstractPropertyHandler<string?>
    {
        public override string PropertyName => "String";

        public override void SetValue(IMajorRecord record, string? value)
        {
            var soundDescriptor = TryCastRecord<ISoundDescriptor>(record, PropertyName);
            if (soundDescriptor != null)
            {
                soundDescriptor.String = value;
            }
        }

        public override string? GetValue(IMajorRecordGetter record)
        {
            var soundDescriptor = TryCastRecord<ISoundDescriptorGetter>(record, PropertyName);
            if (soundDescriptor != null)
            {
                return soundDescriptor.String;
            }
            return null;
        }
    }
}