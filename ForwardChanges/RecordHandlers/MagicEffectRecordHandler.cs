using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Strings;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.MagicEffect;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: semantic MGEF scalar, link, list, and aggregate fields use shared handlers.
    // - Kept specialized: flags, sounds, conditions, and archetype-specific behavior.
    // - Intentionally excluded: Unknown1 is outside the semantic conflict surface.
    // - Rationale: the winning override retains excluded engine-managed data.
    public class MagicEffectRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Name", new NameHandler() },
            { "VirtualMachineAdapter", new VirtualMachineAdapterHandler() },
            { "Description", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IMagicEffect, IMagicEffectGetter>("Description") },
            { "BaseCost", new SimpleReflectionPropertyHandler<float, IMagicEffect, IMagicEffectGetter>("BaseCost") },
            { "Flags", new FlagsHandler() },
            { "CastType", new SimpleReflectionPropertyHandler<CastType, IMagicEffect, IMagicEffectGetter>("CastType") },
            { "TargetType", new SimpleReflectionPropertyHandler<TargetType, IMagicEffect, IMagicEffectGetter>("TargetType") },
            { "MagicSkill", new SimpleReflectionPropertyHandler<ActorValue, IMagicEffect, IMagicEffectGetter>("MagicSkill") },
            { "ResistValue", new SimpleReflectionPropertyHandler<ActorValue, IMagicEffect, IMagicEffectGetter>("ResistValue") },
            { "SecondActorValue", new SimpleReflectionPropertyHandler<ActorValue, IMagicEffect, IMagicEffectGetter>("SecondActorValue") },
            { "CastingSoundLevel", new SimpleReflectionPropertyHandler<SoundLevel, IMagicEffect, IMagicEffectGetter>("CastingSoundLevel") },
            { "MenuDisplayObject", new SimpleReflectionFormLinkPropertyHandler<IStaticGetter, IMagicEffect, IMagicEffectGetter>("MenuDisplayObject") },
            { "Keywords", new KeywordListHandler() },
            { "CastingLight", new SimpleReflectionFormLinkPropertyHandler<ILightGetter, IMagicEffect, IMagicEffectGetter>("CastingLight") },
            { "HitShader", new SimpleReflectionFormLinkPropertyHandler<IEffectShaderGetter, IMagicEffect, IMagicEffectGetter>("HitShader") },
            { "EnchantShader", new SimpleReflectionFormLinkPropertyHandler<IEffectShaderGetter, IMagicEffect, IMagicEffectGetter>("EnchantShader") },
            { "Projectile", new SimpleReflectionFormLinkPropertyHandler<IProjectileGetter, IMagicEffect, IMagicEffectGetter>("Projectile") },
            { "Explosion", new SimpleReflectionFormLinkPropertyHandler<IExplosionGetter, IMagicEffect, IMagicEffectGetter>("Explosion") },
            { "CastingArt", new SimpleReflectionFormLinkPropertyHandler<IArtObjectGetter, IMagicEffect, IMagicEffectGetter>("CastingArt") },
            { "HitEffectArt", new SimpleReflectionFormLinkPropertyHandler<IArtObjectGetter, IMagicEffect, IMagicEffectGetter>("HitEffectArt") },
            { "ImpactData", new SimpleReflectionFormLinkPropertyHandler<IImpactDataSetGetter, IMagicEffect, IMagicEffectGetter>("ImpactData") },
            { "DualCastArt", new SimpleReflectionFormLinkPropertyHandler<IDualCastDataGetter, IMagicEffect, IMagicEffectGetter>("DualCastArt") },
            { "EnchantArt", new SimpleReflectionFormLinkPropertyHandler<IArtObjectGetter, IMagicEffect, IMagicEffectGetter>("EnchantArt") },
            { "HitVisuals", new SimpleReflectionFormLinkPropertyHandler<IVisualEffectGetter, IMagicEffect, IMagicEffectGetter>("HitVisuals") },
            { "EnchantVisuals", new SimpleReflectionFormLinkPropertyHandler<IVisualEffectGetter, IMagicEffect, IMagicEffectGetter>("EnchantVisuals") },
            { "EquipAbility", new SimpleReflectionFormLinkPropertyHandler<ISpellGetter, IMagicEffect, IMagicEffectGetter>("EquipAbility") },
            { "ImageSpaceModifier", new SimpleReflectionFormLinkPropertyHandler<IImageSpaceAdapterGetter, IMagicEffect, IMagicEffectGetter>("ImageSpaceModifier") },
            { "PerkToApply", new SimpleReflectionFormLinkPropertyHandler<IPerkGetter, IMagicEffect, IMagicEffectGetter>("PerkToApply") },
            { "TaperWeight", new SimpleReflectionPropertyHandler<float, IMagicEffect, IMagicEffectGetter>("TaperWeight") },
            { "MinimumSkillLevel", new SimpleReflectionPropertyHandler<uint, IMagicEffect, IMagicEffectGetter>("MinimumSkillLevel") },
            { "SpellmakingArea", new SimpleReflectionPropertyHandler<uint, IMagicEffect, IMagicEffectGetter>("SpellmakingArea") },
            { "SpellmakingCastingTime", new SimpleReflectionPropertyHandler<float, IMagicEffect, IMagicEffectGetter>("SpellmakingCastingTime") },
            { "TaperCurve", new SimpleReflectionPropertyHandler<float, IMagicEffect, IMagicEffectGetter>("TaperCurve") },
            { "TaperDuration", new SimpleReflectionPropertyHandler<float, IMagicEffect, IMagicEffectGetter>("TaperDuration") },
            { "SecondActorValueWeight", new SimpleReflectionPropertyHandler<float, IMagicEffect, IMagicEffectGetter>("SecondActorValueWeight") },
            { "SkillUsageMultiplier", new SimpleReflectionPropertyHandler<float, IMagicEffect, IMagicEffectGetter>("SkillUsageMultiplier") },
            { "DualCastScale", new SimpleReflectionPropertyHandler<float, IMagicEffect, IMagicEffectGetter>("DualCastScale") },
            { "ScriptEffectAIScore", new SimpleReflectionPropertyHandler<float, IMagicEffect, IMagicEffectGetter>("ScriptEffectAIScore") },
            { "ScriptEffectAIDelayTime", new SimpleReflectionPropertyHandler<float, IMagicEffect, IMagicEffectGetter>("ScriptEffectAIDelayTime") },
            { "CounterEffects", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IMagicEffectGetter>, IMagicEffect, IMagicEffectGetter>("CounterEffects", ListSemantics.SortedKeyed) },
            { "Sounds", new SimpleReflectionListPropertyHandler<IMagicEffectSoundGetter, IMagicEffect, IMagicEffectGetter>("Sounds", ListSemantics.SortedKeyed, keySelector: sound => sound.Type) },
            { "Archetype", new ArchetypeHandler() },
            { "Conditions", new ConditionsHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IMagicEffectGetter magicEffectRecord)
            {
                throw new InvalidOperationException($"Expected IMagicEffectGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = magicEffectRecord
                .ToLink<IMagicEffectGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IMagicEffect, IMagicEffectGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }
    }
}
