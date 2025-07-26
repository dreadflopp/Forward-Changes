using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.ListPropertyHandlers
{
    public class DialogResponseLinkToListPropertyHandler : AbstractListPropertyHandler<IFormLinkGetter<IDialogGetter>>
    {
        public override string PropertyName => "LinkTo";

        public override void SetValue(IMajorRecord record, List<IFormLinkGetter<IDialogGetter>>? value)
        {
            if (record is IDialogResponses dialogResponses)
            {
                if (dialogResponses.LinkTo == null)
                    return;
                dialogResponses.LinkTo.Clear();
                if (value != null)
                {
                    // Convert IFormLinkGetter<IDialogGetter> to IFormLink<IDialogGetter>
                    var links = value.Select(link => new FormLink<IDialogGetter>(link.FormKey)).ToList();
                    foreach (var link in links)
                        dialogResponses.LinkTo.Add(link);
                }
            }
        }

        public override List<IFormLinkGetter<IDialogGetter>>? GetValue(IMajorRecordGetter record)
        {
            if (record is IDialogResponsesGetter dialogResponses)
            {
                return dialogResponses.LinkTo?.ToList();
            }
            return null;
        }

        protected override bool IsItemEqual(IFormLinkGetter<IDialogGetter>? item1, IFormLinkGetter<IDialogGetter>? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Compare by FormKey since form links are identified by their FormKey
            return item1.FormKey.Equals(item2.FormKey);
        }

        protected override string FormatItem(IFormLinkGetter<IDialogGetter>? item)
        {
            return item?.FormKey.ToString() ?? "null";
        }
    }
}