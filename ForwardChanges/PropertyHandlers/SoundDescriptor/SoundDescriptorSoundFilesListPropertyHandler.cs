using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim.Assets;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;
using System.Linq;

namespace ForwardChanges.PropertyHandlers.SoundDescriptor
{
    public class SoundDescriptorSoundFilesListPropertyHandler : AbstractListPropertyHandler<IAssetLinkGetter<SkyrimSoundAssetType>>
    {
        public override string PropertyName => "SoundFiles";

        public override void SetValue(IMajorRecord record, List<IAssetLinkGetter<SkyrimSoundAssetType>>? value)
        {
            if (record is ISoundDescriptor soundDescriptor)
            {
                soundDescriptor.SoundFiles.Clear();
                if (value != null)
                {
                    foreach (var assetLink in value)
                    {
                        var assetPath = assetLink?.ToString();
                        if (!string.IsNullOrEmpty(assetPath))
                        {
                            var newAssetLink = new AssetLink<SkyrimSoundAssetType>(assetPath);
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
            return string.Equals(item1.ToString(), item2.ToString(), System.StringComparison.OrdinalIgnoreCase);
        }

        protected override string FormatItem(IAssetLinkGetter<SkyrimSoundAssetType>? item)
        {
            return item?.ToString() ?? "null";
        }
    }
}