using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class ObjectBoundsMinHandler : AbstractPropertyHandler<P2Float>
    {
        public override string PropertyName => "ObjectBoundsMin";

        public override void SetValue(IMajorRecord record, P2Float value)
        {
            if (record is IWorldspace worldspaceRecord)
            {
                worldspaceRecord.ObjectBoundsMin = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IWorldspace for {PropertyName}");
            }
        }

        public override P2Float GetValue(IMajorRecordGetter record)
        {
            if (record is IWorldspaceGetter worldspaceRecord)
            {
                return worldspaceRecord.ObjectBoundsMin;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IWorldspaceGetter for {PropertyName}");
            }
            return default;
        }

        public override bool AreValuesEqual(P2Float value1, P2Float value2)
        {
            // Use P2Float's built-in equality
            return value1.Equals(value2);
        }
    }
}

