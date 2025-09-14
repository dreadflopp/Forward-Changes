using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Cell
{
    public class WaterNoiseTextureHandler : AbstractPropertyHandler<string>
    {
        public override string PropertyName => "WaterNoiseTexture";

        public override void SetValue(IMajorRecord record, string? value)
        {
            if (record is ICell cellRecord)
            {
                cellRecord.WaterNoiseTexture = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ICell for {PropertyName}");
            }
        }

        public override string? GetValue(IMajorRecordGetter record)
        {
            if (record is ICellGetter cellRecord)
            {
                return cellRecord.WaterNoiseTexture;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ICellGetter for {PropertyName}");
            }
            return null;
        }
    }
}