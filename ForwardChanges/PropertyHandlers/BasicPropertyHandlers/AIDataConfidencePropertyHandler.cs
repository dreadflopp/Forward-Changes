using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class AIDataConfidencePropertyHandler : AbstractPropertyHandler
    {
        public override string PropertyName => "AIData.Confidence";

        public override void SetValue(IMajorRecord record, object? value)
        {
            if (record is INpc npc && value is Confidence confidence)
            {
                if (npc.AIData == null)
                {
                    npc.AIData = new AIData();
                }
                npc.AIData.Confidence = confidence;
            }
        }

        public override object? GetValue(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            if (context.Record is INpcGetter npc)
            {
                return npc.AIData?.Confidence;
            }
            return null;
        }

        public override bool AreValuesEqual(object? value1, object? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1 is Confidence conf1 && value2 is Confidence conf2)
            {
                return conf1 == conf2;
            }
            return false;
        }
    }
}