using System.Collections.Generic;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.CameraPath
{
    public class ShotsHandler : AbstractListPropertyHandler<IFormLinkGetter<ICameraShotGetter>>
    {
        public override string PropertyName => "Shots";
        public override ListSemantics Semantics => ListSemantics.AlignedOrdered;

        public override List<IFormLinkGetter<ICameraShotGetter>>? GetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecordGetter record)
        {
            if (record is ICameraPathGetter cameraPath)
            {
                return cameraPath.Shots?.ToList();
            }

            return null;
        }

        public override void SetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecord record, List<IFormLinkGetter<ICameraShotGetter>>? value)
        {
            if (record is not ICameraPath cameraPath)
            {
                return;
            }

            if (cameraPath.Shots == null)
            {
                return;
            }

            cameraPath.Shots.Clear();
            if (value == null)
            {
                return;
            }

            foreach (var link in value)
            {
                if (link == null || link.FormKey.IsNull) continue;
                cameraPath.Shots.Add(new FormLink<ICameraShotGetter>(link.FormKey));
            }
        }
    }
}
