using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Noggog;

namespace ForwardChanges.PropertyHandlers.General
{
    /// <summary>
    /// Handler for full Model structures (File, Data, and AlternateTextures).
    /// Used by IModeled records: Activator, Book, Container, Weapon, MiscItem, Ingredient, Ingestible.
    /// </summary>
    public class ModelHandler : AbstractPropertyHandler<IModelGetter?>
    {
        public override string PropertyName => "Model";

        public override void SetValue(IMajorRecord record, IModelGetter? value)
        {
            if (record is IModeled modeledRecord)
            {
                if (value == null)
                {
                    modeledRecord.Model = null;
                }
                else
                {
                    // Deep copy full model (File, Data, AlternateTextures)
                    var newModel = new Model();
                    newModel.File = value.File.IsNull
                        ? new AssetLink<SkyrimModelAssetType>()
                        : AssetPathHelper.Copy(value.File)!;
                    newModel.Data = value.Data?.ToArray();

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

                    modeledRecord.Model = newModel;
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IModeled for {PropertyName}");
            }
        }

        public override IModelGetter? GetValue(IMajorRecordGetter record)
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

        public override bool AreValuesEqual(IModelGetter? value1, IModelGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            if (!AssetPathHelper.AreEqual(value1.File, value2.File))
            {
                return false;
            }

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
                    if (alt1?.Name != alt2?.Name
                        || alt1?.NewTexture?.FormKey != alt2?.NewTexture?.FormKey
                        || alt1?.Index != alt2?.Index)
                    {
                        return false;
                    }
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

        public override string FormatValue(object? value)
        {
            if (value is IModelGetter model)
            {
                var altTextureCount = model.AlternateTextures?.Count ?? 0;
                return $"Model(File: {AssetPathHelper.Format(model.File)}, AltTextures: {altTextureCount})";
            }
            return value?.ToString() ?? "null";
        }
    }
}
