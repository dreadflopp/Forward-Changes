using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Strings;
using ForwardChanges.PropertyHandlers.Npc;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Abstracts;
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
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "MajorFlags", new MajorFlagsHandler() },
            { "DeathItem", new SimpleReflectionFormLinkPropertyHandler<ILeveledItemGetter, INpc, INpcGetter>("DeathItem") },
            { "CombatOverridePackageList", new SimpleReflectionFormLinkPropertyHandler<IFormListGetter, INpc, INpcGetter>("CombatOverridePackageList") },
            { "SpectatorOverridePackageList", new SimpleReflectionFormLinkPropertyHandler<IFormListGetter, INpc, INpcGetter>("SpectatorOverridePackageList") },
            { "Configuration.Flags", new ProtectionFlagsHandler() },
            { "Configuration.MagickaOffset", new ConfigurationMagickaOffsetHandler() },
            { "EditorID", new EditorIDHandler() },
            { "Class", new SimpleReflectionFormLinkPropertyHandler<IClassGetter, INpc, INpcGetter>("Class") },
            { "AIData.Confidence", new AIDataConfidenceHandler() },
            { "ObserveDeadBodyOverridePackageList", new SimpleReflectionFormLinkPropertyHandler<IFormListGetter, INpc, INpcGetter>("ObserveDeadBodyOverridePackageList") },
            { "Factions", new FactionHandler() },
            { "Packages", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IPackageGetter>, INpc, INpcGetter>("Packages", ListOrdering.PreserveModOrder) },
            { "ActorEffect", new SimpleReflectionListPropertyHandler<IFormLinkGetter<ISpellRecordGetter>, INpc, INpcGetter>("ActorEffect") },
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
            { "Race", new SimpleReflectionFormLinkPropertyHandler<IRaceGetter, INpc, INpcGetter>("Race") },
            { "Destructible", new DestructibleHandler() },
            { "Height", new SimpleReflectionPropertyHandler<float, INpc, INpcGetter>("Height", 0.001f) },
            { "Weight", new SimpleReflectionPropertyHandler<float, INpc, INpcGetter>("Weight", 0.001f) },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "Voice", new SimpleReflectionFormLinkPropertyHandler<IVoiceTypeGetter, INpc, INpcGetter>("Voice") },
            { "Template", new SimpleReflectionFormLinkPropertyHandler<INpcSpawnGetter, INpc, INpcGetter>("Template") },
            { "ShortName", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, INpc, INpcGetter>("ShortName") },
            { "NAM5", new SimpleReflectionPropertyHandler<ushort, INpc, INpcGetter>("NAM5") },
            { "SoundLevel", new SimpleReflectionPropertyHandler<SoundLevel, INpc, INpcGetter>("SoundLevel") },
            { "HeadParts", new HeadPartsHandler() },
            { "WornArmor", new SimpleReflectionFormLinkPropertyHandler<IArmorGetter, INpc, INpcGetter>("WornArmor") },
            { "AttackRace", new SimpleReflectionFormLinkPropertyHandler<IRaceGetter, INpc, INpcGetter>("AttackRace") },
            { "HairColor", new SimpleReflectionFormLinkPropertyHandler<IColorRecordGetter, INpc, INpcGetter>("HairColor") },
            { "DefaultOutfit", new SimpleReflectionFormLinkPropertyHandler<IOutfitGetter, INpc, INpcGetter>("DefaultOutfit") },
            { "FarAwayModel", new SimpleReflectionFormLinkPropertyHandler<IArmorGetter, INpc, INpcGetter>("FarAwayModel") },
            { "Attacks", new AttacksHandler() },
            { "GuardWarnOverridePackageList", new SimpleReflectionFormLinkPropertyHandler<IFormListGetter, INpc, INpcGetter>("GuardWarnOverridePackageList") },
            { "Perks", new PerksHandler() },
            { "CombatStyle", new SimpleReflectionFormLinkPropertyHandler<ICombatStyleGetter, INpc, INpcGetter>("CombatStyle") },
            { "GiftFilter", new SimpleReflectionFormLinkPropertyHandler<IFormListGetter, INpc, INpcGetter>("GiftFilter") },
            { "SleepingOutfit", new SimpleReflectionFormLinkPropertyHandler<IOutfitGetter, INpc, INpcGetter>("SleepingOutfit") },
            { "DefaultPackageList", new SimpleReflectionFormLinkPropertyHandler<IFormListGetter, INpc, INpcGetter>("DefaultPackageList") },
            { "CrimeFaction", new SimpleReflectionFormLinkPropertyHandler<IFactionGetter, INpc, INpcGetter>("CrimeFaction") },
            { "HeadTexture", new SimpleReflectionFormLinkPropertyHandler<ITextureSetGetter, INpc, INpcGetter>("HeadTexture") },
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

        // ApplyForwardedProperties is now handled by the base class
        // The base class automatically handles flag property coordination
    }
}