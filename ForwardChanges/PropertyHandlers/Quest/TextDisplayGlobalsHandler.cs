using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Quest
{
    public class TextDisplayGlobalsHandler : AbstractListPropertyHandler<IFormLinkGetter<IGlobalGetter>>
    {
        public override string PropertyName => "TextDisplayGlobals";

        public override List<IFormLinkGetter<IGlobalGetter>>? GetValue(IMajorRecordGetter record)
        {
            if (record is IQuestGetter questRecord)
            {
                return questRecord.TextDisplayGlobals?.ToList();
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, List<IFormLinkGetter<IGlobalGetter>>? value)
        {
            if (record is IQuest questRecord && value != null)
            {
                if (questRecord.TextDisplayGlobals != null)
                {
                    questRecord.TextDisplayGlobals.Clear();
                    foreach (var global in value)
                    {
                        if (global != null)
                        {
                            questRecord.TextDisplayGlobals.Add(new FormLink<IGlobalGetter>(global.FormKey));
                        }
                    }
                }
            }
        }

        public override bool AreValuesEqual(List<IFormLinkGetter<IGlobalGetter>>? value1, List<IFormLinkGetter<IGlobalGetter>>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1.Count != value2.Count) return false;

            for (int i = 0; i < value1.Count; i++)
            {
                if (!IsItemEqual(value1[i], value2[i])) return false;
            }
            return true;
        }

        protected override bool IsItemEqual(IFormLinkGetter<IGlobalGetter>? item1, IFormLinkGetter<IGlobalGetter>? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;
            return item1.FormKey.Equals(item2.FormKey);
        }

        protected override string FormatItem(IFormLinkGetter<IGlobalGetter>? item)
        {
            return item?.FormKey.ToString() ?? "null";
        }
    }
}
