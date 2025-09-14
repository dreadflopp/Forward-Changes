using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Npc;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    public class NpcRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
                { "Name", new NameHandler() },
                { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
                { "MajorFlags", new MajorFlagsHandler() },
                { "DeathItem", new DeathItemHandler() },
                { "CombatOverridePackageList", new CombatOverridePackageListHandler() },
                { "SpectatorOverridePackageList", new SpectatorOverridePackageListHandler() },
                { "Configuration.Flags", new ProtectionFlagsHandler() },
                { "Configuration.MagickaOffset", new ConfigurationMagickaOffsetHandler() },
                { "EditorID", new EditorIDHandler() },
                { "Class", new ClassHandler() },
                { "AIData.Confidence", new AIDataConfidenceHandler() },
                { "ObserveDeadBodyOverridePackageList", new ObserveDeadBodyOverridePackageListHandler() },
                { "Factions", new FactionHandler() },
                { "Packages", new PackageHandler() },
                { "ActorEffect", new ActorEffectsHandler() },
                { "VirtualMachineAdapter", new VirtualMachineAdapterHandler() },
                { "Items", new ItemHandler() },
                { "Keywords", new KeywordListHandler() },
                { "PlayerSkills.Health", new PlayerSkillsHealthHandler() },
                { "PlayerSkills.Magicka", new PlayerSkillsMagickaHandler() },
                { "PlayerSkills.Stamina", new PlayerSkillsStaminaHandler() },
                { "PlayerSkills.FarAwayModelDistance", new PlayerSkillsFarAwayModelDistanceHandler() },
                { "PlayerSkills.GearedUpWeapons", new PlayerSkillsGearedUpWeaponsHandler() },
                { "PlayerSkills.SkillValues", new PlayerSkillsSkillValuesHandler() },
                { "PlayerSkills.SkillOffsets", new PlayerSkillsSkillOffsetsHandler() },
                { "FaceMorph", new FaceMorphHandler() },
                { "FaceParts", new FacePartsHandler() },
                { "TextureLighting", new TextureLightingHandler() },
                { "TintLayers", new TintLayersHandler() },
                { "Race", new RaceHandler() },
                { "Destructible", new DestructibleHandler() },
                { "Height", new HeightHandler() },
                { "Weight", new WeightHandler() },
                { "ObjectBounds", new ObjectBoundsHandler() },
                { "Voice", new VoiceHandler() },
                { "Template", new TemplateHandler() },
                { "ShortName", new ShortNameHandler() },
                { "NAM5", new NAM5Handler() },
                { "SoundLevel", new SoundLevelHandler() },
                { "HeadParts", new HeadPartsHandler() },
                { "WornArmor", new WornArmorHandler() },
                { "AttackRace", new AttackRaceHandler() },
                { "HairColor", new HairColorHandler() },
                { "DefaultOutfit", new DefaultOutfitHandler() },
                { "FarAwayModel", new FarAwayModelHandler() },
                { "Attacks", new AttacksHandler() },
                { "GuardWarnOverridePackageList", new GuardWarnOverridePackageListHandler() },
                { "Perks", new PerksHandler() },
                { "CombatStyle", new CombatStyleHandler() },
                { "GiftFilter", new GiftFilterHandler() },
                { "SleepingOutfit", new SleepingOutfitHandler() },
                { "DefaultPackageList", new DefaultPackageListHandler() },
                { "CrimeFaction", new CrimeFactionHandler() },
                { "HeadTexture", new HeadTextureHandler() },
                { "Sound", new SoundHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            return winningContext.Record
                .ToLink<INpcGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, INpc, INpcGetter>(state.LinkCache)
                .ToArray();
        }

        public override IMajorRecord GetOverrideRecord(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            return state.PatchMod.Npcs.GetOrAddAsOverride(winningContext.Record);
        }

        public override void ApplyForwardedProperties(IMajorRecord record, Dictionary<string, object?> propertiesToForward)
        {
            foreach (var (propertyName, value) in propertiesToForward)
            {
                if (PropertyHandlers.TryGetValue(propertyName, out var handler))
                {
                    Console.WriteLine($"[{propertyName}] Applying value: {handler.FormatValue(value)}, Type: {value?.GetType()}");
                    handler.SetValue(record, value);
                }
            }
        }
    }
}