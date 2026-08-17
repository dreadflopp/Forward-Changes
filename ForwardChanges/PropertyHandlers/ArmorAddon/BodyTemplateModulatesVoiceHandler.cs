using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.ArmorAddon
{
    /// <summary>
    /// Handles only the ModulatesVoice bit of BodyTemplate.Flags. Unknown bits are preserved on set.
    /// </summary>
    public class BodyTemplateModulatesVoiceHandler : AbstractPropertyHandler<bool>
    {
        private static readonly long FlagBit = (long)BodyTemplate.Flag.ModulatesVoice;

        public override string PropertyName => "BodyTemplateModulatesVoice";

        public override void SetValue(IMajorRecord record, bool value)
        {
            if (record is not IArmorAddon armorAddon) return;
            if (armorAddon.BodyTemplate == null)
                armorAddon.BodyTemplate = new BodyTemplate();
            long current = System.Convert.ToInt64(armorAddon.BodyTemplate.Flags);
            long updated = value ? (current | FlagBit) : (current & ~FlagBit);
            armorAddon.BodyTemplate.Flags = (BodyTemplate.Flag)System.Enum.ToObject(typeof(BodyTemplate.Flag), updated);
        }

        public override bool GetValue(IMajorRecordGetter record)
        {
            if (record is not IArmorAddonGetter armorAddon) return false;
            var flags = armorAddon.BodyTemplate?.Flags ?? default(BodyTemplate.Flag);
            return (System.Convert.ToInt64(flags) & FlagBit) == FlagBit;
        }
    }
}
