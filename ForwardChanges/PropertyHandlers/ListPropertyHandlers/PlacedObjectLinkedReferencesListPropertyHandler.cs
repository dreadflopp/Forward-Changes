using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.ListPropertyHandlers
{
    public class PlacedObjectLinkedReferencesListPropertyHandler : AbstractListPropertyHandler<ILinkedReferencesGetter>
    {
        public override string PropertyName => "LinkedReferences";

        public override void SetValue(IMajorRecord record, List<ILinkedReferencesGetter>? value)
        {
            if (record is IPlacedObject placedObject)
            {
                placedObject.LinkedReferences.Clear();
                if (value != null)
                {
                    foreach (var getter in value)
                    {
                        var linkedRef = new LinkedReferences
                        {
                            KeywordOrReference = getter.KeywordOrReference != null ? (IFormLink<IKeywordLinkedReferenceGetter>)new FormLink<IKeywordLinkedReferenceGetter>(getter.KeywordOrReference.FormKey) : (IFormLink<IKeywordLinkedReferenceGetter>)FormLink<IKeywordLinkedReferenceGetter>.Null,
                            Reference = getter.Reference != null ? (IFormLink<ILinkedReferenceGetter>)new FormLink<ILinkedReferenceGetter>(getter.Reference.FormKey) : (IFormLink<ILinkedReferenceGetter>)FormLink<ILinkedReferenceGetter>.Null
                        };
                        placedObject.LinkedReferences.Add(linkedRef);
                    }
                }
            }
        }

        public override List<ILinkedReferencesGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObject)
            {
                return placedObject.LinkedReferences?.ToList();
            }
            return null;
        }

        protected override bool IsItemEqual(ILinkedReferencesGetter? item1, ILinkedReferencesGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Compare the two key properties that make up a linked reference
            return item1.KeywordOrReference.Equals(item2.KeywordOrReference) &&
                   item1.Reference.Equals(item2.Reference);
        }

        protected override string FormatItem(ILinkedReferencesGetter? item)
        {
            if (item == null) return "null";
            return $"KeywordOrReference: {item.KeywordOrReference}, Reference: {item.Reference}";
        }
    }
}