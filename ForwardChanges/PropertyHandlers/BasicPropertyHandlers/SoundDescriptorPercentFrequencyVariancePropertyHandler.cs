using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class SoundDescriptorPercentFrequencyVariancePropertyHandler : AbstractPropertyHandler<Percent>
    {
        public override string PropertyName => "PercentFrequencyVariance";

        public override void SetValue(IMajorRecord record, Percent value)
        {
            if (record is ISoundDescriptor soundDescriptor)
            {
                soundDescriptor.PercentFrequencyVariance = value;
            }
        }

        public override Percent GetValue(IMajorRecordGetter record)
        {
            if (record is ISoundDescriptorGetter soundDescriptor)
            {
                return soundDescriptor.PercentFrequencyVariance;
            }
            return Percent.Zero;
        }

        public override bool AreValuesEqual(Percent value1, Percent value2)
        {
            return value1.Value.Equals(value2.Value);
        }
    }
}