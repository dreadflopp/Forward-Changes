using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.SoundDescriptor
{
    public class LoopAndRumbleHandler : AbstractPropertyHandler<ISoundLoopAndRumbleGetter?>
    {
        public override string PropertyName => "LoopAndRumble";

        public override void SetValue(IMajorRecord record, ISoundLoopAndRumbleGetter? value)
        {
            var soundDescriptor = TryCastRecord<ISoundDescriptor>(record, PropertyName);
            if (soundDescriptor != null)
            {
                if (value == null)
                {
                    soundDescriptor.LoopAndRumble = null;
                }
                else
                {
                    // Deep copy the SoundLoopAndRumble object
                    var newLoopAndRumble = new SoundLoopAndRumble();
                    newLoopAndRumble.Unknown = value.Unknown;
                    newLoopAndRumble.Loop = value.Loop;
                    newLoopAndRumble.Unknown2 = value.Unknown2;
                    newLoopAndRumble.RumbleValues = value.RumbleValues;
                    soundDescriptor.LoopAndRumble = newLoopAndRumble;
                }
            }
        }

        public override ISoundLoopAndRumbleGetter? GetValue(IMajorRecordGetter record)
        {
            var soundDescriptor = TryCastRecord<ISoundDescriptorGetter>(record, PropertyName);
            if (soundDescriptor != null)
            {
                return soundDescriptor.LoopAndRumble;
            }
            return null;
        }

        public override bool AreValuesEqual(ISoundLoopAndRumbleGetter? value1, ISoundLoopAndRumbleGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare all properties for deep equality
            return value1.Unknown == value2.Unknown &&
                   value1.Loop == value2.Loop &&
                   value1.Unknown2 == value2.Unknown2 &&
                   value1.RumbleValues == value2.RumbleValues;
        }

        public override string FormatValue(object? value)
        {
            if (value is not ISoundLoopAndRumbleGetter loopAndRumble)
            {
                return value?.ToString() ?? "null";
            }

            return $"Loop: {loopAndRumble.Loop}, RumbleValues: {loopAndRumble.RumbleValues}, Unknown: {loopAndRumble.Unknown}, Unknown2: {loopAndRumble.Unknown2}";
        }
    }
}