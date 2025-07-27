using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Location
{
    public class UnreportedCrimeFactionHandler : AbstractPropertyHandler<IFormLinkNullable<IFactionGetter>>
    {
        public override string PropertyName => "UnreportedCrimeFaction";

        public override IFormLinkNullable<IFactionGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is ILocationGetter locationRecord)
            {
                return locationRecord.UnreportedCrimeFaction as IFormLinkNullable<IFactionGetter>;
            }

            Console.WriteLine($"Error: Record does not implement ILocationGetter for {PropertyName}");
            return null;
        }

        public override void SetValue(IMajorRecord record, IFormLinkNullable<IFactionGetter>? value)
        {
            if (record is ILocation locationRecord)
            {
                locationRecord.UnreportedCrimeFaction = value ?? new FormLinkNullable<IFactionGetter>();
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement ILocation for {PropertyName}");
            }
        }
    }
}

