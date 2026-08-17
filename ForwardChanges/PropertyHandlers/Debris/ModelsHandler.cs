using System.Collections.Generic;
using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Debris
{
    public class ModelsHandler : AbstractListPropertyHandler<IDebrisModelGetter>
    {
        public override string PropertyName => "Models";

        public override List<IDebrisModelGetter>? GetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecordGetter record)
        {
            if (record is IDebrisGetter debris)
            {
                return debris.Models?.ToList();
            }

            return null;
        }

        public override void SetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecord record, List<IDebrisModelGetter>? value)
        {
            if (record is not IDebris debris)
            {
                return;
            }

            if (debris.Models == null)
            {
                return;
            }

            debris.Models.Clear();
            if (value == null)
            {
                return;
            }

            foreach (var model in value)
            {
                if (model == null) continue;
                debris.Models.Add(model.DeepCopy());
            }
        }
    }
}
