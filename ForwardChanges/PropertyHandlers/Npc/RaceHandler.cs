using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class RaceHandler : AbstractPropertyHandler<IFormLinkGetter<IRaceGetter>>
    {
        public override string PropertyName => "Race";

        public override IFormLinkGetter<IRaceGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npcRecord)
            {
                return npcRecord.Race;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpcGetter for {PropertyName}");
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, IFormLinkGetter<IRaceGetter>? value)
        {
            if (record is INpc npcRecord)
            {
                if (value != null && !value.FormKey.IsNull)
                {
                    npcRecord.Race = new FormLinkNullable<IRaceGetter>(value.FormKey);
                }
                else
                {
                    npcRecord.Race.Clear();
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpc for {PropertyName}");
            }
        }

        public override bool AreValuesEqual(IFormLinkGetter<IRaceGetter>? value1, IFormLinkGetter<IRaceGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}