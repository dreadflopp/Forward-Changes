using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Plugins.Binary.Overlay;
using System.Reflection;
using System.Linq;
using Noggog;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.RecordHandlers;

namespace ForwardChanges
{
    public class Program
    {
        public static readonly Type[] SupportedRecordTypes = new[]
            {
                typeof(INpcGetter),
                typeof(IContainerGetter),
            //typeof(IWeaponGetter),
                typeof(ICellGetter)
        };

        /// <summary>
        /// Determines if processing can be skipped early based on optimization checks.
        /// Uses try-catch to handle potential data corruption issues gracefully.
        /// </summary>
        /// <param name="winningContext">The winning context for the record</param>
        /// <param name="state">The patcher state</param>
        /// <returns>True if processing can be skipped early, false otherwise</returns>
        protected static bool ShouldBreakEarly(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            try
            {
                // First, check if the winning context is vanilla - if so, we can break early immediately
                if (Utility.IsVanilla(winningContext))
                {
                    //Console.WriteLine("Breaking early: Winning context is vanilla");
                    return true;
                }

                // If we can't determine early break from winning context alone, 
                // we need to load contexts (but only the first 3 for efficiency)
                //var contexts = GetRecordContextsForEarlyBreak(winningContext, state);
                var contexts = winningContext.Record.ToLink().ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>(state.LinkCache).Take(3).ToArray();

                // If we have ≤2 contexts, we can break early
                if (contexts.Length <= 2)
                {
                   // Console.WriteLine("Breaking early: 2 or less contexts");
                    return true;
                }

                // Check if the mod before the winning context is vanilla
                var previousContext = contexts[1]; // Index 1 is the one before winning (index 2)
                if (Utility.IsVanilla(previousContext))
                {
                    //Console.WriteLine("Breaking early: Previous context is vanilla");
                    return true;
                }

                // No early break conditions met
                return false;
            }
            catch (Exception ex)
            {
                // If optimization fails due to data corruption, don't break early
                // Let the concrete handler decide with its safer type-specific implementation
                //Console.WriteLine($"\n     Early break optimization failed for {winningContext.Record.FormKey}, falling back to safe processing for record: {ex.Message}\n");
                return false;
            }
        }

        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "YourPatcher.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            Console.WriteLine("Starting Forward Changes patcher...");
            Console.WriteLine($"Processing {SupportedRecordTypes.Length} record types");

            // Get all contexts from the state and print the count
            Console.WriteLine("\nGetting all contexts from state...");

            // Get counts by record type
            var npcContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, INpc, INpcGetter>(state.LinkCache).ToArray();
            var containerContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IContainer, IContainerGetter>(state.LinkCache).ToArray();
            var cellContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ICell, ICellGetter>(state.LinkCache).ToArray();
            var weaponContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IWeapon, IWeaponGetter>(state.LinkCache).ToArray();

            // Filter out contexts that would break early
            Console.WriteLine("Filtering contexts (this may take a while)...");
            Console.Write("Filtering NPCs...");
            var filteredNpcContexts = npcContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.Write("done\n");
            Console.Write("Filtering Containers...");
            var filteredContainerContexts = containerContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.Write("done\n");
            Console.Write("Filtering Cells...");
            var filteredCellContexts = cellContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.Write("done\n");
            Console.Write("Filtering Weapons...");
            var filteredWeaponContexts = weaponContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine("done\n");

            var totalContexts = npcContexts.Length + containerContexts.Length + cellContexts.Length + weaponContexts.Length;
            var totalFilteredContexts = filteredNpcContexts.Length + filteredContainerContexts.Length + filteredCellContexts.Length + filteredWeaponContexts.Length;

            Console.WriteLine();
            Console.WriteLine($"NPC contexts: {npcContexts.Length} -> {filteredNpcContexts.Length} (filtered: {npcContexts.Length - filteredNpcContexts.Length})");
            Console.WriteLine($"Container contexts: {containerContexts.Length} -> {filteredContainerContexts.Length} (filtered: {containerContexts.Length - filteredContainerContexts.Length})");
            Console.WriteLine($"Cell contexts: {cellContexts.Length} -> {filteredCellContexts.Length} (filtered: {cellContexts.Length - filteredCellContexts.Length})");
            Console.WriteLine($"Weapon contexts: {weaponContexts.Length} -> {filteredWeaponContexts.Length} (filtered: {weaponContexts.Length - filteredWeaponContexts.Length})");

            foreach (var recordType in SupportedRecordTypes)
            {
                try
                {
                    Console.WriteLine("\n" + new string('-', 80));
                    Console.WriteLine($"Processing {recordType.Name} records");
                    Console.WriteLine(new string('-', 80));

                    switch (recordType)
                    {
                        case Type t when t == typeof(INpcGetter):
                            var npcHandler = new NpcRecordHandler();
                            npcHandler.Process(state, filteredNpcContexts);
                            break;
                        case Type t when t == typeof(IContainerGetter):
                            var containerHandler = new ContainerRecordHandler();
                            containerHandler.Process(state, filteredContainerContexts);
                            break;
                        case Type t when t == typeof(IWeaponGetter):
                            WeaponRecordHandler.ProcessWeaponRecords(state);
                            break;
                        case Type t when t == typeof(ICellGetter):
                            var cellHandler = new CellRecordHandler();
                            cellHandler.Process(state, filteredCellContexts);
                            break;
                        default:
                            Console.WriteLine($"Warning: No handler implemented for {recordType.Name}");
                            break;
                    }

                    Console.WriteLine($"Completed processing {recordType.Name} records");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {recordType.Name} records:");
                    Console.WriteLine($"Exception: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }

            Console.WriteLine("\nForward Changes patcher completed.");
        }
    }
}
