using System.Collections.Generic;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogView
{
    public class BranchesHandler : AbstractListPropertyHandler<IFormLinkGetter<IDialogBranchGetter>>
    {
        public override string PropertyName => "Branches";

        public override List<IFormLinkGetter<IDialogBranchGetter>>? GetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecordGetter record)
        {
            if (record is IDialogViewGetter dialogView)
            {
                return dialogView.Branches?.ToList();
            }

            return null;
        }

        public override void SetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecord record, List<IFormLinkGetter<IDialogBranchGetter>>? value)
        {
            if (record is not IDialogView dialogView)
            {
                return;
            }

            if (dialogView.Branches == null)
            {
                return;
            }

            dialogView.Branches.Clear();
            if (value == null)
            {
                return;
            }

            foreach (var link in value)
            {
                if (link == null || link.FormKey.IsNull) continue;
                dialogView.Branches.Add(new FormLink<IDialogBranchGetter>(link.FormKey));
            }
        }
    }
}
