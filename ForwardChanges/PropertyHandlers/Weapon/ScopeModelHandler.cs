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
            // Compare File - use DataRelativePath for value-based comparison (avoids reference equality from different overlays)
            if (model1.File.DataRelativePath != model2.File.DataRelativePath) return false;

            var alt1Count = model1.AlternateTextures?.Count ?? 0;
            var alt2Count = model2.AlternateTextures?.Count ?? 0;
            if (alt1Count != alt2Count) return false;

            if (alt1Count > 0 && model1.AlternateTextures != null && model2.AlternateTextures != null)
            {
                for (int i = 0; i < alt1Count; i++)
                {
                    var alt1 = model1.AlternateTextures[i];
                    var alt2 = model2.AlternateTextures[i];
                    if (alt1?.Name != alt2?.Name || alt1?.NewTexture?.FormKey != alt2?.NewTexture?.FormKey) return false;
                }
            }

            return true;
        }

        private Model? DeepCopyModel(IModelGetter sourceModel)
        {
            if (sourceModel == null) return null;

            // Create a new Model with the same data
            var newModel = new Model();
            newModel.File = sourceModel.File.IsNull
                ? new AssetLink<SkyrimModelAssetType>()
                : new AssetLink<SkyrimModelAssetType>(sourceModel.File.DataRelativePath.ToString());
            newModel.Data = sourceModel.Data?.ToArray();

            // Copy alternate textures if they exist
            if (sourceModel.AlternateTextures != null)
            {
                var alternateTextures = new ExtendedList<AlternateTexture>();
                foreach (var altTexture in sourceModel.AlternateTextures)
                {
                    var newAltTexture = new AlternateTexture();
                    newAltTexture.Name = altTexture.Name;
                    newAltTexture.NewTexture = new FormLink<ITextureSetGetter>(altTexture.NewTexture.FormKey);
                    newAltTexture.Index = altTexture.Index;
                    alternateTextures.Add(newAltTexture);
                }
                newModel.AlternateTextures = alternateTextures;
            }

            return newModel;
        }
    }
}