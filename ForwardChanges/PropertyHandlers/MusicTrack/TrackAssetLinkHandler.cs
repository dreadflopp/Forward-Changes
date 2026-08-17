using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.MusicTrack
{
    public class TrackAssetLinkHandler : AbstractPropertyHandler<AssetLinkGetter<SkyrimMusicAssetType>?>
    {
        private readonly string _propertyName;

        public TrackAssetLinkHandler(string propertyName)
        {
            _propertyName = propertyName;
        }

        public override string PropertyName => _propertyName;

        public override AssetLinkGetter<SkyrimMusicAssetType>? GetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecordGetter record)
        {
            return _propertyName == "TrackFilename"
                ? (record as IMusicTrackGetter)?.TrackFilename
                : (record as IMusicTrackGetter)?.FinaleFilename;
        }

        public override void SetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecord record, AssetLinkGetter<SkyrimMusicAssetType>? value)
        {
            if (record is not IMusicTrack musicTrack)
            {
                return;
            }

            var newValue = value == null || value.IsNull
                ? null
                : new AssetLink<SkyrimMusicAssetType>(value.DataRelativePath);

            if (_propertyName == "TrackFilename")
            {
                musicTrack.TrackFilename = newValue;
            }
            else
            {
                musicTrack.FinaleFilename = newValue;
            }
        }

        public override bool AreValuesEqual(AssetLinkGetter<SkyrimMusicAssetType>? value1, AssetLinkGetter<SkyrimMusicAssetType>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.DataRelativePath == value2.DataRelativePath;
        }

        public override string FormatValue(object? value)
        {
            return value is AssetLinkGetter<SkyrimMusicAssetType> assetLink
                ? assetLink.DataRelativePath.ToString()
                : value?.ToString() ?? "null";
        }
    }
}