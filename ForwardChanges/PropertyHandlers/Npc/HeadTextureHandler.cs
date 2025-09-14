using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class HeadTextureHandler : AbstractFormLinkPropertyHandler<INpc, INpcGetter, ITextureSetGetter>
    {
        public override string PropertyName => "HeadTexture";

        protected override IFormLinkNullableGetter<ITextureSetGetter>? GetFormLinkValue(INpcGetter record)
        {
            return record.HeadTexture;
        }

        protected override void SetFormLinkValue(INpc record, IFormLinkNullableGetter<ITextureSetGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.HeadTexture = new FormLinkNullable<ITextureSetGetter>(value.FormKey);
            }
            else
            {
                record.HeadTexture.Clear();
            }
        }
    }
}