using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using System.Collections.Generic;

namespace ForwardChanges.RecordHandlers
{
    public static class CellRecordHandler
    {
        public static void ProcessCellRecords(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            // TODO: Implement cell record processing
            Console.WriteLine("Cell record processing not yet implemented");
        }

        private static void ApplyForwardedProperties(ICell record, Dictionary<string, object?> propertiesToForward)
        {
            // TODO: Implement cell property forwarding
            Console.WriteLine("Cell property forwarding not yet implemented");
        }
    }
}