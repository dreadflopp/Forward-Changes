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
            if (record is ISoundDescriptor soundDescriptor)
            {
                soundDescriptor.PercentFrequencyShift = value;
            }
        }

        public override Percent GetValue(IMajorRecordGetter record)
        {
            if (record is ISoundDescriptorGetter soundDescriptor)
            {
                return soundDescriptor.PercentFrequencyShift;
            }
            return Percent.Zero;
        }

        public override bool AreValuesEqual(Percent value1, Percent value2)
        {
            return value1.Value.Equals(value2.Value);
        }
    }
}

