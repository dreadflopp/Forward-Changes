using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.SoundDescriptor
{
    public class PercentFrequencyShiftHandler : AbstractPropertyHandler<Percent>
    {
        public override string PropertyName => "PercentFrequencyShift";

        public override void SetValue(IMajorRecord record, Percent value)
        {
            var soundDescriptor = TryCastRecord<ISoundDescriptor>(record, PropertyName);
            if (soundDescriptor != null)
            {
                soundDescriptor.PercentFrequencyShift = value;
            }
        }

        public override Percent GetValue(IMajorRecordGetter record)
        {
            var soundDescriptor = TryCastRecord<ISoundDescriptorGetter>(record, PropertyName);
            if (soundDescriptor != null)
            {
                return soundDescriptor.PercentFrequencyShift;
            }
            return Percent.Zero;
        }


    }
}

