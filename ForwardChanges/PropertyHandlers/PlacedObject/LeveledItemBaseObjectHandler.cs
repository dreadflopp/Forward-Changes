using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class LeveledItemBaseObjectHandler : AbstractFormLinkPropertyHandler<IPlacedObject, IPlacedObjectGetter, ILeveledItemGetter>
    {
        public override string PropertyName => "LeveledItemBaseObject";

        protected override IFormLinkNullableGetter<ILeveledItemGetter> GetFormLinkValue(IPlacedObjectGetter record)
        {
            return record.LeveledItemBaseObject;
        }

        protected override void SetFormLinkValue(IPlacedObject record, IFormLinkNullableGetter<ILeveledItemGetter>? value)
        {
            if (value != null && !value.FormKey.IsNull)
            {
                record.LeveledItemBaseObject = new FormLinkNullable<ILeveledItemGetter>(value.FormKey);
            }
            else
            {
                record.LeveledItemBaseObject.Clear();
            }
        }
    }
}