using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.ArmorAddon
{
    public class BodyTemplateFirstPersonFlagsHandler : AbstractFlagPropertyHandler<BipedObjectFlag>
    {
        public override string PropertyName => "BodyTemplateFirstPersonFlags";

        public override void SetValue(IMajorRecord record, BipedObjectFlag value)
        {
            if (record is IArmorAddon armorAddon)
            {
                // Ensure BodyTemplate exists before setting flags
                if (armorAddon.BodyTemplate == null)
                {
                    armorAddon.BodyTemplate = new BodyTemplate();
                }
                armorAddon.BodyTemplate.FirstPersonFlags = value;
            }
            else
            {
                System.Console.WriteLine($"Error: Record does not implement IArmorAddon for {PropertyName}");
            }
        }

        public override BipedObjectFlag GetValue(IMajorRecordGetter record)
        {
            if (record is IArmorAddonGetter armorAddon)
            {
                // Return default if BodyTemplate is null
                return armorAddon.BodyTemplate?.FirstPersonFlags ?? default(BipedObjectFlag);
            }
            else
            {
                System.Console.WriteLine($"Error: Record does not implement IArmorAddonGetter for {PropertyName}");
            }
            return default(BipedObjectFlag);
        }

        protected override BipedObjectFlag[] GetAllFlags()
        {
            return System.Enum.GetValues<BipedObjectFlag>();
        }

        protected override bool IsFlagSet(BipedObjectFlag flags, BipedObjectFlag flag)
        {
            // Standard bitwise flag check - convert to underlying integer type for bitwise operations
            var flagsInt = System.Convert.ToInt64(flags);
            var flagInt = System.Convert.ToInt64(flag);
            return (flagsInt & flagInt) == flagInt;
        }

        protected override BipedObjectFlag SetFlag(BipedObjectFlag flags, BipedObjectFlag flag, bool value)
        {
            // Standard bitwise flag set/clear - convert to underlying integer type for bitwise operations
            var flagsInt = System.Convert.ToInt64(flags);
            var flagInt = System.Convert.ToInt64(flag);
            long result;
            if (value)
            {
                result = flagsInt | flagInt;
            }
            else
            {
                result = flagsInt & ~flagInt;
            }
            return (BipedObjectFlag)System.Enum.ToObject(typeof(BipedObjectFlag), result);
        }
    }
}
