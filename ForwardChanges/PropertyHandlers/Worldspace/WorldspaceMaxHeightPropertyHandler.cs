using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class WorldspaceMaxHeightPropertyHandler : AbstractPropertyHandler<IWorldspaceMaxHeightGetter>
    {
        public override string PropertyName => "MaxHeight";

        public override void SetValue(IMajorRecord record, IWorldspaceMaxHeightGetter? value)
        {
            if (record is IWorldspace worldspaceRecord)
            {
                if (value == null)
                {
                    worldspaceRecord.MaxHeight = null;
                }
                else
                {
                    // Deep copy
                    var newMaxHeight = new WorldspaceMaxHeight();
                    newMaxHeight.DeepCopyIn(value);
                    worldspaceRecord.MaxHeight = newMaxHeight;
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IWorldspace for {PropertyName}");
            }
        }

        public override IWorldspaceMaxHeightGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IWorldspaceGetter worldspaceRecord)
            {
                return worldspaceRecord.MaxHeight;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IWorldspaceGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IWorldspaceMaxHeightGetter? value1, IWorldspaceMaxHeightGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            // Use Mutagen's built-in equality
            return value1.Equals(value2);
        }
    }
}