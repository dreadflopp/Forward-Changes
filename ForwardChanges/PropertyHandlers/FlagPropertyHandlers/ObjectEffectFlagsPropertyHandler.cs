using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.FlagPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.FlagPropertyHandlers
{
    public class ObjectEffectFlagsPropertyHandler : AbstractFlagPropertyHandler<ObjectEffect.Flag>
    {
        public override string PropertyName => "Flags";

        public override void SetValue(IMajorRecord record, ObjectEffect.Flag value)
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

        public override ObjectEffect.Flag GetValue(IMajorRecordGetter record)
        {
            if (record is IObjectEffectGetter objectEffectRecord)
            {
                return objectEffectRecord.Flags;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IObjectEffectGetter for {PropertyName}");
            }
            return ObjectEffect.Flag.NoAutoCalc; // Default value
        }

        protected override ObjectEffect.Flag[] GetAllFlags()
        {
            return new ObjectEffect.Flag[]
            {
                ObjectEffect.Flag.NoAutoCalc,
                ObjectEffect.Flag.ExtendDurationOnRecast
            };
        }

        protected override bool IsFlagSet(ObjectEffect.Flag flags, ObjectEffect.Flag flag)
        {
            return flags.HasFlag(flag);
        }

        protected override ObjectEffect.Flag SetFlag(ObjectEffect.Flag flags, ObjectEffect.Flag flag, bool value)
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

        protected override string FormatFlag(ObjectEffect.Flag flag)
        {
            return flag switch
            {
                ObjectEffect.Flag.NoAutoCalc => "NoAutoCalc",
                ObjectEffect.Flag.ExtendDurationOnRecast => "ExtendDurationOnRecast",
                _ => flag.ToString()
            };
        }
    }
}