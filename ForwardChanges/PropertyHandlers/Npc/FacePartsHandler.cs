using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class FacePartsHandler : AbstractPropertyHandler<INpcFacePartsGetter>
    {
        public override string PropertyName => "FaceParts";

        public override INpcFacePartsGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npcRecord)
            {
                return npcRecord.FaceParts;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpcGetter for {PropertyName}");
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, INpcFacePartsGetter? value)
        {
            if (record is INpc npcRecord)
            {
                if (value == null)
                {
                    npcRecord.FaceParts = null;
                    return;
                }

                // Create a new NpcFaceParts instance and copy all properties
                var newFaceParts = new NpcFaceParts
                {
                    Nose = value.Nose,
                    Unknown = value.Unknown,
                    Eyes = value.Eyes,
                    Mouth = value.Mouth
                };

                npcRecord.FaceParts = newFaceParts;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpc for {PropertyName}");
            }
        }

        public override bool AreValuesEqual(INpcFacePartsGetter? value1, INpcFacePartsGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare all properties
            if (value1.Nose != value2.Nose) return false;
            if (value1.Unknown != value2.Unknown) return false;
            if (value1.Eyes != value2.Eyes) return false;
            if (value1.Mouth != value2.Mouth) return false;

            return true;
        }
    }
}