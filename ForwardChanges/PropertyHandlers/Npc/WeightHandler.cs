using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class WeightHandler : AbstractPropertyHandler<float>
    {
        public override string PropertyName => "Weight";

        public override void SetValue(IMajorRecord record, float value)
        {
            if (record is INpc npc)
            {
                npc.Weight = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpc for {PropertyName}");
            }
        }

        public override float GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npc)
            {
                return npc.Weight;
            }

            Console.WriteLine($"Error: Record does not implement INpcGetter for {PropertyName}");
            return 0f;
        }
    }
}
