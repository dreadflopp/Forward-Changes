using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Location
{
    public class LocationMusicPropertyHandler : AbstractPropertyHandler<IFormLinkNullable<IMusicTypeGetter>>
    {
        public override string PropertyName => "Music";

        public override IFormLinkNullable<IMusicTypeGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ILocationGetter locationRecord)
            {
                return locationRecord.Music as IFormLinkNullable<IMusicTypeGetter>;
            }

            Console.WriteLine($"Error: Record does not implement ILocationGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, IFormLinkNullable<IMusicTypeGetter>? value)
        {
            if (record is ILocation locationRecord)
            {
                locationRecord.Music = value ?? new FormLinkNullable<IMusicTypeGetter>();
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ILocation for {PropertyName}");
            }
        }
    }
}