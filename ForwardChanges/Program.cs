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
            //typeof(IWeaponGetter),
            //typeof(ICellGetter)
        };

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
                            NpcRecordHandler.ProcessNpcRecords(state);
                            break;
                        case Type t when t == typeof(IWeaponGetter):
                            WeaponRecordHandler.ProcessWeaponRecords(state);
                            break;
                        case Type t when t == typeof(ICellGetter):
                            CellRecordHandler.ProcessCellRecords(state);
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
