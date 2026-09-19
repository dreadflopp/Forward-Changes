using System;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;

namespace DreadsMashedPatch.PropertyHandlers.Abstracts
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
                    newIcons.LargeIconFilename = AssetPathHelper.Copy(value.LargeIconFilename)!;
                }

                // Copy SmallIconFilename
                if (value.SmallIconFilename != null && !value.SmallIconFilename.IsNull)
                {
                    newIcons.SmallIconFilename = AssetPathHelper.Copy(value.SmallIconFilename);
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
            if (!AssetPathHelper.AreEqual(value1.LargeIconFilename, value2.LargeIconFilename)) return false;

            // Compare SmallIconFilename
            if (!AssetPathHelper.AreEqual(value1.SmallIconFilename, value2.SmallIconFilename)) return false;

            return true;
        }

        protected abstract IIconsGetter? GetIcons(TRecordGetter record);
        protected abstract void SetIcons(TRecord record, Icons? value);
    }
}
