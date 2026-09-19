using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;

namespace ForwardChanges.PropertyHandlers.SoundDescriptor
{
    public class SoundFilesHandler : AbstractListPropertyHandler<IAssetLinkGetter<SkyrimSoundAssetType>>
    {
        public override string PropertyName => "SoundFiles";
        // xEdit defines these repeated ANAM strings as an indexed, non-sorted wbRArray.
        // Each numbered slot therefore has positional identity.
        public override ListSemantics Semantics => ListSemantics.ExactOrdered;

        public override void SetValue(IMajorRecord record, List<IAssetLinkGetter<SkyrimSoundAssetType>>? value)
        {
            if (record is ISoundDescriptor soundDescriptor)
            {
                soundDescriptor.SoundFiles.Clear();
                if (value != null)
                {
                    foreach (var assetLink in value)
                    {
                        if (assetLink != null)
                        {
                            // Mutagen writes GivenPath to ANAM. DataRelativePath is only a normalized
                            // lookup path and strips Data\ while adding the Sound\ asset base folder.
                            var newAssetLink = AssetPathHelper.Copy(assetLink)!;
                            soundDescriptor.SoundFiles.Add(newAssetLink);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"[{PropertyName}] Record is not ISoundDescriptor, actual type: {record.GetType().Name}");
            }
        }

        public override List<IAssetLinkGetter<SkyrimSoundAssetType>>? GetValue(IMajorRecordGetter record)
        {
            if (record is ISoundDescriptorGetter soundDescriptor)
            {
                return soundDescriptor.SoundFiles?.ToList();
            }
            return null;
        }

        protected override bool IsItemEqual(IAssetLinkGetter<SkyrimSoundAssetType>? item1, IAssetLinkGetter<SkyrimSoundAssetType>? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;
            return AssetPathHelper.AreEqual(item1, item2);
        }

        protected override string FormatItem(IAssetLinkGetter<SkyrimSoundAssetType>? item)
        {
            if (item == null) return "null";

            return AssetPathHelper.Format(item);
        }
    }
}

