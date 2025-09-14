using System;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Abstracts
{
    public abstract class AbstractIconsHandler<TRecordGetter, TRecord> : AbstractPropertyHandler<IIconsGetter?>
        where TRecordGetter : class, IMajorRecordGetter
        where TRecord : class, IMajorRecord
    {
        public override string PropertyName => "Icons";

        public override IIconsGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is TRecordGetter typedRecord)
            {
                return GetIcons(typedRecord);
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement {typeof(TRecordGetter).Name} for {PropertyName}");
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, IIconsGetter? value)
        {
            if (record is TRecord typedRecord)
            {
                if (value == null)
                {
                    SetIcons(typedRecord, null);
                    return;
                }

                // Create a new Icons instance and copy properties
                var newIcons = new Icons();

                // Copy LargeIconFilename
                if (value.LargeIconFilename != null && !value.LargeIconFilename.IsNull)
                {
                    newIcons.LargeIconFilename = new AssetLink<SkyrimTextureAssetType>(value.LargeIconFilename.ToString());
                }

                // Copy SmallIconFilename
                if (value.SmallIconFilename != null && !value.SmallIconFilename.IsNull)
                {
                    newIcons.SmallIconFilename = new AssetLink<SkyrimTextureAssetType>(value.SmallIconFilename.ToString());
                }

                SetIcons(typedRecord, newIcons);
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement {typeof(TRecord).Name} for {PropertyName}");
            }
        }

        public override bool AreValuesEqual(IIconsGetter? value1, IIconsGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare LargeIconFilename
            if (value1.LargeIconFilename?.ToString() != value2.LargeIconFilename?.ToString()) return false;

            // Compare SmallIconFilename
            if (value1.SmallIconFilename?.ToString() != value2.SmallIconFilename?.ToString()) return false;

            return true;
        }

        protected abstract IIconsGetter? GetIcons(TRecordGetter record);
        protected abstract void SetIcons(TRecord record, Icons? value);
    }
}