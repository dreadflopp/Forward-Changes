using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Worldspace
{
    public class WorldspaceMusicPropertyHandler : AbstractPropertyHandler<IFormLinkNullableGetter<IMusicTypeGetter>>
    {
        public override string PropertyName => "Music";

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<IMusicTypeGetter>? value)
        {
            if (record is IWorldspace worldspaceRecord)
            {
                if (value != null)
                {
                    worldspaceRecord.Music.SetTo(value.FormKey);
                }
                else
                {
                    worldspaceRecord.Music.SetTo(null);
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IWorldspace for {PropertyName}");
            }
        }

        public override IFormLinkNullableGetter<IMusicTypeGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IWorldspaceGetter worldspaceRecord)
            {
                return worldspaceRecord.Music;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IWorldspaceGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<IMusicTypeGetter>? value1, IFormLinkNullableGetter<IMusicTypeGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            // Compare FormKeys
            return value1.FormKey == value2.FormKey;
        }
    }
}