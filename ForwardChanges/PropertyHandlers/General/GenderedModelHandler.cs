using System;
using System.Reflection;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using ForwardChanges.PropertyHandlers.Abstracts;
using Noggog;

namespace ForwardChanges.PropertyHandlers.General
{
    /// <summary>
    /// General handler for gendered model properties (FirstPersonModel, WorldModel) on IArmorAddon.
    /// Uses reflection to access the property dynamically based on the property name.
    /// </summary>
    public class GenderedModelHandler : AbstractPropertyHandler<IGenderedItemGetter<IModelGetter?>>
    {
        /// <summary>
        /// Model paths in the plugin are stored relative to the Meshes folder (e.g. "DLC02\Armor\...").
        /// Mutagen's DataRelativePath is relative to the Data folder (e.g. "Meshes\DLC02\..."). Strip the prefix for comparison, display, and writing.
        /// </summary>
        private static string GetModelPathNormalized(IAssetLinkGetter<SkyrimModelAssetType> file)
        {
            var path = file.DataRelativePath.ToString();
            if (string.IsNullOrEmpty(path)) return path;
            const string meshesPrefix = "Meshes\\";
            if (path.StartsWith(meshesPrefix, StringComparison.OrdinalIgnoreCase))
                return path.Substring(meshesPrefix.Length);
            return path;
        }
        private readonly string _propertyName;
        private readonly PropertyInfo? _getterProperty;
        private readonly PropertyInfo? _setterProperty;

        public GenderedModelHandler(string propertyName)
        {
            _propertyName = propertyName;

            _getterProperty = typeof(IArmorAddonGetter).GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            _setterProperty = typeof(IArmorAddon).GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            if (_getterProperty == null)
            {
                throw new ArgumentException(
                    $"Property '{propertyName}' not found on IArmorAddonGetter");
            }
        }

        public override string PropertyName => _propertyName;

        public override void SetValue(IMajorRecord record, IGenderedItemGetter<IModelGetter?>? value)
        {
            if (record is not IArmorAddon armorAddonRecord)
            {
                Console.WriteLine($"Error: Record does not implement IArmorAddon for {PropertyName}");
                return;
            }

            if (_setterProperty == null)
            {
                Console.WriteLine($"Error: Property '{PropertyName}' is read-only or not found on IArmorAddon");
                return;
            }

            if (value == null)
            {
                _setterProperty.SetValue(armorAddonRecord, null);
            }
            else
            {
                var maleModel = value.Male != null ? DeepCopyModel(value.Male) : null;
                var femaleModel = value.Female != null ? DeepCopyModel(value.Female) : null;
                var newGenderedItem = new GenderedItem<Model?>(maleModel as Model, femaleModel as Model);
                _setterProperty.SetValue(armorAddonRecord, newGenderedItem);
            }
        }

        public override IGenderedItemGetter<IModelGetter?>? GetValue(IMajorRecordGetter record)
        {
            if (record is not IArmorAddonGetter armorAddonRecord)
            {
                Console.WriteLine($"Error: Record does not implement IArmorAddonGetter for {PropertyName}");
                return null;
            }

            if (_getterProperty == null)
            {
                return null;
            }

            try
            {
                return _getterProperty.GetValue(armorAddonRecord) as IGenderedItemGetter<IModelGetter?>;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting property '{PropertyName}' via reflection: {ex.Message}");
                return null;
            }
        }

        public override bool AreValuesEqual(IGenderedItemGetter<IModelGetter?>? value1, IGenderedItemGetter<IModelGetter?>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            bool maleEqual = AreModelsEqual(value1.Male, value2.Male);
            bool femaleEqual = AreModelsEqual(value1.Female, value2.Female);

            return maleEqual && femaleEqual;
        }

        private static bool AreModelsEqual(IModelGetter? model1, IModelGetter? model2)
        {
            if (model1 == null && model2 == null) return true;
            if (model1 == null || model2 == null) return false;

            if (GetModelPathNormalized(model1.File) != GetModelPathNormalized(model2.File)) return false;

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

        private static IModelGetter? DeepCopyModel(IModelGetter? sourceModel)
        {
            if (sourceModel == null) return null;

            var newModel = new Model();
            newModel.File = new AssetLink<SkyrimModelAssetType>(GetModelPathNormalized(sourceModel.File));
            newModel.Data = sourceModel.Data?.ToArray();

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

        public override string FormatValue(object? value)
        {
            if (value is IGenderedItemGetter<IModelGetter?> gendered)
            {
                var maleStr = gendered.Male != null ? $"File: {GetModelPathNormalized(gendered.Male.File)}" : "null";
                var femaleStr = gendered.Female != null ? $"File: {GetModelPathNormalized(gendered.Female.File)}" : "null";
                return $"Male: {maleStr}, Female: {femaleStr}";
            }
            return value?.ToString() ?? "null";
        }
    }
}
