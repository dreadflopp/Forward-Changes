using System;
using System.Collections.Generic;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Static
{
    public class ModelHandler : AbstractPropertyHandler<IModelGetter?>
    {
        public override string PropertyName => "Model";

        public override IModelGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IStaticGetter staticRecord)
            {
                return staticRecord.Model;
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, IModelGetter? value)
        {
            if (record is IStatic staticRecord)
            {
                if (value == null)
                {
                    staticRecord.Model = null;
                    return;
                }

                // Create a new Model and copy the values
                var newModel = new Model
                {
                    File = new AssetLink<SkyrimModelAssetType>(value.File.DataRelativePath.ToString())
                };

                // Copy AlternateTextures if they exist
                if (value.AlternateTextures != null)
                {
                    newModel.AlternateTextures = new ExtendedList<AlternateTexture>();
                    foreach (var altTexture in value.AlternateTextures)
                    {
                        var newAltTexture = new AlternateTexture
                        {
                            Name = altTexture.Name,
                            NewTexture = new FormLink<ITextureSetGetter>(altTexture.NewTexture.FormKey),
                            Index = altTexture.Index
                        };
                        newModel.AlternateTextures.Add(newAltTexture);
                    }
                }

                staticRecord.Model = newModel;
            }
        }

        public override bool AreValuesEqual(IModelGetter? value1, IModelGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare File
            if (value1.File != value2.File) return false;

            // Compare AlternateTextures
            if (value1.AlternateTextures == null && value2.AlternateTextures == null) return true;
            if (value1.AlternateTextures == null || value2.AlternateTextures == null) return false;
            if (value1.AlternateTextures.Count != value2.AlternateTextures.Count) return false;

            for (int i = 0; i < value1.AlternateTextures.Count; i++)
            {
                var alt1 = value1.AlternateTextures[i];
                var alt2 = value2.AlternateTextures[i];
                if (alt1.Name != alt2.Name || alt1.NewTexture != alt2.NewTexture || alt1.Index != alt2.Index)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
