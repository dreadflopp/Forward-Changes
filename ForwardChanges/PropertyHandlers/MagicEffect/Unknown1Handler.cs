using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class Unknown1Handler : AbstractPropertyHandler<ushort>
    {
        public override string PropertyName => "Unknown1";

        public override void SetValue(IMajorRecord record, ushort value)
        {
            if (record is IMagicEffect magicEffect)
            {
                magicEffect.Unknown1 = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffect for {PropertyName}");
            }
        }

        public override ushort GetValue(IMajorRecordGetter record)
        {
            if (record is IMagicEffectGetter magicEffect)
            {
                return magicEffect.Unknown1;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IMagicEffectGetter for {PropertyName}");
            }
            return 0;
        }
    }
}
