using System.Collections.Generic;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.CameraPath
{
    public class RelatedPathsHandler : AbstractListPropertyHandler<IFormLinkGetter<ICameraPathGetter>>
    {
        public override string PropertyName => "RelatedPaths";

        public override List<IFormLinkGetter<ICameraPathGetter>>? GetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecordGetter record)
        {
            if (record is ICameraPathGetter cameraPath)
            {
                return cameraPath.RelatedPaths?.ToList();
            }

            return null;
        }

        public override void SetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecord record, List<IFormLinkGetter<ICameraPathGetter>>? value)
        {
            if (record is not ICameraPath cameraPath)
            {
                return;
            }

            if (cameraPath.RelatedPaths == null)
            {
                return;
            }

            cameraPath.RelatedPaths.Clear();
            if (value == null)
            {
                return;
            }

            foreach (var link in value)
            {
                if (link == null || link.FormKey.IsNull) continue;
                cameraPath.RelatedPaths.Add(new FormLink<ICameraPathGetter>(link.FormKey));
            }
        }
    }
}
