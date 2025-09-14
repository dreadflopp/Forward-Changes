using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Activator
{
    public class InteractionKeywordHandler : AbstractFormLinkPropertyHandler<IActivator, IActivatorGetter, IKeywordGetter>
    {
        public override string PropertyName => "InteractionKeyword";

        protected override IFormLinkNullableGetter<IKeywordGetter>? GetFormLinkValue(IActivatorGetter record)
        {
            return record.InteractionKeyword;
        }

        protected override void SetFormLinkValue(IActivator record, IFormLinkNullableGetter<IKeywordGetter>? value)
        {
            if (value != null)
            {
                record.InteractionKeyword.SetTo(value.FormKey);
            }
            else
            {
                record.InteractionKeyword.Clear();
            }
        }
    }
}
