using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class SecondActorValueHandler : AbstractPropertyHandler<ActorValue>
    {
        public override string PropertyName => "SecondActorValue";

        public override void SetValue(IMajorRecord record, ActorValue value)
        {
            if (record is IMagicEffect magicEffect)
            {
                magicEffect.SecondActorValue = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffect for {PropertyName}");
            }
        }

        public override ActorValue GetValue(IMajorRecordGetter record)
        {
            if (record is IMagicEffectGetter magicEffect)
            {
                return magicEffect.SecondActorValue;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
            }
            return ActorValue.None;
        }
    }
}
