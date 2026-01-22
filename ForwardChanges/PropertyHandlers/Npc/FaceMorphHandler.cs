using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class FaceMorphHandler : AbstractPropertyHandler<INpcFaceMorphGetter>
    {
        public override string PropertyName => "FaceMorph";

        public override INpcFaceMorphGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npcRecord)
            {
                return npcRecord.FaceMorph;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpcGetter for {PropertyName}");
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, INpcFaceMorphGetter? value)
        {
            if (record is INpc npcRecord)
            {
                if (value == null)
                {
                    npcRecord.FaceMorph = null;
                    return;
                }

                // Create a new NpcFaceMorph instance and copy all properties
                var newFaceMorph = new NpcFaceMorph
                {
                    NoseLongVsShort = value.NoseLongVsShort,
                    NoseUpVsDown = value.NoseUpVsDown,
                    JawUpVsDown = value.JawUpVsDown,
                    JawNarrowVsWide = value.JawNarrowVsWide,
                    JawForwardVsBack = value.JawForwardVsBack,
                    CheeksUpVsDown = value.CheeksUpVsDown,
                    CheeksForwardVsBack = value.CheeksForwardVsBack,
                    EyesUpVsDown = value.EyesUpVsDown,
                    EyesInVsOut = value.EyesInVsOut,
                    BrowsUpVsDown = value.BrowsUpVsDown,
                    BrowsInVsOut = value.BrowsInVsOut,
                    BrowsForwardVsBack = value.BrowsForwardVsBack,
                    LipsUpVsDown = value.LipsUpVsDown,
                    LipsInVsOut = value.LipsInVsOut,
                    ChinNarrowVsWide = value.ChinNarrowVsWide,
                    ChinUpVsDown = value.ChinUpVsDown,
                    ChinUnderbiteVsOverbite = value.ChinUnderbiteVsOverbite,
                    EyesForwardVsBack = value.EyesForwardVsBack,
                    Unknown = value.Unknown
                };

                npcRecord.FaceMorph = newFaceMorph;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpc for {PropertyName}");
            }
        }

        public override bool AreValuesEqual(INpcFaceMorphGetter? value1, INpcFaceMorphGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare all properties
            if (Math.Abs(value1.NoseLongVsShort - value2.NoseLongVsShort) >= 0.001f) return false;
            if (Math.Abs(value1.NoseUpVsDown - value2.NoseUpVsDown) >= 0.001f) return false;
            if (Math.Abs(value1.JawUpVsDown - value2.JawUpVsDown) >= 0.001f) return false;
            if (Math.Abs(value1.JawNarrowVsWide - value2.JawNarrowVsWide) >= 0.001f) return false;
            if (Math.Abs(value1.JawForwardVsBack - value2.JawForwardVsBack) >= 0.001f) return false;
            if (Math.Abs(value1.CheeksUpVsDown - value2.CheeksUpVsDown) >= 0.001f) return false;
            if (Math.Abs(value1.CheeksForwardVsBack - value2.CheeksForwardVsBack) >= 0.001f) return false;
            if (Math.Abs(value1.EyesUpVsDown - value2.EyesUpVsDown) >= 0.001f) return false;
            if (Math.Abs(value1.EyesInVsOut - value2.EyesInVsOut) >= 0.001f) return false;
            if (Math.Abs(value1.BrowsUpVsDown - value2.BrowsUpVsDown) >= 0.001f) return false;
            if (Math.Abs(value1.BrowsInVsOut - value2.BrowsInVsOut) >= 0.001f) return false;
            if (Math.Abs(value1.BrowsForwardVsBack - value2.BrowsForwardVsBack) >= 0.001f) return false;
            if (Math.Abs(value1.LipsUpVsDown - value2.LipsUpVsDown) >= 0.001f) return false;
            if (Math.Abs(value1.LipsInVsOut - value2.LipsInVsOut) >= 0.001f) return false;
            if (Math.Abs(value1.ChinNarrowVsWide - value2.ChinNarrowVsWide) >= 0.001f) return false;
            if (Math.Abs(value1.ChinUpVsDown - value2.ChinUpVsDown) >= 0.001f) return false;
            if (Math.Abs(value1.ChinUnderbiteVsOverbite - value2.ChinUnderbiteVsOverbite) >= 0.001f) return false;
            if (Math.Abs(value1.EyesForwardVsBack - value2.EyesForwardVsBack) >= 0.001f) return false;
            if (Math.Abs(value1.Unknown - value2.Unknown) >= 0.001f) return false;

            return true;
        }

        public override string FormatValue(object? value)
        {
            if (value is INpcFaceMorphGetter faceMorph)
            {
                return $"NoseLongVsShort={faceMorph.NoseLongVsShort:F3}, " +
                       $"NoseUpVsDown={faceMorph.NoseUpVsDown:F3}, " +
                       $"JawUpVsDown={faceMorph.JawUpVsDown:F3}, " +
                       $"JawNarrowVsWide={faceMorph.JawNarrowVsWide:F3}, " +
                       $"JawForwardVsBack={faceMorph.JawForwardVsBack:F3}, " +
                       $"CheeksUpVsDown={faceMorph.CheeksUpVsDown:F3}, " +
                       $"CheeksForwardVsBack={faceMorph.CheeksForwardVsBack:F3}, " +
                       $"EyesUpVsDown={faceMorph.EyesUpVsDown:F3}, " +
                       $"EyesInVsOut={faceMorph.EyesInVsOut:F3}, " +
                       $"BrowsUpVsDown={faceMorph.BrowsUpVsDown:F3}, " +
                       $"BrowsInVsOut={faceMorph.BrowsInVsOut:F3}, " +
                       $"BrowsForwardVsBack={faceMorph.BrowsForwardVsBack:F3}, " +
                       $"LipsUpVsDown={faceMorph.LipsUpVsDown:F3}, " +
                       $"LipsInVsOut={faceMorph.LipsInVsOut:F3}, " +
                       $"ChinNarrowVsWide={faceMorph.ChinNarrowVsWide:F3}, " +
                       $"ChinUpVsDown={faceMorph.ChinUpVsDown:F3}, " +
                       $"ChinUnderbiteVsOverbite={faceMorph.ChinUnderbiteVsOverbite:F3}, " +
                       $"EyesForwardVsBack={faceMorph.EyesForwardVsBack:F3}, " +
                       $"Unknown={faceMorph.Unknown:F3}";
            }
            return value?.ToString() ?? "null";
        }
    }
}