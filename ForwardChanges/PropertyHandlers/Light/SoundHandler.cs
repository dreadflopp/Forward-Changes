using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Light
{
    public class SoundHandler : AbstractFormLinkPropertyHandler<Mutagen.Bethesda.Skyrim.Light, ILightGetter, ISoundDescriptorGetter>
    {
        public override string PropertyName => "Sound";

        protected override IFormLinkNullableGetter<ISoundDescriptorGetter>? GetFormLinkValue(ILightGetter record)
        {
            return record.Sound;
        }

        protected override void SetFormLinkValue(Mutagen.Bethesda.Skyrim.Light record, IFormLinkNullableGetter<ISoundDescriptorGetter>? value)
        {
            if (value?.FormKey != null)
            {
                record.Sound.SetTo(value.FormKey);
            }
            else
            {
                record.Sound.Clear();
            }
        }
    }
}


