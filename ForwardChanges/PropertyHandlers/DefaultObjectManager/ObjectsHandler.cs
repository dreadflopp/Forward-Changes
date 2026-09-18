using System.Collections.Generic;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DefaultObjectManager
{
    public class ObjectsHandler : AbstractListPropertyHandler<IDefaultObjectGetter>
    {
        public override string PropertyName => "Objects";
        public override ListSemantics Semantics => ListSemantics.SortedKeyed;

        protected override bool IsItemIdentityEqual(IDefaultObjectGetter? left, IDefaultObjectGetter? right) =>
            left?.Use == right?.Use;

        protected override IReadOnlyList<object?> GetSortKey(IDefaultObjectGetter item) => [item.Use.TypeInt];

        public override List<IDefaultObjectGetter>? GetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecordGetter record)
        {
            if (record is IDefaultObjectManagerGetter manager)
            {
                return manager.Objects?.ToList();
            }

            return null;
        }

        public override void SetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecord record, List<IDefaultObjectGetter>? value)
        {
            if (record is not IDefaultObjectManager manager)
            {
                return;
            }

            if (value == null)
            {
                manager.Objects = null;
                return;
            }

            manager.Objects ??= new ExtendedList<DefaultObject>();
            manager.Objects.Clear();

            foreach (var item in value)
            {
                if (item == null) continue;
                manager.Objects.Add(item.DeepCopy());
            }
        }
    }
}
