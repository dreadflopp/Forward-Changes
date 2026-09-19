using System.Collections.Generic;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using DreadsMashedPatch.PropertyHandlers.Abstracts;

namespace DreadsMashedPatch.PropertyHandlers.CollisionLayer
{
    public class CollidesWithHandler : AbstractListPropertyHandler<IFormLinkGetter<ICollisionLayerGetter>>
    {
        public override string PropertyName => "CollidesWith";
        public override ListSemantics Semantics => ListSemantics.SortedKeyed;

        protected override IReadOnlyList<object?> GetSortKey(IFormLinkGetter<ICollisionLayerGetter> item) => [item.FormKey];

        public override List<IFormLinkGetter<ICollisionLayerGetter>>? GetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecordGetter record)
        {
            if (record is ICollisionLayerGetter collisionLayer)
            {
                return collisionLayer.CollidesWith?.ToList();
            }

            return null;
        }

        public override void SetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecord record, List<IFormLinkGetter<ICollisionLayerGetter>>? value)
        {
            if (record is not ICollisionLayer collisionLayer)
            {
                return;
            }

            if (collisionLayer.CollidesWith == null)
            {
                return;
            }

            collisionLayer.CollidesWith.Clear();
            if (value == null)
            {
                return;
            }

            foreach (var link in value)
            {
                if (link == null || link.FormKey.IsNull) continue;
                collisionLayer.CollidesWith.Add(new FormLink<ICollisionLayerGetter>(link.FormKey));
            }
        }
    }
}
