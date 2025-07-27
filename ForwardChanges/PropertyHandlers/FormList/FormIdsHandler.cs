using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ForwardChanges.PropertyHandlers.FormList
{
    public class FormIdsHandler : AbstractListPropertyHandler<IFormLinkGetter<ISkyrimMajorRecordGetter>>
    {
        public override string PropertyName => "Items";

        public override void SetValue(IMajorRecord record, List<IFormLinkGetter<ISkyrimMajorRecordGetter>>? value)
        {
            if (record is IFormList formList)
            {
                formList.Items.Clear();
                if (value != null)
                {
                    foreach (var link in value)
                        formList.Items.Add(new FormLink<ISkyrimMajorRecordGetter>(link.FormKey));
                }
            }
            else
            {
                Console.WriteLine($"[{PropertyName}] Record is not IFormList, actual type: {record.GetType().Name}");
            }
        }

        public override List<IFormLinkGetter<ISkyrimMajorRecordGetter>>? GetValue(IMajorRecordGetter record)
        {
            if (record is IFormListGetter formList)
            {
                return formList.Items?.ToList();
            }
            return null;
        }

        protected override bool IsItemEqual(IFormLinkGetter<ISkyrimMajorRecordGetter>? item1, IFormLinkGetter<ISkyrimMajorRecordGetter>? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;
            return item1.FormKey.Equals(item2.FormKey);
        }

        protected override string FormatItem(IFormLinkGetter<ISkyrimMajorRecordGetter>? item)
        {
            return item?.FormKey.ToString() ?? "null";
        }
    }
}

