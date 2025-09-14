using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Light
{
    public class LensHandler : AbstractFormLinkPropertyHandler<Mutagen.Bethesda.Skyrim.Light, ILightGetter, ILensFlareGetter>
    {
        public override string PropertyName => "Lens";

        protected override IFormLinkNullableGetter<ILensFlareGetter>? GetFormLinkValue(ILightGetter record)
        {
            return record.Lens;
        }

        protected override void SetFormLinkValue(Mutagen.Bethesda.Skyrim.Light record, IFormLinkNullableGetter<ILensFlareGetter>? value)
        {
            if (value?.FormKey != null)
            {
                record.Lens.SetTo(value.FormKey);
            }
            else
            {
                record.Lens.Clear();
            }
        }
    }
}


