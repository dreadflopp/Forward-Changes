using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using DreadsMashedPatch.PropertyHandlers.Abstracts;

namespace DreadsMashedPatch.PropertyHandlers.MusicTrack
{
    public class CuePointsHandler : AbstractPropertyHandler<List<float>?>
    {
        public override string PropertyName => "CuePoints";

        public override List<float>? GetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecordGetter record)
        {
            return (record as IMusicTrackGetter)?.CuePoints?.ToList();
        }

        public override void SetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecord record, List<float>? value)
        {
            if (record is not IMusicTrack musicTrack)
            {
                return;
            }

            if (value == null)
            {
                musicTrack.CuePoints = null;
                return;
            }

            if (musicTrack.CuePoints == null)
            {
                musicTrack.CuePoints = new Noggog.ExtendedList<float>();
            }

            musicTrack.CuePoints.Clear();
            foreach (var cuePoint in value)
            {
                musicTrack.CuePoints.Add(cuePoint);
            }
        }

        public override bool AreValuesEqual(List<float>? value1, List<float>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1.Count != value2.Count) return false;

            for (var i = 0; i < value1.Count; i++)
            {
                if (value1[i] != value2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}