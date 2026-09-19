using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Noggog;

namespace ForwardChanges.PropertyHandlers.ArmorAddon
{
    public class AdditionalRacesHandler : AbstractListPropertyHandler<IFormLinkGetter<IRaceGetter>>
    {
        public override string PropertyName => "AdditionalRaces";
        public override ListSemantics Semantics => ListSemantics.SortedKeyed;

        protected override IReadOnlyList<object?> GetSortKey(IFormLinkGetter<IRaceGetter> item) => [item.FormKey];

        public override void SetValue(IMajorRecord record, List<IFormLinkGetter<IRaceGetter>>? value)
        {
            if (record is IArmorAddon armorAddonRecord)
            {
                if (value == null)
                {
                    armorAddonRecord.AdditionalRaces.Clear();
                    return;
                }

                // Clear existing races and add new ones
                armorAddonRecord.AdditionalRaces.Clear();
                foreach (var race in value)
                {
                    if (race != null && !race.FormKey.IsNull)
                    {
                        armorAddonRecord.AdditionalRaces.Add(new FormLink<IRaceGetter>(race.FormKey));
                    }
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

        public override bool AreValuesEqual(List<IFormLinkGetter<IRaceGetter>>? value1, List<IFormLinkGetter<IRaceGetter>>? value2)
        {
            // Treat null and empty list as equivalent
            var count1 = value1?.Count ?? 0;
            var count2 = value2?.Count ?? 0;
            if (count1 != count2) return false;
            if (count1 == 0) return true;

            return base.AreValuesEqual(value1, value2);
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

