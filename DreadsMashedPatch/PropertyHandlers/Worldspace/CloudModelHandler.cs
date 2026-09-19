using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.PropertyHandlers.General;
using Noggog;

namespace DreadsMashedPatch.PropertyHandlers.Worldspace
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

            if (!AssetPathHelper.AreEqual(value1.File, value2.File)) return false;

            if (!AreDataEqual(value1, value2)) return false;

            // Compare AlternateTextures - treat null and empty as equivalent
            var alt1Count = value1.AlternateTextures?.Count ?? 0;
            var alt2Count = value2.AlternateTextures?.Count ?? 0;
            if (alt1Count != alt2Count) return false;

            if (alt1Count > 0 && value1.AlternateTextures != null && value2.AlternateTextures != null)
            {
                for (int i = 0; i < alt1Count; i++)
                {
                    var alt1 = value1.AlternateTextures[i];
                    var alt2 = value2.AlternateTextures[i];
                    if (alt1.Name != alt2.Name
                        || alt1.NewTexture?.FormKey != alt2.NewTexture?.FormKey
                        || alt1.Index != alt2.Index) return false;
                }
            }

            return true;
        }

        private static bool AreDataEqual(IModelGetter value1, IModelGetter value2)
        {
            if (value1.Data == null || value2.Data == null)
            {
                return value1.Data == null && value2.Data == null;
            }

            return value1.Data.Value.Span.SequenceEqual(value2.Data.Value.Span);
        }

        private Model DeepCopyModel(IModelGetter value)
        {
            var newModel = new Model();

            // Copy File
            if (value.File != null)
            {
                newModel.File = AssetPathHelper.Copy(value.File)!;
            }

            newModel.Data = value.Data?.ToArray();

            // Copy AlternateTextures
            if (value.AlternateTextures != null)
            {
                newModel.AlternateTextures = new ExtendedList<AlternateTexture>();
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
                        newAltTexture.Index = altTexture.Index;
                        newModel.AlternateTextures.Add(newAltTexture);
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
            
            return $"File: {AssetPathHelper.Format(model.File)}{alternateTextures}";
        }
    }
}
