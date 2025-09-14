using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Noggog;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class CloudModelHandler : AbstractPropertyHandler<IModelGetter?>
    {
        public override string PropertyName => "CloudModel";

        public override void SetValue(IMajorRecord record, IModelGetter? value)
        {
            var worldspaceRecord = TryCastRecord<IWorldspace>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                worldspaceRecord.CloudModel = value != null ? DeepCopyModel(value) : null;
            }
        }

        public override IModelGetter? GetValue(IMajorRecordGetter record)
        {
            var worldspaceRecord = TryCastRecord<IWorldspaceGetter>(record, PropertyName);
            if (worldspaceRecord != null)
            {
                return worldspaceRecord.CloudModel;
            }
            return null;
        }

        public override bool AreValuesEqual(IModelGetter? value1, IModelGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare File
            if (value1.File.ToString() != value2.File.ToString()) return false;

            // Compare AlternateTextures
            if (value1.AlternateTextures?.Count != value2.AlternateTextures?.Count) return false;
            if (value1.AlternateTextures != null && value2.AlternateTextures != null)
            {
                for (int i = 0; i < value1.AlternateTextures.Count; i++)
                {
                    var alt1 = value1.AlternateTextures[i];
                    var alt2 = value2.AlternateTextures[i];
                    if (alt1.Name != alt2.Name || alt1.NewTexture.ToString() != alt2.NewTexture.ToString()) return false;
                }
            }

            return true;
        }

        private Model DeepCopyModel(IModelGetter value)
        {
            var newModel = new Model();

            // Copy File
            if (value.File != null)
            {
                newModel.File = new AssetLink<SkyrimModelAssetType>(value.File.ToString());
            }

            // Copy AlternateTextures
            if (value.AlternateTextures != null)
            {
                foreach (var altTexture in value.AlternateTextures)
                {
                    if (altTexture != null)
                    {
                        var newAltTexture = new AlternateTexture();
                        newAltTexture.Name = altTexture.Name ?? string.Empty;
                        if (altTexture.NewTexture != null)
                        {
                            var newTexture = altTexture.NewTexture;
                            if (newTexture != null && !newTexture.FormKey.IsNull)
                            {
                                newAltTexture.NewTexture = new FormLink<ITextureSetGetter>(newTexture.FormKey);
                            }
                        }
                        if (newModel.AlternateTextures != null)
                        {
                            newModel.AlternateTextures.Add(newAltTexture);
                        }
                    }
                }
            }

            return newModel;
        }

        public override string FormatValue(object? value)
        {
            if (value is not IModelGetter model)
            {
                return value?.ToString() ?? "null";
            }

            var alternateTextures = model.AlternateTextures?.Count > 0 
                ? $", {model.AlternateTextures.Count} alternate textures" 
                : "";
            
            return $"File: {model.File}{alternateTextures}";
        }
    }
}