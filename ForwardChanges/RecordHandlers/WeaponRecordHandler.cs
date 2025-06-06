using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using System.Collections.Generic;

namespace ForwardChanges.RecordHandlers
{
    public static class WeaponRecordHandler
    {
        public static void ProcessWeaponRecords(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            // TODO: Implement weapon record processing
            Console.WriteLine("Weapon record processing not yet implemented");
        }

        private static void ApplyForwardedProperties(IWeapon record, Dictionary<string, object?> propertiesToForward)
        {
            // TODO: Implement weapon property forwarding
            Console.WriteLine("Weapon property forwarding not yet implemented");
        }
    }
}