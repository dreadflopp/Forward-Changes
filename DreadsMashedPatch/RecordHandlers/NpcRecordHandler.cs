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
    // Migration note:
    // - Generalized: semantic AIData, Configuration, and scalar PlayerSkills leaves use exact dotted shared handlers.
    // - Specialized: protection policy, skill dictionaries, float tolerance, and NPC collection merging remain record-specific.
    // - Intentionally excluded: AIData.Unused and PlayerSkills.Unused* are serialization-only fields.
    // - Rationale: direct semantic leaves are reflection-safe; unused storage is not an xEdit-visible conflict surface.
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
            { "Configuration.MagickaOffset", new SimpleReflectionPropertyHandler<short, INpc, INpcGetter>("Configuration.MagickaOffset") },
            { "Configuration.StaminaOffset", new SimpleReflectionPropertyHandler<short, INpc, INpcGetter>("Configuration.StaminaOffset") },
            { "Configuration.Level", new ComplexReflectionPropertyHandler<IANpcLevelGetter, INpc, INpcGetter>("Configuration.Level") },
            { "Configuration.CalcMinLevel", new SimpleReflectionPropertyHandler<short, INpc, INpcGetter>("Configuration.CalcMinLevel") },
            { "Configuration.CalcMaxLevel", new SimpleReflectionPropertyHandler<short, INpc, INpcGetter>("Configuration.CalcMaxLevel") },
            { "Configuration.SpeedMultiplier", new SimpleReflectionPropertyHandler<short, INpc, INpcGetter>("Configuration.SpeedMultiplier") },
            { "Configuration.DispositionBase", new SimpleReflectionPropertyHandler<short, INpc, INpcGetter>("Configuration.DispositionBase") },
            { "Configuration.TemplateFlags", new SimpleReflectionFlagPropertyHandler<NpcConfiguration.TemplateFlag, INpc, INpcGetter>("Configuration.TemplateFlags", preserveUnknownBits: true) },
            { "Configuration.HealthOffset", new SimpleReflectionPropertyHandler<short, INpc, INpcGetter>("Configuration.HealthOffset") },
            { "Configuration.BleedoutOverride", new SimpleReflectionPropertyHandler<short, INpc, INpcGetter>("Configuration.BleedoutOverride") },
            { "EditorID", new EditorIDHandler() },
            { "Class", new SimpleReflectionFormLinkPropertyHandler<IClassGetter, INpc, INpcGetter>("Class") },
            { "AIData.Aggression", new SimpleReflectionPropertyHandler<Aggression, INpc, INpcGetter>("AIData.Aggression") },
            { "AIData.Confidence", new SimpleReflectionPropertyHandler<Confidence, INpc, INpcGetter>("AIData.Confidence") },
            { "AIData.EnergyLevel", new SimpleReflectionPropertyHandler<byte, INpc, INpcGetter>("AIData.EnergyLevel") },
            { "AIData.Responsibility", new SimpleReflectionPropertyHandler<Responsibility, INpc, INpcGetter>("AIData.Responsibility") },
            { "AIData.Mood", new SimpleReflectionPropertyHandler<Mood, INpc, INpcGetter>("AIData.Mood") },
            { "AIData.Assistance", new SimpleReflectionPropertyHandler<Assistance, INpc, INpcGetter>("AIData.Assistance") },
            { "AIData.AggroRadiusBehavior", new SimpleReflectionPropertyHandler<bool, INpc, INpcGetter>("AIData.AggroRadiusBehavior") },
            { "AIData.Warn", new SimpleReflectionPropertyHandler<uint, INpc, INpcGetter>("AIData.Warn") },
            { "AIData.WarnOrAttack", new SimpleReflectionPropertyHandler<uint, INpc, INpcGetter>("AIData.WarnOrAttack") },
            { "AIData.Attack", new SimpleReflectionPropertyHandler<uint, INpc, INpcGetter>("AIData.Attack") },
            { "ObserveDeadBodyOverridePackageList", new SimpleReflectionFormLinkPropertyHandler<IFormListGetter, INpc, INpcGetter>("ObserveDeadBodyOverridePackageList") },
            { "Factions", new FactionHandler() },
            { "Packages", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IPackageGetter>, INpc, INpcGetter>("Packages", ListSemantics.AlignedOrdered) },
            { "ActorEffect", new SimpleReflectionListPropertyHandler<IFormLinkGetter<ISpellRecordGetter>, INpc, INpcGetter>("ActorEffect", ListSemantics.SortedKeyed) },
            { "VirtualMachineAdapter", new VirtualMachineAdapterHandler() },
            { "Items", new ItemHandler() },
            { "Keywords", new KeywordListHandler() },
            { "PlayerSkills.Health", new SimpleReflectionPropertyHandler<ushort, INpc, INpcGetter>("PlayerSkills.Health") },
            { "PlayerSkills.Magicka", new SimpleReflectionPropertyHandler<ushort, INpc, INpcGetter>("PlayerSkills.Magicka") },
            { "PlayerSkills.Stamina", new SimpleReflectionPropertyHandler<ushort, INpc, INpcGetter>("PlayerSkills.Stamina") },
            { "PlayerSkills.FarAwayModelDistance", new PlayerSkillsFarAwayModelDistanceHandler() },
            { "PlayerSkills.GearedUpWeapons", new SimpleReflectionPropertyHandler<byte, INpc, INpcGetter>("PlayerSkills.GearedUpWeapons") },
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
