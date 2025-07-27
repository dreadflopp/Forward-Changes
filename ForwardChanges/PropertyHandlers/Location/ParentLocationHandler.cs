using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Location
{
    public class ParentLocationHandler : AbstractPropertyHandler<IFormLinkNullable<ILocationGetter>>
    {
        public override string PropertyName => "ParentLocation";

        public override IFormLinkNullable<ILocationGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ILocationGetter locationRecord)
            {
                return locationRecord.ParentLocation as IFormLinkNullable<ILocationGetter>;
            }

            Console.WriteLine($"Error: Record does not implement ILocationGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, IFormLinkNullable<ILocationGetter>? value)
        {
            if (record is ILocation locationRecord)
            {
                locationRecord.ParentLocation = value ?? new FormLinkNullable<ILocationGetter>();
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ILocation for {PropertyName}");
            }
        }
    }
}

