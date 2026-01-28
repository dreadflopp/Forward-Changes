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
using Mutagen.Bethesda.Plugins.Aspects;

namespace ForwardChanges
{
    public class Program
    {
        public static readonly Type[] SupportedRecordTypes = new[]
            {
                typeof(INpcGetter),
                typeof(IContainerGetter),
                typeof(IWeaponGetter),
                typeof(ICellGetter),
                typeof(IPlacedObjectGetter),
                typeof(IPlacedNpcGetter),
                typeof(IIngestibleGetter),
                typeof(IIngredientGetter),
                typeof(IObjectEffectGetter),
                typeof(IWorldspaceGetter),
                typeof(IDialogTopicGetter),
                typeof(IDialogResponsesGetter),
                typeof(IFormListGetter),
                typeof(ISoundDescriptorGetter),
                typeof(IEffectShaderGetter),
                typeof(IArmorAddonGetter),
                typeof(IBookGetter),
                typeof(ISpellGetter),
                typeof(ILocationGetter),
                typeof(IFactionGetter),
                typeof(IEncounterZoneGetter),
                typeof(IActivatorGetter),
                typeof(ILightGetter),
                typeof(IMagicEffectGetter),
                typeof(IQuestGetter),
                typeof(ITextureSetGetter),
                typeof(IMiscItemGetter),
                typeof(IStaticGetter),
                typeof(ILeveledItemGetter)
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
                //Console.WriteLine($"     Early break optimization failed for {winningContext.Record.FormKey} ({winningContext.ModKey}): {ex.Message}");
                //Console.WriteLine($"     Record type: {winningContext.Record.GetType().Name}");
                //Console.WriteLine($"     Exception type: {ex.GetType().Name}");
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

            string outputModName = state.PatchMod.ModKey.ToString();
            Console.WriteLine($"Output mod name: {outputModName}");

            // Get all contexts from the state and print the count
            Console.WriteLine("\nGetting all contexts from state...");

            // Get counts by record type
            var npcContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, INpc, INpcGetter>(state.LinkCache).ToArray();
            var containerContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IContainer, IContainerGetter>(state.LinkCache).ToArray();
            var cellContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ICell, ICellGetter>(state.LinkCache).ToArray();
            var weaponContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IWeapon, IWeaponGetter>(state.LinkCache).ToArray();
            var placedObjectContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IPlacedObject, IPlacedObjectGetter>(state.LinkCache).ToArray();
            var placedNpcContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IPlacedNpc, IPlacedNpcGetter>(state.LinkCache).ToArray();
            var ingestibleContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IIngestible, IIngestibleGetter>(state.LinkCache).ToArray();
            var ingredientContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IIngredient, IIngredientGetter>(state.LinkCache).ToArray();
            var objectEffectContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IObjectEffect, IObjectEffectGetter>(state.LinkCache).ToArray();
            var worldspaceContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IWorldspace, IWorldspaceGetter>(state.LinkCache).ToArray();
            var dialogTopicContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IDialogTopic, IDialogTopicGetter>(state.LinkCache).ToArray();
            var dialogResponseContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IDialogResponses, IDialogResponsesGetter>(state.LinkCache).ToArray();
            var formListContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IFormList, IFormListGetter>(state.LinkCache).ToArray();
            var soundDescriptorContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ISoundDescriptor, ISoundDescriptorGetter>(state.LinkCache).ToArray();
            var effectShaderContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IEffectShader, IEffectShaderGetter>(state.LinkCache).ToArray();
            var armorAddonContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IArmorAddon, IArmorAddonGetter>(state.LinkCache).ToArray();
            var bookContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IBook, IBookGetter>(state.LinkCache).ToArray();
            var spellContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ISpell, ISpellGetter>(state.LinkCache).ToArray();
            var locationContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ILocation, ILocationGetter>(state.LinkCache).ToArray();
            var factionContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IFaction, IFactionGetter>(state.LinkCache).ToArray();
            var encounterZoneContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IEncounterZone, IEncounterZoneGetter>(state.LinkCache).ToArray();
            var activatorContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IActivator, IActivatorGetter>(state.LinkCache).ToArray();
            var lightContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ILight, ILightGetter>(state.LinkCache).ToArray();
            var magicEffectContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IMagicEffect, IMagicEffectGetter>(state.LinkCache).ToArray();
            var questContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IQuest, IQuestGetter>(state.LinkCache).ToArray();
            var textureSetContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ITextureSet, ITextureSetGetter>(state.LinkCache).ToArray();
            var miscItemContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IMiscItem, IMiscItemGetter>(state.LinkCache).ToArray();
            var staticContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IStatic, IStaticGetter>(state.LinkCache).ToArray();
            var leveledItemContexts = state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ILeveledItem, ILeveledItemGetter>(state.LinkCache).ToArray();

            // Filter out contexts that would break early
            Console.WriteLine("Filtering contexts (this may take a while)...");
            Console.WriteLine("Filtering Ingestibles...");
            var filteredIngestibleContexts = ingestibleContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Ingestible contexts: {ingestibleContexts.Length} -> {filteredIngestibleContexts.Length} (filtered: {ingestibleContexts.Length - filteredIngestibleContexts.Length})");
            Console.WriteLine("Filtering Ingredients...");
            var filteredIngredientContexts = ingredientContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Ingredient contexts: {ingredientContexts.Length} -> {filteredIngredientContexts.Length} (filtered: {ingredientContexts.Length - filteredIngredientContexts.Length})");
            Console.WriteLine("Filtering Object Effects...");
            var filteredObjectEffectContexts = objectEffectContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Object Effect contexts: {objectEffectContexts.Length} -> {filteredObjectEffectContexts.Length} (filtered: {objectEffectContexts.Length - filteredObjectEffectContexts.Length})");
            Console.WriteLine("Filtering Worldspaces...");
            var filteredWorldspaceContexts = worldspaceContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Worldspace contexts: {worldspaceContexts.Length} -> {filteredWorldspaceContexts.Length} (filtered: {worldspaceContexts.Length - filteredWorldspaceContexts.Length})");
            Console.WriteLine("Filtering Containers...");
            var filteredContainerContexts = containerContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Container contexts: {containerContexts.Length} -> {filteredContainerContexts.Length} (filtered: {containerContexts.Length - filteredContainerContexts.Length})");
            Console.WriteLine("Filtering Cells...");
            var filteredCellContexts = cellContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Cell contexts: {cellContexts.Length} -> {filteredCellContexts.Length} (filtered: {cellContexts.Length - filteredCellContexts.Length})");
            Console.WriteLine("Filtering Weapons...");
            var filteredWeaponContexts = weaponContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Weapon contexts: {weaponContexts.Length} -> {filteredWeaponContexts.Length} (filtered: {weaponContexts.Length - filteredWeaponContexts.Length})");
            Console.WriteLine("Filtering Placed Objects...");
            var filteredPlacedObjectContexts = placedObjectContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Placed Object contexts: {placedObjectContexts.Length} -> {filteredPlacedObjectContexts.Length} (filtered: {placedObjectContexts.Length - filteredPlacedObjectContexts.Length})");
            Console.WriteLine("Filtering Placed NPCs...");
            var filteredPlacedNpcContexts = placedNpcContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Placed NPC contexts: {placedNpcContexts.Length} -> {filteredPlacedNpcContexts.Length} (filtered: {placedNpcContexts.Length - filteredPlacedNpcContexts.Length})");
            Console.WriteLine("Filtering NPCs...");
            var filteredNpcContexts = npcContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"NPC contexts: {npcContexts.Length} -> {filteredNpcContexts.Length} (filtered: {npcContexts.Length - filteredNpcContexts.Length})");
            Console.WriteLine("Filtering Dialog Topics...");
            var filteredDialogTopicContexts = dialogTopicContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Dialog Topic contexts: {dialogTopicContexts.Length} -> {filteredDialogTopicContexts.Length} (filtered: {dialogTopicContexts.Length - filteredDialogTopicContexts.Length})");
            Console.WriteLine("Filtering Dialog Responses...");
            var filteredDialogResponseContexts = dialogResponseContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Dialog Response contexts: {dialogResponseContexts.Length} -> {filteredDialogResponseContexts.Length} (filtered: {dialogResponseContexts.Length - filteredDialogResponseContexts.Length})");
            Console.WriteLine("Filtering Form Lists...");
            var filteredFormListContexts = formListContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Form List contexts: {formListContexts.Length} -> {filteredFormListContexts.Length} (filtered: {formListContexts.Length - filteredFormListContexts.Length})");
            Console.WriteLine("Filtering Sound Descriptors...");
            var filteredSoundDescriptorContexts = soundDescriptorContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Sound Descriptor contexts: {soundDescriptorContexts.Length} -> {filteredSoundDescriptorContexts.Length} (filtered: {soundDescriptorContexts.Length - filteredSoundDescriptorContexts.Length})");
            Console.WriteLine("Filtering Effect Shaders...");
            var filteredEffectShaderContexts = effectShaderContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Effect Shader contexts: {effectShaderContexts.Length} -> {filteredEffectShaderContexts.Length} (filtered: {effectShaderContexts.Length - filteredEffectShaderContexts.Length})");
            Console.WriteLine("Filtering Armor Addons...");
            var filteredArmorAddonContexts = armorAddonContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Armor Addon contexts: {armorAddonContexts.Length} -> {filteredArmorAddonContexts.Length} (filtered: {armorAddonContexts.Length - filteredArmorAddonContexts.Length})");
            Console.WriteLine("Filtering Books...");
            var filteredBookContexts = bookContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Book contexts: {bookContexts.Length} -> {filteredBookContexts.Length} (filtered: {bookContexts.Length - filteredBookContexts.Length})");
            Console.WriteLine("Filtering Spells...");
            var filteredSpellContexts = spellContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Spell contexts: {spellContexts.Length} -> {filteredSpellContexts.Length} (filtered: {spellContexts.Length - filteredSpellContexts.Length})");
            Console.WriteLine("Filtering Locations...");
            var filteredLocationContexts = locationContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Location contexts: {locationContexts.Length} -> {filteredLocationContexts.Length} (filtered: {locationContexts.Length - filteredLocationContexts.Length})");
            Console.WriteLine("Filtering Factions...");
            var filteredFactionContexts = factionContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Faction contexts: {factionContexts.Length} -> {filteredFactionContexts.Length} (filtered: {factionContexts.Length - filteredFactionContexts.Length})");
            Console.WriteLine("Filtering Encounter Zones...");
            var filteredEncounterZoneContexts = encounterZoneContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Encounter Zone contexts: {encounterZoneContexts.Length} -> {filteredEncounterZoneContexts.Length} (filtered: {encounterZoneContexts.Length - filteredEncounterZoneContexts.Length})");
            Console.WriteLine("Filtering Activators...");
            var filteredActivatorContexts = activatorContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Activator contexts: {activatorContexts.Length} -> {filteredActivatorContexts.Length} (filtered: {activatorContexts.Length - filteredActivatorContexts.Length})");
            Console.WriteLine("Filtering Lights...");
            var filteredLightContexts = lightContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Light contexts: {lightContexts.Length} -> {filteredLightContexts.Length} (filtered: {lightContexts.Length - filteredLightContexts.Length})");
            Console.WriteLine("Filtering Magic Effects...");
            var filteredMagicEffectContexts = magicEffectContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Magic Effect contexts: {magicEffectContexts.Length} -> {filteredMagicEffectContexts.Length} (filtered: {magicEffectContexts.Length - filteredMagicEffectContexts.Length})");
            Console.WriteLine("Filtering Quests...");
            var filteredQuestContexts = questContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Quest contexts: {questContexts.Length} -> {filteredQuestContexts.Length} (filtered: {questContexts.Length - filteredQuestContexts.Length})");
            Console.WriteLine("Filtering Texture Sets...");
            var filteredTextureSetContexts = textureSetContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Texture Set contexts: {textureSetContexts.Length} -> {filteredTextureSetContexts.Length} (filtered: {textureSetContexts.Length - filteredTextureSetContexts.Length})");
            Console.WriteLine("Filtering Misc Items...");
            var filteredMiscItemContexts = miscItemContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Misc Item contexts: {miscItemContexts.Length} -> {filteredMiscItemContexts.Length} (filtered: {miscItemContexts.Length - filteredMiscItemContexts.Length})");
            Console.WriteLine("Filtering Statics...");
            var filteredStaticContexts = staticContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Static contexts: {staticContexts.Length} -> {filteredStaticContexts.Length} (filtered: {staticContexts.Length - filteredStaticContexts.Length})");
            Console.WriteLine("Filtering Leveled Items...");
            var filteredLeveledItemContexts = leveledItemContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Leveled Item contexts: {leveledItemContexts.Length} -> {filteredLeveledItemContexts.Length} (filtered: {leveledItemContexts.Length - filteredLeveledItemContexts.Length})");


            Console.WriteLine();

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
                            var weaponHandler = new WeaponRecordHandler();
                            weaponHandler.Process(state, filteredWeaponContexts);
                            break;
                        case Type t when t == typeof(ICellGetter):
                            var cellHandler = new CellRecordHandler();
                            cellHandler.Process(state, filteredCellContexts);
                            break;
                        case Type t when t == typeof(IPlacedObjectGetter):
                            var placedObjectHandler = new PlacedObjectRecordHandler();
                            placedObjectHandler.Process(state, filteredPlacedObjectContexts);
                            break;
                        case Type t when t == typeof(IPlacedNpcGetter):
                            var placedNpcHandler = new PlacedNpcRecordHandler();
                            placedNpcHandler.Process(state, filteredPlacedNpcContexts);
                            break;
                        case Type t when t == typeof(IIngestibleGetter):
                            var ingestibleHandler = new IngestibleRecordHandler();
                            ingestibleHandler.Process(state, filteredIngestibleContexts);
                            break;
                        case Type t when t == typeof(IIngredientGetter):
                            var ingredientHandler = new IngredientRecordHandler();
                            ingredientHandler.Process(state, filteredIngredientContexts);
                            break;
                        case Type t when t == typeof(IObjectEffectGetter):
                            var objectEffectHandler = new ObjectEffectRecordHandler();
                            objectEffectHandler.Process(state, filteredObjectEffectContexts);
                            break;
                        case Type t when t == typeof(IWorldspaceGetter):
                            var worldspaceHandler = new WorldspaceRecordHandler();
                            worldspaceHandler.Process(state, filteredWorldspaceContexts);
                            break;
                        case Type t when t == typeof(IDialogTopicGetter):
                            var dialogTopicHandler = new DialogTopicRecordHandler();
                            dialogTopicHandler.Process(state, filteredDialogTopicContexts);
                            break;
                        case Type t when t == typeof(IDialogResponsesGetter):
                            var dialogResponseHandler = new DialogResponseRecordHandler();
                            dialogResponseHandler.Process(state, filteredDialogResponseContexts);
                            break;
                        case Type t when t == typeof(IFormListGetter):
                            var formListHandler = new FormIdRecordHandler();
                            formListHandler.Process(state, filteredFormListContexts);
                            break;
                        case Type t when t == typeof(ISoundDescriptorGetter):
                            var soundDescriptorHandler = new SoundDescriptorRecordHandler();
                            soundDescriptorHandler.Process(state, filteredSoundDescriptorContexts);
                            break;
                        case Type t when t == typeof(IEffectShaderGetter):
                            var effectShaderHandler = new EffectShaderRecordHandler();
                            effectShaderHandler.Process(state, filteredEffectShaderContexts);
                            break;
                        case Type t when t == typeof(IArmorAddonGetter):
                            var armorAddonHandler = new ArmorAddonRecordHandler();
                            armorAddonHandler.Process(state, filteredArmorAddonContexts);
                            break;
                        case Type t when t == typeof(IBookGetter):
                            var bookHandler = new BookRecordHandler();
                            bookHandler.Process(state, filteredBookContexts);
                            break;
                        case Type t when t == typeof(ISpellGetter):
                            var spellHandler = new SpellRecordHandler();
                            spellHandler.Process(state, filteredSpellContexts);
                            break;
                        case Type t when t == typeof(ILocationGetter):
                            var locationHandler = new LocationRecordHandler();
                            locationHandler.Process(state, filteredLocationContexts);
                            break;
                        case Type t when t == typeof(IFactionGetter):
                            var factionHandler = new FactionRecordHandler();
                            factionHandler.Process(state, filteredFactionContexts);
                            break;
                        case Type t when t == typeof(IEncounterZoneGetter):
                            var encounterZoneHandler = new EncounterZoneRecordHandler();
                            encounterZoneHandler.Process(state, filteredEncounterZoneContexts);
                            break;
                        case Type t when t == typeof(IActivatorGetter):
                            var activatorHandler = new ActivatorRecordHandler();
                            activatorHandler.Process(state, filteredActivatorContexts);
                            break;
                        case Type t when t == typeof(ILightGetter):
                            var lightHandler = new LightRecordHandler();
                            lightHandler.Process(state, filteredLightContexts);
                            break;
                        case Type t when t == typeof(IMagicEffectGetter):
                            var magicEffectHandler = new MagicEffectRecordHandler();
                            magicEffectHandler.Process(state, filteredMagicEffectContexts);
                            break;
                        case Type t when t == typeof(IQuestGetter):
                            var questHandler = new QuestRecordHandler();
                            questHandler.Process(state, filteredQuestContexts);
                            break;
                        case Type t when t == typeof(ITextureSetGetter):
                            var textureSetHandler = new TextureSetRecordHandler();
                            textureSetHandler.Process(state, filteredTextureSetContexts);
                            break;
                        case Type t when t == typeof(IMiscItemGetter):
                            var miscItemHandler = new MiscItemRecordHandler();
                            miscItemHandler.Process(state, filteredMiscItemContexts);
                            break;
                        case Type t when t == typeof(IStaticGetter):
                            var staticHandler = new StaticRecordHandler();
                            staticHandler.Process(state, filteredStaticContexts);
                            break;
                        case Type t when t == typeof(ILeveledItemGetter):
                            var leveledItemHandler = new LeveledItemRecordHandler();
                            leveledItemHandler.Process(state, filteredLeveledItemContexts);
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
