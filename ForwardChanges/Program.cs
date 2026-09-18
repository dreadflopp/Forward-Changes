using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Plugins.Binary.Overlay;
using System.Linq;
using System.Collections.Generic;
using Noggog;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.RecordHandlers;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.FormList;
using ForwardChanges.Contexts;
using Mutagen.Bethesda.Plugins.Aspects;

namespace ForwardChanges
{
    public class Program
    {
        public static readonly IReadOnlyDictionary<Type, string> ExcludedRecordTypes =
            new Dictionary<Type, string>
            {
                [typeof(IDefaultObjectManagerGetter)] = "Default object mappings are merged by the Skyrim runtime.",
                [typeof(ILandscapeTextureGetter)] = "Landscape texture records are excluded by the runtime-field forwarding policy.",
                [typeof(ILandscapeGetter)] = "Landscape records are excluded by the runtime-field forwarding policy.",
                [typeof(IImageSpaceAdapterGetter)] = "Image Space Adapter records are disabled because Mutagen does not preserve missing DNAM subrecords and writes zero-valued DNAM data instead."
            };

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
                typeof(IPackageGetter),
                typeof(IPerkGetter),
                typeof(IRaceGetter),
                typeof(IRegionGetter),
                typeof(IWorldspaceGetter),
                typeof(IDialogTopicGetter),
                typeof(IDialogResponsesGetter),
                typeof(IFormListGetter),
                typeof(ISoundDescriptorGetter),
                typeof(IEffectShaderGetter),
                typeof(IArmorAddonGetter),
                typeof(IArmorGetter),
                typeof(IAmmunitionGetter),
                typeof(IBookGetter),
                typeof(ISpellGetter),
                typeof(ILocationGetter),
                typeof(IFactionGetter),
                typeof(IEncounterZoneGetter),
                typeof(IActivatorGetter),
                typeof(ILightGetter),
                typeof(IMagicEffectGetter),
                typeof(IProjectileGetter),
                typeof(IQuestGetter),
                typeof(ITextureSetGetter),
                typeof(IMiscItemGetter),
                typeof(IKeyGetter),
                typeof(IStaticGetter),
                typeof(ILeveledItemGetter),
                typeof(IActionRecordGetter),
                typeof(IAddonNodeGetter),
                typeof(IAnimatedObjectGetter),
                typeof(IAlchemicalApparatusGetter),
                typeof(IArtObjectGetter),
                typeof(IAcousticSpaceGetter),
                typeof(IAssociationTypeGetter),
                typeof(IActorValueInformationGetter),
                typeof(IBodyPartDataGetter),
                typeof(ICameraShotGetter),
                typeof(IClassGetter),
                typeof(IColorRecordGetter),
                typeof(IClimateGetter),
                typeof(IConstructibleObjectGetter),
                typeof(ICollisionLayerGetter),
                typeof(ICameraPathGetter),
                typeof(ICombatStyleGetter),
                typeof(IDebrisGetter),
                typeof(IDialogBranchGetter),
                typeof(IDialogViewGetter),
                typeof(IDoorGetter),
                typeof(IDualCastDataGetter),
                typeof(IEquipTypeGetter),
                typeof(IExplosionGetter),
                typeof(IEyesGetter),
                typeof(IFloraGetter),
                typeof(IFootstepGetter),
                typeof(IFootstepSetGetter),
                typeof(IFurnitureGetter),
                typeof(IGlobalIntGetter),
                typeof(IGlobalShortGetter),
                typeof(IGlobalFloatGetter),
                typeof(IGlobalUnknownGetter),
                typeof(IGameSettingIntGetter),
                typeof(IGameSettingFloatGetter),
                typeof(IGameSettingStringGetter),
                typeof(IGameSettingBoolGetter),
                typeof(IGrassGetter),
                typeof(IHazardGetter),
                typeof(IHeadPartGetter),
                typeof(IIdleAnimationGetter),
                typeof(IIdleMarkerGetter),
                typeof(ILightingTemplateGetter),
                typeof(ILoadScreenGetter),
                typeof(ILeveledNpcGetter),
                typeof(ILeveledSpellGetter),
                typeof(IMoveableStaticGetter),
                typeof(IMovementTypeGetter),
                typeof(IMusicTypeGetter),
                typeof(IMusicTrackGetter),
                typeof(INavigationMeshGetter),
                typeof(IOutfitGetter),
                typeof(IPlacedHazardGetter),
                typeof(ISoundCategoryGetter),
                typeof(ISoundOutputModelGetter),
                typeof(ISoundMarkerGetter),
                typeof(IShaderParticleGeometryGetter),
                typeof(ISceneGetter),
                typeof(IScrollGetter),
                typeof(IShoutGetter),
                typeof(ISoulGemGetter),
                typeof(IStoryManagerBranchNodeGetter),
                typeof(IStoryManagerEventNodeGetter),
                typeof(IStoryManagerQuestNodeGetter),
                typeof(ITreeGetter),
                typeof(IVoiceTypeGetter),
                typeof(IWaterGetter),
                typeof(IWeatherGetter),
                typeof(IWordOfPowerGetter),
                typeof(IRelationshipGetter),
                typeof(IReverbParametersGetter),
                typeof(IVisualEffectGetter),
                typeof(IMaterialObjectGetter),
                typeof(IMaterialTypeGetter),
                typeof(IMessageGetter),
                typeof(IKeywordGetter),
                typeof(ILocationReferenceTypeGetter),
                typeof(IImageSpaceGetter),
                typeof(IImpactGetter),
                typeof(IImpactDataSetGetter)
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
            catch (Exception)
            {
                //Console.WriteLine($"     Early break optimization failed for {winningContext.Record.FormKey} ({winningContext.ModKey}): {ex.Message}");
                //Console.WriteLine($"     Record type: {winningContext.Record.GetType().Name}");
                //Console.WriteLine($"     Exception type: {ex.GetType().Name}");
                return false;
            }
        }

        private static TContext[] LoadContextsSafely<TContext>(IEnumerable<TContext> contexts, string contextName)
        {
            var skippedCount = 0;
            var loaded = contexts
                .Catch(ex =>
                {
                    skippedCount++;
                    Console.WriteLine($"[Warning] Skipping malformed {contextName} context: {ex.GetType().Name}: {ex.Message}");
                })
                .ToArray();

            if (skippedCount > 0)
            {
                Console.WriteLine($"[Warning] Skipped {skippedCount} malformed {contextName} context(s) during load.");
            }

            return loaded;
        }

        private static IModContext<ISkyrimMod, ISkyrimModGetter, TDerivedSetter, TDerivedGetter>[] NarrowContexts<
            TBaseSetter,
            TBaseGetter,
            TDerivedSetter,
            TDerivedGetter>(
            IEnumerable<IModContext<ISkyrimMod, ISkyrimModGetter, TBaseSetter, TBaseGetter>> contexts)
            where TBaseSetter : class, IMajorRecordQueryable, TBaseGetter
            where TBaseGetter : class, IMajorRecordQueryableGetter
            where TDerivedSetter : class, TBaseSetter, TDerivedGetter
            where TDerivedGetter : class, TBaseGetter
        {
            return contexts
                .Where(context => context.Record is TDerivedGetter)
                .Select(context => context.AsType<
                    ISkyrimMod,
                    ISkyrimModGetter,
                    TBaseSetter,
                    TBaseGetter,
                    TDerivedSetter,
                    TDerivedGetter>())
                .ToArray();
        }


        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "Synthesis.esp")
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
            var npcContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, INpc, INpcGetter>(state.LinkCache),
                "npcContexts");
            var containerContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IContainer, IContainerGetter>(state.LinkCache),
                "containerContexts");
            var cellContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ICell, ICellGetter>(state.LinkCache),
                "cellContexts");
            var weaponContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IWeapon, IWeaponGetter>(state.LinkCache),
                "weaponContexts");
            var placedObjectContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IPlacedObject, IPlacedObjectGetter>(state.LinkCache),
                "placedObjectContexts");
            var placedNpcContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IPlacedNpc, IPlacedNpcGetter>(state.LinkCache),
                "placedNpcContexts");
            var ingestibleContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IIngestible, IIngestibleGetter>(state.LinkCache),
                "ingestibleContexts");
            var ingredientContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IIngredient, IIngredientGetter>(state.LinkCache),
                "ingredientContexts");
            var objectEffectContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IObjectEffect, IObjectEffectGetter>(state.LinkCache),
                "objectEffectContexts");
            var packageContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IPackage, IPackageGetter>(state.LinkCache),
                "packageContexts");
            var perkContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IPerk, IPerkGetter>(state.LinkCache),
                "perkContexts");
            var raceContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IRace, IRaceGetter>(state.LinkCache),
                "raceContexts");
            var regionContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IRegion, IRegionGetter>(state.LinkCache),
                "regionContexts");
            var worldspaceContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IWorldspace, IWorldspaceGetter>(state.LinkCache),
                "worldspaceContexts");
            var dialogTopicContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IDialogTopic, IDialogTopicGetter>(state.LinkCache),
                "dialogTopicContexts");
            var dialogResponseContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IDialogResponses, IDialogResponsesGetter>(state.LinkCache),
                "dialogResponseContexts");
            var formListContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IFormList, IFormListGetter>(state.LinkCache),
                "formListContexts");
            var soundDescriptorContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ISoundDescriptor, ISoundDescriptorGetter>(state.LinkCache),
                "soundDescriptorContexts");
            var effectShaderContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IEffectShader, IEffectShaderGetter>(state.LinkCache),
                "effectShaderContexts");
            var armorAddonContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IArmorAddon, IArmorAddonGetter>(state.LinkCache),
                "armorAddonContexts");
            var armorContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IArmor, IArmorGetter>(state.LinkCache),
                "armorContexts");
            var ammunitionContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IAmmunition, IAmmunitionGetter>(state.LinkCache),
                "ammunitionContexts");
            var bookContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IBook, IBookGetter>(state.LinkCache),
                "bookContexts");
            var spellContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ISpell, ISpellGetter>(state.LinkCache),
                "spellContexts");
            var locationContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ILocation, ILocationGetter>(state.LinkCache),
                "locationContexts");
            var factionContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IFaction, IFactionGetter>(state.LinkCache),
                "factionContexts");
            var encounterZoneContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IEncounterZone, IEncounterZoneGetter>(state.LinkCache),
                "encounterZoneContexts");
            var activatorContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IActivator, IActivatorGetter>(state.LinkCache),
                "activatorContexts");
            var lightContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ILight, ILightGetter>(state.LinkCache),
                "lightContexts");
            var magicEffectContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IMagicEffect, IMagicEffectGetter>(state.LinkCache),
                "magicEffectContexts");
            var projectileContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IProjectile, IProjectileGetter>(state.LinkCache),
                "projectileContexts");
            var questContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IQuest, IQuestGetter>(state.LinkCache),
                "questContexts");
            var textureSetContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ITextureSet, ITextureSetGetter>(state.LinkCache),
                "textureSetContexts");
            var miscItemContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IMiscItem, IMiscItemGetter>(state.LinkCache),
                "miscItemContexts");
            var keyContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IKey, IKeyGetter>(state.LinkCache),
                "keyContexts");
            var staticContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IStatic, IStaticGetter>(state.LinkCache),
                "staticContexts");
            var leveledItemContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ILeveledItem, ILeveledItemGetter>(state.LinkCache),
                "leveledItemContexts");
            var actionRecordContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IActionRecord, IActionRecordGetter>(state.LinkCache),
                "actionRecordContexts");
            var addonNodeContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IAddonNode, IAddonNodeGetter>(state.LinkCache),
                "addonNodeContexts");
            var animatedObjectContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IAnimatedObject, IAnimatedObjectGetter>(state.LinkCache),
                "animatedObjectContexts");
            var alchemicalApparatusContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IAlchemicalApparatus, IAlchemicalApparatusGetter>(state.LinkCache),
                "alchemicalApparatusContexts");
            var artObjectContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IArtObject, IArtObjectGetter>(state.LinkCache),
                "artObjectContexts");
            var acousticSpaceContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IAcousticSpace, IAcousticSpaceGetter>(state.LinkCache),
                "acousticSpaceContexts");
            var associationTypeContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IAssociationType, IAssociationTypeGetter>(state.LinkCache),
                "associationTypeContexts");
            var actorValueInformationContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IActorValueInformation, IActorValueInformationGetter>(state.LinkCache),
                "actorValueInformationContexts");
            var bodyPartDataContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IBodyPartData, IBodyPartDataGetter>(state.LinkCache),
                "bodyPartDataContexts");
            var cameraShotContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ICameraShot, ICameraShotGetter>(state.LinkCache),
                "cameraShotContexts");
            var classContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IClass, IClassGetter>(state.LinkCache),
                "classContexts");
            var colorRecordContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IColorRecord, IColorRecordGetter>(state.LinkCache),
                "colorRecordContexts");
            var climateContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IClimate, IClimateGetter>(state.LinkCache),
                "climateContexts");
            var constructibleObjectContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IConstructibleObject, IConstructibleObjectGetter>(state.LinkCache),
                "constructibleObjectContexts");
            var collisionLayerContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ICollisionLayer, ICollisionLayerGetter>(state.LinkCache),
                "collisionLayerContexts");
            var cameraPathContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ICameraPath, ICameraPathGetter>(state.LinkCache),
                "cameraPathContexts");
            var combatStyleContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ICombatStyle, ICombatStyleGetter>(state.LinkCache),
                "combatStyleContexts");
            var debrisContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IDebris, IDebrisGetter>(state.LinkCache),
                "debrisContexts");
            var dialogBranchContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IDialogBranch, IDialogBranchGetter>(state.LinkCache),
                "dialogBranchContexts");
            var dialogViewContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IDialogView, IDialogViewGetter>(state.LinkCache),
                "dialogViewContexts");
            var doorContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IDoor, IDoorGetter>(state.LinkCache),
                "doorContexts");
            var dualCastDataContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IDualCastData, IDualCastDataGetter>(state.LinkCache),
                "dualCastDataContexts");
            var equipTypeContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IEquipType, IEquipTypeGetter>(state.LinkCache),
                "equipTypeContexts");
            var explosionContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IExplosion, IExplosionGetter>(state.LinkCache),
                "explosionContexts");
            var eyesContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IEyes, IEyesGetter>(state.LinkCache),
                "eyesContexts");
            var floraContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IFlora, IFloraGetter>(state.LinkCache),
                "floraContexts");
            var footstepContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IFootstep, IFootstepGetter>(state.LinkCache),
                "footstepContexts");
            var footstepSetContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IFootstepSet, IFootstepSetGetter>(state.LinkCache),
                "footstepSetContexts");
            var furnitureContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IFurniture, IFurnitureGetter>(state.LinkCache),
                "furnitureContexts");
            // Global and GameSetting subtypes share a single Mutagen registration.
            // Querying a concrete subtype directly can make the generated enumerator cast a
            // sibling overlay before it has a chance to filter it. Query the common base once,
            // then narrow only contexts whose runtime record implements the requested subtype.
            var globalContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IGlobal, IGlobalGetter>(state.LinkCache),
                "globalContexts");
            var globalIntContexts = NarrowContexts<IGlobal, IGlobalGetter, IGlobalInt, IGlobalIntGetter>(globalContexts);
            var globalShortContexts = NarrowContexts<IGlobal, IGlobalGetter, IGlobalShort, IGlobalShortGetter>(globalContexts);
            var globalFloatContexts = NarrowContexts<IGlobal, IGlobalGetter, IGlobalFloat, IGlobalFloatGetter>(globalContexts);
            var globalUnknownContexts = NarrowContexts<IGlobal, IGlobalGetter, IGlobalUnknown, IGlobalUnknownGetter>(globalContexts);

            var gameSettingContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IGameSetting, IGameSettingGetter>(state.LinkCache),
                "gameSettingContexts");
            var gameSettingIntContexts = NarrowContexts<IGameSetting, IGameSettingGetter, IGameSettingInt, IGameSettingIntGetter>(gameSettingContexts);
            var gameSettingFloatContexts = NarrowContexts<IGameSetting, IGameSettingGetter, IGameSettingFloat, IGameSettingFloatGetter>(gameSettingContexts);
            var gameSettingStringContexts = NarrowContexts<IGameSetting, IGameSettingGetter, IGameSettingString, IGameSettingStringGetter>(gameSettingContexts);
            var gameSettingBoolContexts = NarrowContexts<IGameSetting, IGameSettingGetter, IGameSettingBool, IGameSettingBoolGetter>(gameSettingContexts);
            var grassContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IGrass, IGrassGetter>(state.LinkCache),
                "grassContexts");
            var hazardContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IHazard, IHazardGetter>(state.LinkCache),
                "hazardContexts");
            var headPartContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IHeadPart, IHeadPartGetter>(state.LinkCache),
                "headPartContexts");
            var idleAnimationContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IIdleAnimation, IIdleAnimationGetter>(state.LinkCache),
                "idleAnimationContexts");
            var idleMarkerContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IIdleMarker, IIdleMarkerGetter>(state.LinkCache),
                "idleMarkerContexts");
            var lightingTemplateContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ILightingTemplate, ILightingTemplateGetter>(state.LinkCache),
                "lightingTemplateContexts");
            var loadScreenContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ILoadScreen, ILoadScreenGetter>(state.LinkCache),
                "loadScreenContexts");
            var leveledNpcContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ILeveledNpc, ILeveledNpcGetter>(state.LinkCache),
                "leveledNpcContexts");
            var leveledSpellContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ILeveledSpell, ILeveledSpellGetter>(state.LinkCache),
                "leveledSpellContexts");
            var moveableStaticContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IMoveableStatic, IMoveableStaticGetter>(state.LinkCache),
                "moveableStaticContexts");
            var movementTypeContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IMovementType, IMovementTypeGetter>(state.LinkCache),
                "movementTypeContexts");
            var musicTypeContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IMusicType, IMusicTypeGetter>(state.LinkCache),
                "musicTypeContexts");
            var musicTrackContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IMusicTrack, IMusicTrackGetter>(state.LinkCache),
                "musicTrackContexts");
            var navigationMeshContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, INavigationMesh, INavigationMeshGetter>(state.LinkCache),
                "navigationMeshContexts");
            var outfitContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IOutfit, IOutfitGetter>(state.LinkCache),
                "outfitContexts");
            // PlacedHazard shares APlacedTrap's registration with several sibling placed
            // projectile types, so use the same base-query-then-narrow pattern here.
            var placedTrapContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IAPlacedTrap, IAPlacedTrapGetter>(state.LinkCache),
                "placedTrapContexts");
            var placedHazardContexts = NarrowContexts<IAPlacedTrap, IAPlacedTrapGetter, IPlacedHazard, IPlacedHazardGetter>(placedTrapContexts);
            var soundCategoryContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ISoundCategory, ISoundCategoryGetter>(state.LinkCache),
                "soundCategoryContexts");
            var soundOutputModelContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ISoundOutputModel, ISoundOutputModelGetter>(state.LinkCache),
                "soundOutputModelContexts");
            var soundMarkerContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ISoundMarker, ISoundMarkerGetter>(state.LinkCache),
                "soundMarkerContexts");
            var shaderParticleGeometryContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IShaderParticleGeometry, IShaderParticleGeometryGetter>(state.LinkCache),
                "shaderParticleGeometryContexts");
            var sceneContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IScene, ISceneGetter>(state.LinkCache),
                "sceneContexts");
            var scrollContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IScroll, IScrollGetter>(state.LinkCache),
                "scrollContexts");
            var shoutContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IShout, IShoutGetter>(state.LinkCache),
                "shoutContexts");
            var soulGemContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ISoulGem, ISoulGemGetter>(state.LinkCache),
                "soulGemContexts");
            var storyManagerBranchNodeContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IStoryManagerBranchNode, IStoryManagerBranchNodeGetter>(state.LinkCache),
                "storyManagerBranchNodeContexts");
            var storyManagerEventNodeContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IStoryManagerEventNode, IStoryManagerEventNodeGetter>(state.LinkCache),
                "storyManagerEventNodeContexts");
            var storyManagerQuestNodeContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IStoryManagerQuestNode, IStoryManagerQuestNodeGetter>(state.LinkCache),
                "storyManagerQuestNodeContexts");
            var treeContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ITree, ITreeGetter>(state.LinkCache),
                "treeContexts");
            var voiceTypeContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IVoiceType, IVoiceTypeGetter>(state.LinkCache),
                "voiceTypeContexts");
            var waterContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IWater, IWaterGetter>(state.LinkCache),
                "waterContexts");
            var weatherContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IWeather, IWeatherGetter>(state.LinkCache),
                "weatherContexts");
            var wordOfPowerContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IWordOfPower, IWordOfPowerGetter>(state.LinkCache),
                "wordOfPowerContexts");
            var relationshipContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IRelationship, IRelationshipGetter>(state.LinkCache),
                "relationshipContexts");
            var reverbParametersContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IReverbParameters, IReverbParametersGetter>(state.LinkCache),
                "reverbParametersContexts");
            var visualEffectContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IVisualEffect, IVisualEffectGetter>(state.LinkCache),
                "visualEffectContexts");
            var materialObjectContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IMaterialObject, IMaterialObjectGetter>(state.LinkCache),
                "materialObjectContexts");
            var materialTypeContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IMaterialType, IMaterialTypeGetter>(state.LinkCache),
                "materialTypeContexts");
            var messageContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IMessage, IMessageGetter>(state.LinkCache),
                "messageContexts");
            var keywordContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IKeyword, IKeywordGetter>(state.LinkCache),
                "keywordContexts");
            var locationReferenceTypeContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ILocationReferenceType, ILocationReferenceTypeGetter>(state.LinkCache),
                "locationReferenceTypeContexts");
            var imageSpaceContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IImageSpace, IImageSpaceGetter>(state.LinkCache),
                "imageSpaceContexts");
            var impactContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IImpact, IImpactGetter>(state.LinkCache),
                "impactContexts");
            var impactDataSetContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IImpactDataSet, IImpactDataSetGetter>(state.LinkCache),
                "impactDataSetContexts");
            var talkingActivatorContexts = LoadContextsSafely(
                state.LoadOrder.PriorityOrder.WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, ITalkingActivator, ITalkingActivatorGetter>(state.LinkCache),
                "talkingActivatorContexts");

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
            Console.WriteLine("Filtering Packages...");
            var filteredPackageContexts = packageContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Package contexts: {packageContexts.Length} -> {filteredPackageContexts.Length} (filtered: {packageContexts.Length - filteredPackageContexts.Length})");
            Console.WriteLine("Filtering Perks...");
            var filteredPerkContexts = perkContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Perk contexts: {perkContexts.Length} -> {filteredPerkContexts.Length} (filtered: {perkContexts.Length - filteredPerkContexts.Length})");
            Console.WriteLine("Filtering Races...");
            var filteredRaceContexts = raceContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Race contexts: {raceContexts.Length} -> {filteredRaceContexts.Length} (filtered: {raceContexts.Length - filteredRaceContexts.Length})");
            Console.WriteLine("Filtering Regions...");
            var filteredRegionContexts = regionContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Region contexts: {regionContexts.Length} -> {filteredRegionContexts.Length} (filtered: {regionContexts.Length - filteredRegionContexts.Length})");
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
            Console.WriteLine("Filtering Armors...");
            var filteredArmorContexts = armorContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Armor contexts: {armorContexts.Length} -> {filteredArmorContexts.Length} (filtered: {armorContexts.Length - filteredArmorContexts.Length})");
            Console.WriteLine("Filtering Ammunition...");
            var filteredAmmunitionContexts = ammunitionContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Ammunition contexts: {ammunitionContexts.Length} -> {filteredAmmunitionContexts.Length} (filtered: {ammunitionContexts.Length - filteredAmmunitionContexts.Length})");
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
            Console.WriteLine("Filtering Projectiles...");
            var filteredProjectileContexts = projectileContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Projectile contexts: {projectileContexts.Length} -> {filteredProjectileContexts.Length} (filtered: {projectileContexts.Length - filteredProjectileContexts.Length})");
            Console.WriteLine("Filtering Quests...");
            var filteredQuestContexts = questContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Quest contexts: {questContexts.Length} -> {filteredQuestContexts.Length} (filtered: {questContexts.Length - filteredQuestContexts.Length})");
            Console.WriteLine("Filtering Texture Sets...");
            var filteredTextureSetContexts = textureSetContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Texture Set contexts: {textureSetContexts.Length} -> {filteredTextureSetContexts.Length} (filtered: {textureSetContexts.Length - filteredTextureSetContexts.Length})");
            Console.WriteLine("Filtering Misc Items...");
            var filteredMiscItemContexts = miscItemContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Misc Item contexts: {miscItemContexts.Length} -> {filteredMiscItemContexts.Length} (filtered: {miscItemContexts.Length - filteredMiscItemContexts.Length})");
            Console.WriteLine("Filtering Keys...");
            var filteredKeyContexts = keyContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Key contexts: {keyContexts.Length} -> {filteredKeyContexts.Length} (filtered: {keyContexts.Length - filteredKeyContexts.Length})");
            Console.WriteLine("Filtering Statics...");
            var filteredStaticContexts = staticContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Static contexts: {staticContexts.Length} -> {filteredStaticContexts.Length} (filtered: {staticContexts.Length - filteredStaticContexts.Length})");
            Console.WriteLine("Filtering Leveled Items...");
            var filteredLeveledItemContexts = leveledItemContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Leveled Item contexts: {leveledItemContexts.Length} -> {filteredLeveledItemContexts.Length} (filtered: {leveledItemContexts.Length - filteredLeveledItemContexts.Length})");
            Console.WriteLine("Filtering Action Records...");
            var filteredActionRecordContexts = actionRecordContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Action Record contexts: {actionRecordContexts.Length} -> {filteredActionRecordContexts.Length} (filtered: {actionRecordContexts.Length - filteredActionRecordContexts.Length})");
            Console.WriteLine("Filtering Addon Nodes...");
            var filteredAddonNodeContexts = addonNodeContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Addon Node contexts: {addonNodeContexts.Length} -> {filteredAddonNodeContexts.Length} (filtered: {addonNodeContexts.Length - filteredAddonNodeContexts.Length})");
            Console.WriteLine("Filtering Animated Objects...");
            var filteredAnimatedObjectContexts = animatedObjectContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Animated Object contexts: {animatedObjectContexts.Length} -> {filteredAnimatedObjectContexts.Length} (filtered: {animatedObjectContexts.Length - filteredAnimatedObjectContexts.Length})");
            Console.WriteLine("Filtering Alchemical Apparatus...");
            var filteredAlchemicalApparatusContexts = alchemicalApparatusContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Alchemical Apparatus contexts: {alchemicalApparatusContexts.Length} -> {filteredAlchemicalApparatusContexts.Length} (filtered: {alchemicalApparatusContexts.Length - filteredAlchemicalApparatusContexts.Length})");
            Console.WriteLine("Filtering Art Objects...");
            var filteredArtObjectContexts = artObjectContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Art Object contexts: {artObjectContexts.Length} -> {filteredArtObjectContexts.Length} (filtered: {artObjectContexts.Length - filteredArtObjectContexts.Length})");
            Console.WriteLine("Filtering Acoustic Spaces...");
            var filteredAcousticSpaceContexts = acousticSpaceContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Acoustic Space contexts: {acousticSpaceContexts.Length} -> {filteredAcousticSpaceContexts.Length} (filtered: {acousticSpaceContexts.Length - filteredAcousticSpaceContexts.Length})");
            Console.WriteLine("Filtering Association Types...");
            var filteredAssociationTypeContexts = associationTypeContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Association Type contexts: {associationTypeContexts.Length} -> {filteredAssociationTypeContexts.Length} (filtered: {associationTypeContexts.Length - filteredAssociationTypeContexts.Length})");
            Console.WriteLine("Filtering Actor Value Information...");
            var filteredActorValueInformationContexts = actorValueInformationContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Actor Value Information contexts: {actorValueInformationContexts.Length} -> {filteredActorValueInformationContexts.Length} (filtered: {actorValueInformationContexts.Length - filteredActorValueInformationContexts.Length})");
            Console.WriteLine("Filtering Body Part Data...");
            var filteredBodyPartDataContexts = bodyPartDataContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Body Part Data contexts: {bodyPartDataContexts.Length} -> {filteredBodyPartDataContexts.Length} (filtered: {bodyPartDataContexts.Length - filteredBodyPartDataContexts.Length})");
            Console.WriteLine("Filtering Camera Shots...");
            var filteredCameraShotContexts = cameraShotContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Camera Shot contexts: {cameraShotContexts.Length} -> {filteredCameraShotContexts.Length} (filtered: {cameraShotContexts.Length - filteredCameraShotContexts.Length})");
            Console.WriteLine("Filtering Classes...");
            var filteredClassContexts = classContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Class contexts: {classContexts.Length} -> {filteredClassContexts.Length} (filtered: {classContexts.Length - filteredClassContexts.Length})");
            Console.WriteLine("Filtering Color Records...");
            var filteredColorRecordContexts = colorRecordContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Color Record contexts: {colorRecordContexts.Length} -> {filteredColorRecordContexts.Length} (filtered: {colorRecordContexts.Length - filteredColorRecordContexts.Length})");
            Console.WriteLine("Filtering Climates...");
            var filteredClimateContexts = climateContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Climate contexts: {climateContexts.Length} -> {filteredClimateContexts.Length} (filtered: {climateContexts.Length - filteredClimateContexts.Length})");
            Console.WriteLine("Filtering Constructible Objects...");
            var filteredConstructibleObjectContexts = constructibleObjectContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Constructible Object contexts: {constructibleObjectContexts.Length} -> {filteredConstructibleObjectContexts.Length} (filtered: {constructibleObjectContexts.Length - filteredConstructibleObjectContexts.Length})");
            Console.WriteLine("Filtering Collision Layers...");
            var filteredCollisionLayerContexts = collisionLayerContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Collision Layer contexts: {collisionLayerContexts.Length} -> {filteredCollisionLayerContexts.Length} (filtered: {collisionLayerContexts.Length - filteredCollisionLayerContexts.Length})");
            Console.WriteLine("Filtering Camera Paths...");
            var filteredCameraPathContexts = cameraPathContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Camera Path contexts: {cameraPathContexts.Length} -> {filteredCameraPathContexts.Length} (filtered: {cameraPathContexts.Length - filteredCameraPathContexts.Length})");
            Console.WriteLine("Filtering Combat Styles...");
            var filteredCombatStyleContexts = combatStyleContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Combat Style contexts: {combatStyleContexts.Length} -> {filteredCombatStyleContexts.Length} (filtered: {combatStyleContexts.Length - filteredCombatStyleContexts.Length})");
            Console.WriteLine("Filtering Debris...");
            var filteredDebrisContexts = debrisContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Debris contexts: {debrisContexts.Length} -> {filteredDebrisContexts.Length} (filtered: {debrisContexts.Length - filteredDebrisContexts.Length})");
            Console.WriteLine("Filtering Dialog Branches...");
            var filteredDialogBranchContexts = dialogBranchContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Dialog Branch contexts: {dialogBranchContexts.Length} -> {filteredDialogBranchContexts.Length} (filtered: {dialogBranchContexts.Length - filteredDialogBranchContexts.Length})");
            Console.WriteLine("Filtering Dialog Views...");
            var filteredDialogViewContexts = dialogViewContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Dialog View contexts: {dialogViewContexts.Length} -> {filteredDialogViewContexts.Length} (filtered: {dialogViewContexts.Length - filteredDialogViewContexts.Length})");
            Console.WriteLine("Filtering Doors...");
            var filteredDoorContexts = doorContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Door contexts: {doorContexts.Length} -> {filteredDoorContexts.Length} (filtered: {doorContexts.Length - filteredDoorContexts.Length})");
            Console.WriteLine("Filtering Dual Cast Data...");
            var filteredDualCastDataContexts = dualCastDataContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Dual Cast Data contexts: {dualCastDataContexts.Length} -> {filteredDualCastDataContexts.Length} (filtered: {dualCastDataContexts.Length - filteredDualCastDataContexts.Length})");
            Console.WriteLine("Filtering Equip Types...");
            var filteredEquipTypeContexts = equipTypeContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Equip Type contexts: {equipTypeContexts.Length} -> {filteredEquipTypeContexts.Length} (filtered: {equipTypeContexts.Length - filteredEquipTypeContexts.Length})");
            Console.WriteLine("Filtering Explosions...");
            var filteredExplosionContexts = explosionContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Explosion contexts: {explosionContexts.Length} -> {filteredExplosionContexts.Length} (filtered: {explosionContexts.Length - filteredExplosionContexts.Length})");
            Console.WriteLine("Filtering Eyes...");
            var filteredEyesContexts = eyesContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Eyes contexts: {eyesContexts.Length} -> {filteredEyesContexts.Length} (filtered: {eyesContexts.Length - filteredEyesContexts.Length})");
            Console.WriteLine("Filtering Flora...");
            var filteredFloraContexts = floraContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Flora contexts: {floraContexts.Length} -> {filteredFloraContexts.Length} (filtered: {floraContexts.Length - filteredFloraContexts.Length})");
            Console.WriteLine("Filtering Footsteps...");
            var filteredFootstepContexts = footstepContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Footstep contexts: {footstepContexts.Length} -> {filteredFootstepContexts.Length} (filtered: {footstepContexts.Length - filteredFootstepContexts.Length})");
            Console.WriteLine("Filtering Footstep Sets...");
            var filteredFootstepSetContexts = footstepSetContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Footstep Set contexts: {footstepSetContexts.Length} -> {filteredFootstepSetContexts.Length} (filtered: {footstepSetContexts.Length - filteredFootstepSetContexts.Length})");
            Console.WriteLine("Filtering Furniture...");
            var filteredFurnitureContexts = furnitureContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Furniture contexts: {furnitureContexts.Length} -> {filteredFurnitureContexts.Length} (filtered: {furnitureContexts.Length - filteredFurnitureContexts.Length})");
            Console.WriteLine("Filtering Global Ints...");
            var filteredGlobalIntContexts = globalIntContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Global Int contexts: {globalIntContexts.Length} -> {filteredGlobalIntContexts.Length} (filtered: {globalIntContexts.Length - filteredGlobalIntContexts.Length})");
            Console.WriteLine("Filtering Global Shorts...");
            var filteredGlobalShortContexts = globalShortContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Global Short contexts: {globalShortContexts.Length} -> {filteredGlobalShortContexts.Length} (filtered: {globalShortContexts.Length - filteredGlobalShortContexts.Length})");
            Console.WriteLine("Filtering Global Floats...");
            var filteredGlobalFloatContexts = globalFloatContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Global Float contexts: {globalFloatContexts.Length} -> {filteredGlobalFloatContexts.Length} (filtered: {globalFloatContexts.Length - filteredGlobalFloatContexts.Length})");
            Console.WriteLine("Filtering Global Unknowns...");
            var filteredGlobalUnknownContexts = globalUnknownContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Global Unknown contexts: {globalUnknownContexts.Length} -> {filteredGlobalUnknownContexts.Length} (filtered: {globalUnknownContexts.Length - filteredGlobalUnknownContexts.Length})");
            Console.WriteLine("Filtering Game Setting Ints...");
            var filteredGameSettingIntContexts = gameSettingIntContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Game Setting Int contexts: {gameSettingIntContexts.Length} -> {filteredGameSettingIntContexts.Length} (filtered: {gameSettingIntContexts.Length - filteredGameSettingIntContexts.Length})");
            Console.WriteLine("Filtering Game Setting Floats...");
            var filteredGameSettingFloatContexts = gameSettingFloatContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Game Setting Float contexts: {gameSettingFloatContexts.Length} -> {filteredGameSettingFloatContexts.Length} (filtered: {gameSettingFloatContexts.Length - filteredGameSettingFloatContexts.Length})");
            Console.WriteLine("Filtering Game Setting Strings...");
            var filteredGameSettingStringContexts = gameSettingStringContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Game Setting String contexts: {gameSettingStringContexts.Length} -> {filteredGameSettingStringContexts.Length} (filtered: {gameSettingStringContexts.Length - filteredGameSettingStringContexts.Length})");
            Console.WriteLine("Filtering Game Setting Bools...");
            var filteredGameSettingBoolContexts = gameSettingBoolContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Game Setting Bool contexts: {gameSettingBoolContexts.Length} -> {filteredGameSettingBoolContexts.Length} (filtered: {gameSettingBoolContexts.Length - filteredGameSettingBoolContexts.Length})");
            Console.WriteLine("Filtering Grass...");
            var filteredGrassContexts = grassContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Grass contexts: {grassContexts.Length} -> {filteredGrassContexts.Length} (filtered: {grassContexts.Length - filteredGrassContexts.Length})");
            Console.WriteLine("Filtering Hazards...");
            var filteredHazardContexts = hazardContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Hazard contexts: {hazardContexts.Length} -> {filteredHazardContexts.Length} (filtered: {hazardContexts.Length - filteredHazardContexts.Length})");
            Console.WriteLine("Filtering Head Parts...");
            var filteredHeadPartContexts = headPartContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Head Part contexts: {headPartContexts.Length} -> {filteredHeadPartContexts.Length} (filtered: {headPartContexts.Length - filteredHeadPartContexts.Length})");
            Console.WriteLine("Filtering Idle Animations...");
            var filteredIdleAnimationContexts = idleAnimationContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Idle Animation contexts: {idleAnimationContexts.Length} -> {filteredIdleAnimationContexts.Length} (filtered: {idleAnimationContexts.Length - filteredIdleAnimationContexts.Length})");
            Console.WriteLine("Filtering Idle Markers...");
            var filteredIdleMarkerContexts = idleMarkerContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Idle Marker contexts: {idleMarkerContexts.Length} -> {filteredIdleMarkerContexts.Length} (filtered: {idleMarkerContexts.Length - filteredIdleMarkerContexts.Length})");
            Console.WriteLine("Filtering Lighting Templates...");
            var filteredLightingTemplateContexts = lightingTemplateContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Lighting Template contexts: {lightingTemplateContexts.Length} -> {filteredLightingTemplateContexts.Length} (filtered: {lightingTemplateContexts.Length - filteredLightingTemplateContexts.Length})");
            Console.WriteLine("Filtering Load Screens...");
            var filteredLoadScreenContexts = loadScreenContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Load Screen contexts: {loadScreenContexts.Length} -> {filteredLoadScreenContexts.Length} (filtered: {loadScreenContexts.Length - filteredLoadScreenContexts.Length})");
            Console.WriteLine("Filtering Leveled NPCs...");
            var filteredLeveledNpcContexts = leveledNpcContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Leveled NPC contexts: {leveledNpcContexts.Length} -> {filteredLeveledNpcContexts.Length} (filtered: {leveledNpcContexts.Length - filteredLeveledNpcContexts.Length})");
            Console.WriteLine("Filtering Leveled Spells...");
            var filteredLeveledSpellContexts = leveledSpellContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Leveled Spell contexts: {leveledSpellContexts.Length} -> {filteredLeveledSpellContexts.Length} (filtered: {leveledSpellContexts.Length - filteredLeveledSpellContexts.Length})");
            Console.WriteLine("Filtering Moveable Statics...");
            var filteredMoveableStaticContexts = moveableStaticContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Moveable Static contexts: {moveableStaticContexts.Length} -> {filteredMoveableStaticContexts.Length} (filtered: {moveableStaticContexts.Length - filteredMoveableStaticContexts.Length})");
            Console.WriteLine("Filtering Movement Types...");
            var filteredMovementTypeContexts = movementTypeContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Movement Type contexts: {movementTypeContexts.Length} -> {filteredMovementTypeContexts.Length} (filtered: {movementTypeContexts.Length - filteredMovementTypeContexts.Length})");
            Console.WriteLine("Filtering Music Types...");
            var filteredMusicTypeContexts = musicTypeContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Music Type contexts: {musicTypeContexts.Length} -> {filteredMusicTypeContexts.Length} (filtered: {musicTypeContexts.Length - filteredMusicTypeContexts.Length})");
            Console.WriteLine("Filtering Music Tracks...");
            var filteredMusicTrackContexts = musicTrackContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Music Track contexts: {musicTrackContexts.Length} -> {filteredMusicTrackContexts.Length} (filtered: {musicTrackContexts.Length - filteredMusicTrackContexts.Length})");
            Console.WriteLine("Filtering Navigation Meshes...");
            var filteredNavigationMeshContexts = navigationMeshContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Navigation Mesh contexts: {navigationMeshContexts.Length} -> {filteredNavigationMeshContexts.Length} (filtered: {navigationMeshContexts.Length - filteredNavigationMeshContexts.Length})");
            Console.WriteLine("Filtering Outfits...");
            var filteredOutfitContexts = outfitContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Outfit contexts: {outfitContexts.Length} -> {filteredOutfitContexts.Length} (filtered: {outfitContexts.Length - filteredOutfitContexts.Length})");
            Console.WriteLine("Filtering Placed Hazards...");
            var filteredPlacedHazardContexts = placedHazardContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Placed Hazard contexts: {placedHazardContexts.Length} -> {filteredPlacedHazardContexts.Length} (filtered: {placedHazardContexts.Length - filteredPlacedHazardContexts.Length})");
            Console.WriteLine("Filtering Sound Categories...");
            var filteredSoundCategoryContexts = soundCategoryContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Sound Category contexts: {soundCategoryContexts.Length} -> {filteredSoundCategoryContexts.Length} (filtered: {soundCategoryContexts.Length - filteredSoundCategoryContexts.Length})");
            Console.WriteLine("Filtering Sound Output Models...");
            var filteredSoundOutputModelContexts = soundOutputModelContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Sound Output Model contexts: {soundOutputModelContexts.Length} -> {filteredSoundOutputModelContexts.Length} (filtered: {soundOutputModelContexts.Length - filteredSoundOutputModelContexts.Length})");
            Console.WriteLine("Filtering Sound Markers...");
            var filteredSoundMarkerContexts = soundMarkerContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Sound Marker contexts: {soundMarkerContexts.Length} -> {filteredSoundMarkerContexts.Length} (filtered: {soundMarkerContexts.Length - filteredSoundMarkerContexts.Length})");
            Console.WriteLine("Filtering Shader Particle Geometry...");
            var filteredShaderParticleGeometryContexts = shaderParticleGeometryContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Shader Particle Geometry contexts: {shaderParticleGeometryContexts.Length} -> {filteredShaderParticleGeometryContexts.Length} (filtered: {shaderParticleGeometryContexts.Length - filteredShaderParticleGeometryContexts.Length})");
            Console.WriteLine("Filtering Scenes...");
            var filteredSceneContexts = sceneContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Scene contexts: {sceneContexts.Length} -> {filteredSceneContexts.Length} (filtered: {sceneContexts.Length - filteredSceneContexts.Length})");
            Console.WriteLine("Filtering Scrolls...");
            var filteredScrollContexts = scrollContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Scroll contexts: {scrollContexts.Length} -> {filteredScrollContexts.Length} (filtered: {scrollContexts.Length - filteredScrollContexts.Length})");
            Console.WriteLine("Filtering Shouts...");
            var filteredShoutContexts = shoutContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Shout contexts: {shoutContexts.Length} -> {filteredShoutContexts.Length} (filtered: {shoutContexts.Length - filteredShoutContexts.Length})");
            Console.WriteLine("Filtering Soul Gems...");
            var filteredSoulGemContexts = soulGemContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Soul Gem contexts: {soulGemContexts.Length} -> {filteredSoulGemContexts.Length} (filtered: {soulGemContexts.Length - filteredSoulGemContexts.Length})");
            Console.WriteLine("Filtering Story Manager Branch Nodes...");
            var filteredStoryManagerBranchNodeContexts = storyManagerBranchNodeContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Story Manager Branch Node contexts: {storyManagerBranchNodeContexts.Length} -> {filteredStoryManagerBranchNodeContexts.Length} (filtered: {storyManagerBranchNodeContexts.Length - filteredStoryManagerBranchNodeContexts.Length})");
            Console.WriteLine("Filtering Story Manager Event Nodes...");
            var filteredStoryManagerEventNodeContexts = storyManagerEventNodeContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Story Manager Event Node contexts: {storyManagerEventNodeContexts.Length} -> {filteredStoryManagerEventNodeContexts.Length} (filtered: {storyManagerEventNodeContexts.Length - filteredStoryManagerEventNodeContexts.Length})");
            Console.WriteLine("Filtering Story Manager Quest Nodes...");
            var filteredStoryManagerQuestNodeContexts = storyManagerQuestNodeContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Story Manager Quest Node contexts: {storyManagerQuestNodeContexts.Length} -> {filteredStoryManagerQuestNodeContexts.Length} (filtered: {storyManagerQuestNodeContexts.Length - filteredStoryManagerQuestNodeContexts.Length})");
            Console.WriteLine("Filtering Trees...");
            var filteredTreeContexts = treeContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Tree contexts: {treeContexts.Length} -> {filteredTreeContexts.Length} (filtered: {treeContexts.Length - filteredTreeContexts.Length})");
            Console.WriteLine("Filtering Voice Types...");
            var filteredVoiceTypeContexts = voiceTypeContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Voice Type contexts: {voiceTypeContexts.Length} -> {filteredVoiceTypeContexts.Length} (filtered: {voiceTypeContexts.Length - filteredVoiceTypeContexts.Length})");
            Console.WriteLine("Filtering Waters...");
            var filteredWaterContexts = waterContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Water contexts: {waterContexts.Length} -> {filteredWaterContexts.Length} (filtered: {waterContexts.Length - filteredWaterContexts.Length})");
            Console.WriteLine("Filtering Weathers...");
            var filteredWeatherContexts = weatherContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Weather contexts: {weatherContexts.Length} -> {filteredWeatherContexts.Length} (filtered: {weatherContexts.Length - filteredWeatherContexts.Length})");
            Console.WriteLine("Filtering Words Of Power...");
            var filteredWordOfPowerContexts = wordOfPowerContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Word Of Power contexts: {wordOfPowerContexts.Length} -> {filteredWordOfPowerContexts.Length} (filtered: {wordOfPowerContexts.Length - filteredWordOfPowerContexts.Length})");
            Console.WriteLine("Filtering Relationships...");
            var filteredRelationshipContexts = relationshipContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Relationship contexts: {relationshipContexts.Length} -> {filteredRelationshipContexts.Length} (filtered: {relationshipContexts.Length - filteredRelationshipContexts.Length})");
            Console.WriteLine("Filtering Reverb Parameters...");
            var filteredReverbParametersContexts = reverbParametersContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Reverb Parameters contexts: {reverbParametersContexts.Length} -> {filteredReverbParametersContexts.Length} (filtered: {reverbParametersContexts.Length - filteredReverbParametersContexts.Length})");
            Console.WriteLine("Filtering Visual Effects...");
            var filteredVisualEffectContexts = visualEffectContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Visual Effect contexts: {visualEffectContexts.Length} -> {filteredVisualEffectContexts.Length} (filtered: {visualEffectContexts.Length - filteredVisualEffectContexts.Length})");
            Console.WriteLine("Filtering Material Objects...");
            var filteredMaterialObjectContexts = materialObjectContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Material Object contexts: {materialObjectContexts.Length} -> {filteredMaterialObjectContexts.Length} (filtered: {materialObjectContexts.Length - filteredMaterialObjectContexts.Length})");
            Console.WriteLine("Filtering Material Types...");
            var filteredMaterialTypeContexts = materialTypeContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Material Type contexts: {materialTypeContexts.Length} -> {filteredMaterialTypeContexts.Length} (filtered: {materialTypeContexts.Length - filteredMaterialTypeContexts.Length})");
            Console.WriteLine("Filtering Messages...");
            var filteredMessageContexts = messageContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Message contexts: {messageContexts.Length} -> {filteredMessageContexts.Length} (filtered: {messageContexts.Length - filteredMessageContexts.Length})");
            Console.WriteLine("Filtering Keywords...");
            var filteredKeywordContexts = keywordContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Keyword contexts: {keywordContexts.Length} -> {filteredKeywordContexts.Length} (filtered: {keywordContexts.Length - filteredKeywordContexts.Length})");
            Console.WriteLine("Filtering Location Reference Types...");
            var filteredLocationReferenceTypeContexts = locationReferenceTypeContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Location Reference Type contexts: {locationReferenceTypeContexts.Length} -> {filteredLocationReferenceTypeContexts.Length} (filtered: {locationReferenceTypeContexts.Length - filteredLocationReferenceTypeContexts.Length})");
            Console.WriteLine("Filtering Image Spaces...");
            var filteredImageSpaceContexts = imageSpaceContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Image Space contexts: {imageSpaceContexts.Length} -> {filteredImageSpaceContexts.Length} (filtered: {imageSpaceContexts.Length - filteredImageSpaceContexts.Length})");
            Console.WriteLine("Filtering Impacts...");
            var filteredImpactContexts = impactContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Impact contexts: {impactContexts.Length} -> {filteredImpactContexts.Length} (filtered: {impactContexts.Length - filteredImpactContexts.Length})");
            Console.WriteLine("Filtering Impact Data Sets...");
            var filteredImpactDataSetContexts = impactDataSetContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Impact Data Set contexts: {impactDataSetContexts.Length} -> {filteredImpactDataSetContexts.Length} (filtered: {impactDataSetContexts.Length - filteredImpactDataSetContexts.Length})");
            Console.WriteLine("Filtering Talking Activators...");
            var filteredTalkingActivatorContexts = talkingActivatorContexts.Where(context => !ShouldBreakEarly(context, state)).ToArray();
            Console.WriteLine($"Talking Activator contexts: {talkingActivatorContexts.Length} -> {filteredTalkingActivatorContexts.Length} (filtered: {talkingActivatorContexts.Length - filteredTalkingActivatorContexts.Length})");


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
                        case Type t when t == typeof(IPackageGetter):
                            var packageHandler = new PackageRecordHandler();
                            packageHandler.Process(state, filteredPackageContexts);
                            break;
                        case Type t when t == typeof(IPerkGetter):
                            var perkHandler = new PerkRecordHandler();
                            perkHandler.Process(state, filteredPerkContexts);
                            break;
                        case Type t when t == typeof(IRaceGetter):
                            var raceHandler = new RaceRecordHandler();
                            raceHandler.Process(state, filteredRaceContexts);
                            break;
                        case Type t when t == typeof(IRegionGetter):
                            var regionHandler = new RegionRecordHandler();
                            regionHandler.Process(state, filteredRegionContexts);
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
                        case Type t when t == typeof(IArmorGetter):
                            var armorHandler = new ArmorRecordHandler();
                            armorHandler.Process(state, filteredArmorContexts);
                            break;
                        case Type t when t == typeof(IAmmunitionGetter):
                            var ammunitionHandler = new AmmunitionRecordHandler();
                            ammunitionHandler.Process(state, filteredAmmunitionContexts);
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
                        case Type t when t == typeof(IProjectileGetter):
                            var projectileHandler = new ProjectileRecordHandler();
                            projectileHandler.Process(state, filteredProjectileContexts);
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
                        case Type t when t == typeof(IKeyGetter):
                            var keyHandler = new KeyRecordHandler();
                            keyHandler.Process(state, filteredKeyContexts);
                            break;
                        case Type t when t == typeof(IStaticGetter):
                            var staticHandler = new StaticRecordHandler();
                            staticHandler.Process(state, filteredStaticContexts);
                            break;
                        case Type t when t == typeof(ILeveledItemGetter):
                            var leveledItemHandler = new LeveledItemRecordHandler();
                            leveledItemHandler.Process(state, filteredLeveledItemContexts);
                            break;
                        case Type t when t == typeof(IActionRecordGetter):
                            var actionRecordHandler = new ActionRecordHandler();
                            actionRecordHandler.Process(state, filteredActionRecordContexts);
                            break;
                        case Type t when t == typeof(IAddonNodeGetter):
                            var addonNodeHandler = new AddonNodeRecordHandler();
                            addonNodeHandler.Process(state, filteredAddonNodeContexts);
                            break;
                        case Type t when t == typeof(IAnimatedObjectGetter):
                            var animatedObjectHandler = new AnimatedObjectRecordHandler();
                            animatedObjectHandler.Process(state, filteredAnimatedObjectContexts);
                            break;
                        case Type t when t == typeof(IAlchemicalApparatusGetter):
                            var alchemicalApparatusHandler = new AlchemicalApparatusRecordHandler();
                            alchemicalApparatusHandler.Process(state, filteredAlchemicalApparatusContexts);
                            break;
                        case Type t when t == typeof(IArtObjectGetter):
                            var artObjectHandler = new ArtObjectRecordHandler();
                            artObjectHandler.Process(state, filteredArtObjectContexts);
                            break;
                        case Type t when t == typeof(IAcousticSpaceGetter):
                            var acousticSpaceHandler = new AcousticSpaceRecordHandler();
                            acousticSpaceHandler.Process(state, filteredAcousticSpaceContexts);
                            break;
                        case Type t when t == typeof(IAssociationTypeGetter):
                            var associationTypeHandler = new AssociationTypeRecordHandler();
                            associationTypeHandler.Process(state, filteredAssociationTypeContexts);
                            break;
                        case Type t when t == typeof(IActorValueInformationGetter):
                            var actorValueInformationHandler = new ActorValueInformationRecordHandler();
                            actorValueInformationHandler.Process(state, filteredActorValueInformationContexts);
                            break;
                        case Type t when t == typeof(IBodyPartDataGetter):
                            var bodyPartDataHandler = new BodyPartDataRecordHandler();
                            bodyPartDataHandler.Process(state, filteredBodyPartDataContexts);
                            break;
                        case Type t when t == typeof(ICameraShotGetter):
                            var cameraShotHandler = new CameraShotRecordHandler();
                            cameraShotHandler.Process(state, filteredCameraShotContexts);
                            break;
                        case Type t when t == typeof(IClassGetter):
                            var classHandler = new ClassRecordHandler();
                            classHandler.Process(state, filteredClassContexts);
                            break;
                        case Type t when t == typeof(IColorRecordGetter):
                            var colorRecordHandler = new ColorRecordHandler();
                            colorRecordHandler.Process(state, filteredColorRecordContexts);
                            break;
                        case Type t when t == typeof(IClimateGetter):
                            var climateHandler = new ClimateRecordHandler();
                            climateHandler.Process(state, filteredClimateContexts);
                            break;
                        case Type t when t == typeof(IConstructibleObjectGetter):
                            var constructibleObjectHandler = new ConstructibleObjectRecordHandler();
                            constructibleObjectHandler.Process(state, filteredConstructibleObjectContexts);
                            break;
                        case Type t when t == typeof(ICollisionLayerGetter):
                            var collisionLayerHandler = new CollisionLayerRecordHandler();
                            collisionLayerHandler.Process(state, filteredCollisionLayerContexts);
                            break;
                        case Type t when t == typeof(ICameraPathGetter):
                            var cameraPathHandler = new CameraPathRecordHandler();
                            cameraPathHandler.Process(state, filteredCameraPathContexts);
                            break;
                        case Type t when t == typeof(ICombatStyleGetter):
                            var combatStyleHandler = new CombatStyleRecordHandler();
                            combatStyleHandler.Process(state, filteredCombatStyleContexts);
                            break;
                        case Type t when t == typeof(IDebrisGetter):
                            var debrisHandler = new DebrisRecordHandler();
                            debrisHandler.Process(state, filteredDebrisContexts);
                            break;
                        case Type t when t == typeof(IDialogBranchGetter):
                            var dialogBranchHandler = new DialogBranchRecordHandler();
                            dialogBranchHandler.Process(state, filteredDialogBranchContexts);
                            break;
                        case Type t when t == typeof(IDialogViewGetter):
                            var dialogViewHandler = new DialogViewRecordHandler();
                            dialogViewHandler.Process(state, filteredDialogViewContexts);
                            break;
                        case Type t when t == typeof(IDoorGetter):
                            var doorHandler = new DoorRecordHandler();
                            doorHandler.Process(state, filteredDoorContexts);
                            break;
                        case Type t when t == typeof(IDualCastDataGetter):
                            var dualCastDataHandler = new DualCastDataRecordHandler();
                            dualCastDataHandler.Process(state, filteredDualCastDataContexts);
                            break;
                        case Type t when t == typeof(IEquipTypeGetter):
                            var equipTypeHandler = new EquipTypeRecordHandler();
                            equipTypeHandler.Process(state, filteredEquipTypeContexts);
                            break;
                        case Type t when t == typeof(IExplosionGetter):
                            var explosionHandler = new ExplosionRecordHandler();
                            explosionHandler.Process(state, filteredExplosionContexts);
                            break;
                        case Type t when t == typeof(IEyesGetter):
                            var eyesHandler = new EyesRecordHandler();
                            eyesHandler.Process(state, filteredEyesContexts);
                            break;
                        case Type t when t == typeof(IFloraGetter):
                            var floraHandler = new FloraRecordHandler();
                            floraHandler.Process(state, filteredFloraContexts);
                            break;
                        case Type t when t == typeof(IFootstepGetter):
                            var footstepHandler = new FootstepRecordHandler();
                            footstepHandler.Process(state, filteredFootstepContexts);
                            break;
                        case Type t when t == typeof(IFootstepSetGetter):
                            var footstepSetHandler = new FootstepSetRecordHandler();
                            footstepSetHandler.Process(state, filteredFootstepSetContexts);
                            break;
                        case Type t when t == typeof(IFurnitureGetter):
                            var furnitureHandler = new FurnitureRecordHandler();
                            furnitureHandler.Process(state, filteredFurnitureContexts);
                            break;
                        case Type t when t == typeof(IGlobalIntGetter):
                            var globalIntHandler = new GlobalIntRecordHandler();
                            globalIntHandler.Process(state, filteredGlobalIntContexts);
                            break;
                        case Type t when t == typeof(IGlobalShortGetter):
                            var globalShortHandler = new GlobalShortRecordHandler();
                            globalShortHandler.Process(state, filteredGlobalShortContexts);
                            break;
                        case Type t when t == typeof(IGlobalFloatGetter):
                            var globalFloatHandler = new GlobalFloatRecordHandler();
                            globalFloatHandler.Process(state, filteredGlobalFloatContexts);
                            break;
                        case Type t when t == typeof(IGlobalUnknownGetter):
                            var globalUnknownHandler = new GlobalUnknownRecordHandler();
                            globalUnknownHandler.Process(state, filteredGlobalUnknownContexts);
                            break;
                        case Type t when t == typeof(IGameSettingIntGetter):
                            var gameSettingIntHandler = new GameSettingIntRecordHandler();
                            gameSettingIntHandler.Process(state, filteredGameSettingIntContexts);
                            break;
                        case Type t when t == typeof(IGameSettingFloatGetter):
                            var gameSettingFloatHandler = new GameSettingFloatRecordHandler();
                            gameSettingFloatHandler.Process(state, filteredGameSettingFloatContexts);
                            break;
                        case Type t when t == typeof(IGameSettingStringGetter):
                            var gameSettingStringHandler = new GameSettingStringRecordHandler();
                            gameSettingStringHandler.Process(state, filteredGameSettingStringContexts);
                            break;
                        case Type t when t == typeof(IGameSettingBoolGetter):
                            var gameSettingBoolHandler = new GameSettingBoolRecordHandler();
                            gameSettingBoolHandler.Process(state, filteredGameSettingBoolContexts);
                            break;
                        case Type t when t == typeof(IGrassGetter):
                            var grassHandler = new GrassRecordHandler();
                            grassHandler.Process(state, filteredGrassContexts);
                            break;
                        case Type t when t == typeof(IHazardGetter):
                            var hazardHandler = new HazardRecordHandler();
                            hazardHandler.Process(state, filteredHazardContexts);
                            break;
                        case Type t when t == typeof(IHeadPartGetter):
                            var headPartHandler = new HeadPartRecordHandler();
                            headPartHandler.Process(state, filteredHeadPartContexts);
                            break;
                        case Type t when t == typeof(IIdleAnimationGetter):
                            var idleAnimationHandler = new IdleAnimationRecordHandler();
                            idleAnimationHandler.Process(state, filteredIdleAnimationContexts);
                            break;
                        case Type t when t == typeof(IIdleMarkerGetter):
                            var idleMarkerHandler = new IdleMarkerRecordHandler();
                            idleMarkerHandler.Process(state, filteredIdleMarkerContexts);
                            break;
                        case Type t when t == typeof(ILightingTemplateGetter):
                            var lightingTemplateHandler = new LightingTemplateRecordHandler();
                            lightingTemplateHandler.Process(state, filteredLightingTemplateContexts);
                            break;
                        case Type t when t == typeof(ILoadScreenGetter):
                            var loadScreenHandler = new LoadScreenRecordHandler();
                            loadScreenHandler.Process(state, filteredLoadScreenContexts);
                            break;
                        case Type t when t == typeof(ILeveledNpcGetter):
                            var leveledNpcHandler = new LeveledNpcRecordHandler();
                            leveledNpcHandler.Process(state, filteredLeveledNpcContexts);
                            break;
                        case Type t when t == typeof(ILeveledSpellGetter):
                            var leveledSpellHandler = new LeveledSpellRecordHandler();
                            leveledSpellHandler.Process(state, filteredLeveledSpellContexts);
                            break;
                        case Type t when t == typeof(IMoveableStaticGetter):
                            var moveableStaticHandler = new MoveableStaticRecordHandler();
                            moveableStaticHandler.Process(state, filteredMoveableStaticContexts);
                            break;
                        case Type t when t == typeof(IMovementTypeGetter):
                            var movementTypeHandler = new MovementTypeRecordHandler();
                            movementTypeHandler.Process(state, filteredMovementTypeContexts);
                            break;
                        case Type t when t == typeof(IMusicTypeGetter):
                            var musicTypeHandler = new MusicTypeRecordHandler();
                            musicTypeHandler.Process(state, filteredMusicTypeContexts);
                            break;
                        case Type t when t == typeof(IMusicTrackGetter):
                            var musicTrackHandler = new MusicTrackRecordHandler();
                            musicTrackHandler.Process(state, filteredMusicTrackContexts);
                            break;
                        case Type t when t == typeof(INavigationMeshGetter):
                            var navigationMeshHandler = new NavigationMeshRecordHandler();
                            navigationMeshHandler.Process(state, filteredNavigationMeshContexts);
                            break;
                        case Type t when t == typeof(IOutfitGetter):
                            var outfitHandler = new OutfitRecordHandler();
                            outfitHandler.Process(state, filteredOutfitContexts);
                            break;
                        case Type t when t == typeof(IPlacedHazardGetter):
                            var placedHazardHandler = new PlacedHazardRecordHandler();
                            placedHazardHandler.Process(state, filteredPlacedHazardContexts);
                            break;
                        case Type t when t == typeof(ISoundCategoryGetter):
                            var soundCategoryHandler = new SoundCategoryRecordHandler();
                            soundCategoryHandler.Process(state, filteredSoundCategoryContexts);
                            break;
                        case Type t when t == typeof(ISoundOutputModelGetter):
                            var soundOutputModelHandler = new SoundOutputModelRecordHandler();
                            soundOutputModelHandler.Process(state, filteredSoundOutputModelContexts);
                            break;
                        case Type t when t == typeof(ISoundMarkerGetter):
                            var soundMarkerHandler = new SoundMarkerRecordHandler();
                            soundMarkerHandler.Process(state, filteredSoundMarkerContexts);
                            break;
                        case Type t when t == typeof(IShaderParticleGeometryGetter):
                            var shaderParticleGeometryHandler = new ShaderParticleGeometryRecordHandler();
                            shaderParticleGeometryHandler.Process(state, filteredShaderParticleGeometryContexts);
                            break;
                        case Type t when t == typeof(ISceneGetter):
                            var sceneHandler = new SceneRecordHandler();
                            sceneHandler.Process(state, filteredSceneContexts);
                            break;
                        case Type t when t == typeof(IScrollGetter):
                            var scrollHandler = new ScrollRecordHandler();
                            scrollHandler.Process(state, filteredScrollContexts);
                            break;
                        case Type t when t == typeof(IShoutGetter):
                            var shoutHandler = new ShoutRecordHandler();
                            shoutHandler.Process(state, filteredShoutContexts);
                            break;
                        case Type t when t == typeof(ISoulGemGetter):
                            var soulGemHandler = new SoulGemRecordHandler();
                            soulGemHandler.Process(state, filteredSoulGemContexts);
                            break;
                        case Type t when t == typeof(IStoryManagerBranchNodeGetter):
                            var storyManagerBranchNodeHandler = new StoryManagerBranchNodeRecordHandler();
                            storyManagerBranchNodeHandler.Process(state, filteredStoryManagerBranchNodeContexts);
                            break;
                        case Type t when t == typeof(IStoryManagerEventNodeGetter):
                            var storyManagerEventNodeHandler = new StoryManagerEventNodeRecordHandler();
                            storyManagerEventNodeHandler.Process(state, filteredStoryManagerEventNodeContexts);
                            break;
                        case Type t when t == typeof(IStoryManagerQuestNodeGetter):
                            var storyManagerQuestNodeHandler = new StoryManagerQuestNodeRecordHandler();
                            storyManagerQuestNodeHandler.Process(state, filteredStoryManagerQuestNodeContexts);
                            break;
                        case Type t when t == typeof(ITreeGetter):
                            var treeHandler = new TreeRecordHandler();
                            treeHandler.Process(state, filteredTreeContexts);
                            break;
                        case Type t when t == typeof(IVoiceTypeGetter):
                            var voiceTypeHandler = new VoiceTypeRecordHandler();
                            voiceTypeHandler.Process(state, filteredVoiceTypeContexts);
                            break;
                        case Type t when t == typeof(IWaterGetter):
                            var waterHandler = new WaterRecordHandler();
                            waterHandler.Process(state, filteredWaterContexts);
                            break;
                        case Type t when t == typeof(IWeatherGetter):
                            var weatherHandler = new WeatherRecordHandler();
                            weatherHandler.Process(state, filteredWeatherContexts);
                            break;
                        case Type t when t == typeof(IWordOfPowerGetter):
                            var wordOfPowerHandler = new WordOfPowerRecordHandler();
                            wordOfPowerHandler.Process(state, filteredWordOfPowerContexts);
                            break;
                        case Type t when t == typeof(IRelationshipGetter):
                            var relationshipHandler = new RelationshipRecordHandler();
                            relationshipHandler.Process(state, filteredRelationshipContexts);
                            break;
                        case Type t when t == typeof(IReverbParametersGetter):
                            var reverbParametersHandler = new ReverbParametersRecordHandler();
                            reverbParametersHandler.Process(state, filteredReverbParametersContexts);
                            break;
                        case Type t when t == typeof(IVisualEffectGetter):
                            var visualEffectHandler = new VisualEffectRecordHandler();
                            visualEffectHandler.Process(state, filteredVisualEffectContexts);
                            break;
                        case Type t when t == typeof(IMaterialObjectGetter):
                            var materialObjectHandler = new MaterialObjectRecordHandler();
                            materialObjectHandler.Process(state, filteredMaterialObjectContexts);
                            break;
                        case Type t when t == typeof(IMaterialTypeGetter):
                            var materialTypeHandler = new MaterialTypeRecordHandler();
                            materialTypeHandler.Process(state, filteredMaterialTypeContexts);
                            break;
                        case Type t when t == typeof(IMessageGetter):
                            var messageHandler = new MessageRecordHandler();
                            messageHandler.Process(state, filteredMessageContexts);
                            break;
                        case Type t when t == typeof(IKeywordGetter):
                            var keywordHandler = new KeywordRecordHandler();
                            keywordHandler.Process(state, filteredKeywordContexts);
                            break;
                        case Type t when t == typeof(ILocationReferenceTypeGetter):
                            var locationReferenceTypeHandler = new LocationReferenceTypeRecordHandler();
                            locationReferenceTypeHandler.Process(state, filteredLocationReferenceTypeContexts);
                            break;
                        case Type t when t == typeof(IImageSpaceGetter):
                            var imageSpaceHandler = new ImageSpaceRecordHandler();
                            imageSpaceHandler.Process(state, filteredImageSpaceContexts);
                            break;
                        case Type t when t == typeof(IImpactGetter):
                            var impactHandler = new ImpactRecordHandler();
                            impactHandler.Process(state, filteredImpactContexts);
                            break;
                        case Type t when t == typeof(IImpactDataSetGetter):
                            var impactDataSetHandler = new ImpactDataSetRecordHandler();
                            impactDataSetHandler.Process(state, filteredImpactDataSetContexts);
                            break;
                        case Type t when t == typeof(ITalkingActivatorGetter):
                            var talkingActivatorHandler = new TalkingActivatorRecordHandler();
                            talkingActivatorHandler.Process(state, filteredTalkingActivatorContexts);
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



