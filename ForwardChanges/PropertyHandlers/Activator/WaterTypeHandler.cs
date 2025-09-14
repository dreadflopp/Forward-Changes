using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Activator
{
    public class WaterTypeHandler : AbstractFormLinkPropertyHandler<IActivator, IActivatorGetter, IWaterGetter>
    {
        public override string PropertyName => "WaterType";

        protected override IFormLinkNullableGetter<IWaterGetter>? GetFormLinkValue(IActivatorGetter record)
        {
            return record.WaterType;
        }

        protected override void SetFormLinkValue(IActivator record, IFormLinkNullableGetter<IWaterGetter>? value)
        {
            if (value != null)
            {
                record.WaterType.SetTo(value.FormKey);
            }
            else
            {
                record.WaterType.Clear();
            }
        }
    }
}
