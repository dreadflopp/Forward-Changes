using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class EditorIDPropertyHandler : AbstractPropertyHandler<string>
    {
        public override string PropertyName => "EditorID";

        public override void SetValue(IMajorRecord record, string? value)
        {
            if (record is INpc npc)
            {
                npc.EditorID = value;
            }
            else
            {
                Console.WriteLine($"Error: Record is not an NPC for {PropertyName}");
            }
        }

        public override string? GetValue(
            IMajorRecordGetter record)
        {
            if (record is INpcGetter npc)
            {
                return npc.EditorID;
            }
            else
            {
                Console.WriteLine($"Error: Record is not an NPC for {PropertyName}");
            }
            return null;
        }
    }
}