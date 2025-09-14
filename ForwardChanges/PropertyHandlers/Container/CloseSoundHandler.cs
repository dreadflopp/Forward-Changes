using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.Container
{
    public class CloseSoundHandler : AbstractFormLinkPropertyHandler<IContainer, IContainerGetter, ISoundDescriptorGetter>
    {
        public override string PropertyName => "CloseSound";

        protected override IFormLinkNullableGetter<ISoundDescriptorGetter> GetFormLinkValue(IContainerGetter record)
        {
            return record.CloseSound;
        }

        protected override void SetFormLinkValue(IContainer record, IFormLinkNullableGetter<ISoundDescriptorGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.CloseSound = new FormLinkNullable<ISoundDescriptorGetter>(value.FormKey);
            }
            else
            {
                record.CloseSound.Clear();
            }
        }
    }
}