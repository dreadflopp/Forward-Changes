using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim.Assets;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Noggog;

namespace ForwardChanges.PropertyHandlers.BasicPropertyHandlers
{
    public class ArmorAddonFirstPersonModelPropertyHandler : AbstractPropertyHandler<IGenderedItemGetter<IModelGetter?>>
    {
        public override string PropertyName => "FirstPersonModel";

        public override void SetValue(IMajorRecord record, IGenderedItemGetter<IModelGetter?>? value)
        {
            if (record is IArmorAddon armorAddonRecord)
            {
                if (value == null)
                {
                    armorAddonRecord.FirstPersonModel = null;
                }
                else
                {
                    // Create a new GenderedItem with deep copies of the models
                    var maleModel = value.Male != null ? DeepCopyModel(value.Male) : null;
                    var femaleModel = value.Female != null ? DeepCopyModel(value.Female) : null;
                    var newGenderedItem = new GenderedItem<Model?>(maleModel as Model, femaleModel as Model);
                    armorAddonRecord.FirstPersonModel = newGenderedItem;
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IArmorAddon for {PropertyName}");
            }
        }

        public override IGenderedItemGetter<IModelGetter?>? GetValue(IMajorRecordGetter record)
        {
            if (record is IArmorAddonGetter armorAddonRecord)
            {
                return armorAddonRecord.FirstPersonModel;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IArmorAddonGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IGenderedItemGetter<IModelGetter?>? value1, IGenderedItemGetter<IModelGetter?>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare male models
            bool maleEqual = AreModelsEqual(value1.Male, value2.Male);
            // Compare female models
            bool femaleEqual = AreModelsEqual(value1.Female, value2.Female);

            return maleEqual && femaleEqual;
        }

        private bool AreModelsEqual(IModelGetter? model1, IModelGetter? model2)
        {
            if (model1 == null && model2 == null) return true;
            if (model1 == null || model2 == null) return false;

            // Use Mutagen's built-in equality for models
            return model1.Equals(model2);
        }

        private IModelGetter? DeepCopyModel(IModelGetter? sourceModel)
        {
            if (sourceModel == null) return null;

            // Create a new Model with the same data
            var newModel = new Model();
            newModel.File = (AssetLink<SkyrimModelAssetType>)sourceModel.File;
            newModel.Data = sourceModel.Data?.ToArray();

            // Copy alternate textures if they exist
            if (sourceModel.AlternateTextures != null)
            {
                var alternateTextures = new ExtendedList<AlternateTexture>();
                foreach (var altTexture in sourceModel.AlternateTextures)
                {
                    var newAltTexture = new AlternateTexture();
                    newAltTexture.Name = altTexture.Name;
                    newAltTexture.NewTexture = (IFormLink<ITextureSetGetter>)altTexture.NewTexture;
                    newAltTexture.Index = altTexture.Index;
                    alternateTextures.Add(newAltTexture);
                }
                newModel.AlternateTextures = alternateTextures;
            }

            return newModel;
        }


    }
}