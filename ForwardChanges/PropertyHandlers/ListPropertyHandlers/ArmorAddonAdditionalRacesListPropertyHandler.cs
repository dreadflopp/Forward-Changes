using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Noggog;

namespace ForwardChanges.PropertyHandlers.ListPropertyHandlers
{
    public class ArmorAddonAdditionalRacesListPropertyHandler : AbstractListPropertyHandler<IFormLinkGetter<IRaceGetter>>
    {
        public override string PropertyName => "AdditionalRaces";

        public override void SetValue(IMajorRecord record, List<IFormLinkGetter<IRaceGetter>>? value)
        {
            if (record is IArmorAddon armorAddonRecord)
            {
                if (value == null)
                {
                    Console.WriteLine($"Warning: Cannot set {PropertyName} to null - property is read-only");
                }
                else
                {
                    // AdditionalRaces is read-only, we can't modify it directly
                    Console.WriteLine($"Warning: Cannot set {PropertyName} - property is read-only");
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IArmorAddon for {PropertyName}");
            }
        }

        public override List<IFormLinkGetter<IRaceGetter>>? GetValue(IMajorRecordGetter record)
        {
            if (record is IArmorAddonGetter armorAddonRecord)
            {
                if (armorAddonRecord.AdditionalRaces == null)
                    return null;

                return armorAddonRecord.AdditionalRaces.ToList();
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IArmorAddonGetter for {PropertyName}");
            }
            return null;
        }

        protected override bool IsItemEqual(IFormLinkGetter<IRaceGetter>? item1, IFormLinkGetter<IRaceGetter>? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Compare by FormKey
            return item1.FormKey.Equals(item2.FormKey);
        }

        protected override string FormatItem(IFormLinkGetter<IRaceGetter>? item)
        {
            return item?.FormKey.ToString() ?? "null";
        }
    }
}