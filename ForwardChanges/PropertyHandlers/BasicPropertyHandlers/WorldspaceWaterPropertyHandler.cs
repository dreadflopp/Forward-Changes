using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class WorldspaceWaterPropertyHandler : AbstractPropertyHandler<IFormLinkNullableGetter<IWaterGetter>>
    {
        public override string PropertyName => "Water";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<IWaterGetter>? value)
        {
            if (record is IWorldspace worldspaceRecord)
            {
                if (value != null)
                {
                    worldspaceRecord.Water.SetTo(value.FormKey);
                }
                else
                {
                    worldspaceRecord.Water.SetTo(null);
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IWorldspace for {PropertyName}");
            }
        }

        public override IFormLinkNullableGetter<IWaterGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IWorldspaceGetter worldspaceRecord)
            {
                return worldspaceRecord.Water;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IWorldspaceGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<IWaterGetter>? value1, IFormLinkNullableGetter<IWaterGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            // Compare FormKeys
            return value1.FormKey == value2.FormKey;
        }
    }
}