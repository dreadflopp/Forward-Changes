using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class WorldspaceLocationPropertyHandler : AbstractPropertyHandler<IFormLinkNullableGetter<ILocationGetter>>
    {
        public override string PropertyName => "Location";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<ILocationGetter>? value)
        {
            if (record is IWorldspace worldspaceRecord)
            {
                if (value != null)
                {
                    worldspaceRecord.Location.SetTo(value.FormKey);
                }
                else
                {
                    worldspaceRecord.Location.SetTo(null);
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IWorldspace for {PropertyName}");
            }
        }

        public override IFormLinkNullableGetter<ILocationGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IWorldspaceGetter worldspaceRecord)
            {
                return worldspaceRecord.Location;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IWorldspaceGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<ILocationGetter>? value1, IFormLinkNullableGetter<ILocationGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            // Compare FormKeys
            return value1.FormKey == value2.FormKey;
        }
    }
}