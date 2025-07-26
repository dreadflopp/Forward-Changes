using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.ObjectEffect
{
    public class ObjectEffectFlagsPropertyHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.ObjectEffect.Flag>
    {
        public override string PropertyName => "Flags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.ObjectEffect.Flag value)
        {
            if (record is IObjectEffect objectEffectRecord)
            {
                objectEffectRecord.Flags = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IObjectEffect for {PropertyName}");
            }
        }

        public override Mutagen.Bethesda.Skyrim.ObjectEffect.Flag GetValue(IMajorRecordGetter record)
        {
            if (record is IObjectEffectGetter objectEffectRecord)
            {
                return objectEffectRecord.Flags;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IObjectEffectGetter for {PropertyName}");
            }
            return Mutagen.Bethesda.Skyrim.ObjectEffect.Flag.NoAutoCalc; // Default value
        }

        protected override Mutagen.Bethesda.Skyrim.ObjectEffect.Flag[] GetAllFlags()
        {
            return new Mutagen.Bethesda.Skyrim.ObjectEffect.Flag[]
            {
                Mutagen.Bethesda.Skyrim.ObjectEffect.Flag.NoAutoCalc,
                Mutagen.Bethesda.Skyrim.ObjectEffect.Flag.ExtendDurationOnRecast
            };
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.ObjectEffect.Flag flags, Mutagen.Bethesda.Skyrim.ObjectEffect.Flag flag)
        {
            return flags.HasFlag(flag);
        }

        protected override Mutagen.Bethesda.Skyrim.ObjectEffect.Flag SetFlag(Mutagen.Bethesda.Skyrim.ObjectEffect.Flag flags, Mutagen.Bethesda.Skyrim.ObjectEffect.Flag flag, bool value)
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

        protected override string FormatFlag(Mutagen.Bethesda.Skyrim.ObjectEffect.Flag flag)
        {
            return flag switch
            {
                Mutagen.Bethesda.Skyrim.ObjectEffect.Flag.NoAutoCalc => "NoAutoCalc",
                Mutagen.Bethesda.Skyrim.ObjectEffect.Flag.ExtendDurationOnRecast => "ExtendDurationOnRecast",
                _ => flag.ToString()
            };
        }
    }
}