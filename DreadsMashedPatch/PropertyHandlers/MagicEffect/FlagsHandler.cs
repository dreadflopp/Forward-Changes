using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class FlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.MagicEffect.Flag>
    {
        public override string PropertyName => "Flags";

        public override Mutagen.Bethesda.Skyrim.MagicEffect.Flag GetValue(IMajorRecordGetter record)
        {
            if (record is IMagicEffectGetter magicEffect)
            {
                return magicEffect.Flags;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
                return default(Mutagen.Bethesda.Skyrim.MagicEffect.Flag);
            }
        }

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.MagicEffect.Flag value)
        {
            if (record is IMagicEffect magicEffect)
            {
                magicEffect.Flags = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffect for {PropertyName}");
            }
        }

        protected override Mutagen.Bethesda.Skyrim.MagicEffect.Flag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.MagicEffect.Flag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.MagicEffect.Flag flags, Mutagen.Bethesda.Skyrim.MagicEffect.Flag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Mutagen.Bethesda.Skyrim.MagicEffect.Flag SetFlag(Mutagen.Bethesda.Skyrim.MagicEffect.Flag flags, Mutagen.Bethesda.Skyrim.MagicEffect.Flag flag, bool value)
        {
            if (value)
            {
                return flags | flag;
            }
            else
            {
                return flags & ~flag;
            }
        }
    }
}
