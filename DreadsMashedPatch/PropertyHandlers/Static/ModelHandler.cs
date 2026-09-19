using System;
using System.Collections.Generic;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;

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
                    File = AssetPathHelper.Copy(value.File)!,
                    Data = value.Data?.ToArray()
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
                    if (alt1.Name != alt2.Name || alt1.NewTexture?.FormKey != alt2.NewTexture?.FormKey || alt1.Index != alt2.Index)
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
    }
}

