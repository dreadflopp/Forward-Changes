using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using ForwardChanges.PropertyHandlers.Abstracts;
using Noggog;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class ScopeModelHandler : AbstractPropertyHandler<IModelGetter?>
    {
        public override string PropertyName => "ScopeModel";

        public override void SetValue(IMajorRecord record, IModelGetter? value)
        {
            var weaponRecord = TryCastRecord<IWeapon>(record, PropertyName);
            if (weaponRecord != null)
            {
                weaponRecord.ScopeModel = value != null ? DeepCopyModel(value) : null;
            }
        }

        public override IModelGetter? GetValue(IMajorRecordGetter record)
        {
            var weaponRecord = TryCastRecord<IWeaponGetter>(record, PropertyName);
            if (weaponRecord != null)
            {
                return weaponRecord.ScopeModel;
            }
            return null;
        }

        public override bool AreValuesEqual(IModelGetter? value1, IModelGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare model properties using value-based comparison
            return AreModelsEqual(value1, value2);
        }

        private bool AreModelsEqual(IModelGetter model1, IModelGetter model2)
        {
            // Compare basic model properties (from ISimpleModelGetter)
            if (model1.File != model2.File) return false;
            if (model1.AlternateTextures?.Count != model2.AlternateTextures?.Count) return false;

            // Compare alternate textures if they exist
            if (model1.AlternateTextures != null && model2.AlternateTextures != null)
            {
                for (int i = 0; i < model1.AlternateTextures.Count; i++)
                {
                    var alt1 = model1.AlternateTextures[i];
                    var alt2 = model2.AlternateTextures[i];
                    if (alt1?.Name != alt2?.Name || alt1?.NewTexture != alt2?.NewTexture) return false;
                }
            }

            return true;
        }

        private Model? DeepCopyModel(IModelGetter sourceModel)
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