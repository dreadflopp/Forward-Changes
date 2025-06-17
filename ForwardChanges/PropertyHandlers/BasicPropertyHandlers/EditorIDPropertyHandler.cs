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
        }

        public override string? GetValue(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (context.Record is INpcGetter npc)
            {
                return npc.EditorID;
            }
            return null;
        }
    }
}