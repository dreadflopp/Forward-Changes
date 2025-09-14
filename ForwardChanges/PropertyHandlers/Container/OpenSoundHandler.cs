using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Container
{
    public class OpenSoundHandler : AbstractFormLinkPropertyHandler<IContainer, IContainerGetter, ISoundDescriptorGetter>
    {
        public override string PropertyName => "OpenSound";

        protected override IFormLinkNullableGetter<ISoundDescriptorGetter> GetFormLinkValue(IContainerGetter record)
        {
            return record.OpenSound;
        }

        protected override void SetFormLinkValue(IContainer record, IFormLinkNullableGetter<ISoundDescriptorGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.OpenSound = new FormLinkNullable<ISoundDescriptorGetter>(value.FormKey);
            }
            else
            {
                record.OpenSound.Clear();
            }
        }
    }
}