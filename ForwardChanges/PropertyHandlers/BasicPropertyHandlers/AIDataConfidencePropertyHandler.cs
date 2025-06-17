using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class AIDataConfidencePropertyHandler : AbstractPropertyHandler<Confidence>
    {
        public override string PropertyName => "AIData.Confidence";

        public override void SetValue(IMajorRecord record, Confidence value)
        {
            if (record is INpc npc)
            {
                if (npc.AIData == null)
                {
                    npc.AIData = new AIData();
                }
                npc.AIData.Confidence = value;
            }
        }

        public override Confidence GetValue(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (context.Record is INpcGetter npc && npc.AIData?.Confidence != null)
            {
                return npc.AIData.Confidence;
            }
            return Confidence.Cowardly;
        }
    }
}