using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class LinkedReferencesHandler : AbstractListPropertyHandler<ILinkedReferencesGetter>
    {
        public override string PropertyName => "LinkedReferences";

        public override void SetValue(IMajorRecord record, List<ILinkedReferencesGetter>? value)
        {
            if (record is IPlacedNpc placedNpcRecord)
            {
                if (value != null)
                {
                    // Clear existing collection
                    placedNpcRecord.LinkedReferences.Clear();

                    // Add new items
                    foreach (var linkedRef in value)
                    {
                        var newLinkedRef = new LinkedReferences
                        {
                            Versioning = linkedRef.Versioning,
                            KeywordOrReference = new FormLink<IKeywordLinkedReferenceGetter>(linkedRef.KeywordOrReference.FormKey),
                            Reference = new FormLink<ILinkedReferenceGetter>(linkedRef.Reference.FormKey)
                        };
                        placedNpcRecord.LinkedReferences.Add(newLinkedRef);
                    }
                }
                else
                {
                    placedNpcRecord.LinkedReferences.Clear();
                }
            }
        }

        public override List<ILinkedReferencesGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedNpcGetter placedNpcRecord)
            {
                return placedNpcRecord.LinkedReferences?.ToList();
            }
            return null;
        }

        protected override bool IsItemEqual(ILinkedReferencesGetter? item1, ILinkedReferencesGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            return item1.Versioning == item2.Versioning &&
                   item1.KeywordOrReference.FormKey == item2.KeywordOrReference.FormKey &&
                   item1.Reference.FormKey == item2.Reference.FormKey;
        }

        protected override string FormatItem(ILinkedReferencesGetter? item)
        {
            if (item == null) return "null";
            return $"Versioning: {item.Versioning}, KeywordOrReference: {item.KeywordOrReference.FormKey}, Reference: {item.Reference.FormKey}";
        }
    }
}