using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class ParentHandler : AbstractPropertyHandler<IWorldspaceParentGetter?>
    {
        public override string PropertyName => "Parent";

        public override void SetValue(IMajorRecord record, IWorldspaceParentGetter? value)
        {
            var worldspaceRecord = TryCastRecord<IWorldspace>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                if (value == null)
                {
                    worldspaceRecord.Parent = null;
                }
                else
                {
                    // Deep copy
                    var newParent = new WorldspaceParent();
                    newParent.DeepCopyIn(value);
                    worldspaceRecord.Parent = newParent;
                }
            }
        }

        public override IWorldspaceParentGetter? GetValue(IMajorRecordGetter record)
        {
            var worldspaceRecord = TryCastRecord<IWorldspaceGetter>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                return worldspaceRecord.Parent;
            }
            return null;
        }

        public override bool AreValuesEqual(IWorldspaceParentGetter? value1, IWorldspaceParentGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            // Use Mutagen's built-in equality
            return value1.Equals(value2);
        }

        public override string FormatValue(object? value)
        {
            if (value is not IWorldspaceParentGetter parent)
            {
                return value?.ToString() ?? "null";
            }

            var worldspace = !parent.Worldspace.FormKey.IsNull ? parent.Worldspace.FormKey.ToString() : "null";
            return $"Worldspace: {worldspace}, Flags: {parent.Flags}";
        }
    }
}