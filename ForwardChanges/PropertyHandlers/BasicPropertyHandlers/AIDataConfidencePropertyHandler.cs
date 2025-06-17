using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class AIDataConfidencePropertyHandler : AbstractPropertyHandler<Confidence>, IPropertyHandler<object>
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

        public override bool AreValuesEqual(Confidence value1, Confidence value2)
        {
            return value1 == value2;
        }

        // IPropertyHandler<object> implementation
        void IPropertyHandler<object>.SetValue(IMajorRecord record, object? value)
        {
            if (value is Confidence confidence)
            {
                SetValue(record, confidence);
            }
        }

        object? IPropertyHandler<object>.GetValue(IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            return GetValue(context);
        }

        bool IPropertyHandler<object>.AreValuesEqual(object? value1, object? value2)
        {
            if (value1 is Confidence conf1 && value2 is Confidence conf2)
            {
                return AreValuesEqual(conf1, conf2);
            }
            return false;
        }
    }
}