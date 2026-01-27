using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Static
{
    public class LodHandler : AbstractPropertyHandler<ILodGetter?>
    {
        public override string PropertyName => "Lod";

        public override void SetValue(IMajorRecord record, ILodGetter? value)
        {
            var staticRecord = TryCastRecord<IStatic>(record, PropertyName);
            if (staticRecord != null)
            {
                if (value == null)
                {
                    staticRecord.Lod = null;
                }
                else
                {
                    // Deep copy
                    var newLod = new Lod();
                    if (!value.Level0.IsNull)
                    {
                        newLod.Level0 = new AssetLink<SkyrimModelAssetType>(value.Level0.DataRelativePath.ToString());
                    }
                    if (!value.Level1.IsNull)
                    {
                        newLod.Level1 = new AssetLink<SkyrimModelAssetType>(value.Level1.DataRelativePath.ToString());
                    }
                    if (!value.Level2.IsNull)
                    {
                        newLod.Level2 = new AssetLink<SkyrimModelAssetType>(value.Level2.DataRelativePath.ToString());
                    }
                    if (!value.Level3.IsNull)
                    {
                        newLod.Level3 = new AssetLink<SkyrimModelAssetType>(value.Level3.DataRelativePath.ToString());
                    }
                    staticRecord.Lod = newLod;
                }
            }
        }

        public override ILodGetter? GetValue(IMajorRecordGetter record)
        {
            var staticRecord = TryCastRecord<IStaticGetter>(record, PropertyName);
            if (staticRecord != null)
            {
                return staticRecord.Lod;
            }
            return null;
        }

        public override bool AreValuesEqual(ILodGetter? value1, ILodGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            return value1.Level0 == value2.Level0 &&
                   value1.Level1 == value2.Level1 &&
                   value1.Level2 == value2.Level2 &&
                   value1.Level3 == value2.Level3;
        }
    }
}
