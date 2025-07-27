using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Noggog;

namespace ForwardChanges.PropertyHandlers.General
{
    public class ModelHandler : AbstractPropertyHandler<ISimpleModelGetter>
    {
        public override string PropertyName => "Model";

        public override void SetValue(IMajorRecord record, ISimpleModelGetter? value)
        {
            if (record is IModeled modeledRecord)
            {
                if (value == null)
                {
                    modeledRecord.Model = null;
                }
                else
                {
                    // Deep copy
                    var newModel = new Model();
                    newModel.File = (AssetLink<SkyrimModelAssetType>)value.File;
                    newModel.Data = value.Data?.ToArray();
                    modeledRecord.Model = newModel;
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IModeled for {PropertyName}");
            }
        }

        public override ISimpleModelGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IModeledGetter modeledRecord)
            {
                return modeledRecord.Model;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IModeledGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(ISimpleModelGetter? value1, ISimpleModelGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            // Use Mutagen's built-in equality
            return value1.Equals(value2);
        }
    }
}
