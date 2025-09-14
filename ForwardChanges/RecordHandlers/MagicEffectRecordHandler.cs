using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.MagicEffect;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.RecordHandlers
{
    public class MagicEffectRecordHandler : AbstractRecordHandler
    {
        private readonly Dictionary<string, IPropertyHandler> _propertyHandlers;

        public override Dictionary<string, IPropertyHandler> PropertyHandlers => _propertyHandlers;

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

        public MagicEffectRecordHandler()
        {
            _propertyHandlers = new Dictionary<string, IPropertyHandler>
            {
                // Base properties
                { "EditorID", new EditorIDHandler() },
                { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
                
                // Essential Magic Effect properties
                { "Name", new NameHandler() },
                { "BaseCost", new BaseCostHandler() },
                { "Flags", new FlagsHandler() },
                { "CastType", new CastTypeHandler() },
                { "TargetType", new TargetTypeHandler() },
                { "MagicSkill", new MagicSkillHandler() },
                { "ResistValue", new ResistValueHandler() },
                { "SecondActorValue", new SecondActorValueHandler() },
                { "CastingSoundLevel", new CastingSoundLevelHandler() },
                
                // FormLink properties
                { "MenuDisplayObject", new MenuDisplayObjectHandler() },
                { "Keywords", new KeywordsHandler() },
                { "CastingLight", new CastingLightHandler() },
                { "HitShader", new HitShaderHandler() },
                { "EnchantShader", new EnchantShaderHandler() },
                { "Projectile", new ProjectileHandler() },
                { "Explosion", new ExplosionHandler() },
                { "CastingArt", new CastingArtHandler() },
                { "HitEffectArt", new HitEffectArtHandler() },
                { "ImpactData", new ImpactDataHandler() },
                { "DualCastArt", new DualCastArtHandler() },
                { "EnchantArt", new EnchantArtHandler() },
                { "HitVisuals", new HitVisualsHandler() },
                { "EnchantVisuals", new EnchantVisualsHandler() },
                { "EquipAbility", new EquipAbilityHandler() },
                { "ImageSpaceModifier", new ImageSpaceModifierHandler() },
                { "PerkToApply", new PerkToApplyHandler() },
                
                // Simple properties
                { "Unknown1", new Unknown1Handler() },
                { "TaperWeight", new TaperWeightHandler() },
                { "MinimumSkillLevel", new MinimumSkillLevelHandler() },
                { "SpellmakingArea", new SpellmakingAreaHandler() },
                { "SpellmakingCastingTime", new SpellmakingCastingTimeHandler() },
                { "TaperCurve", new TaperCurveHandler() },
                { "TaperDuration", new TaperDurationHandler() },
                { "SecondActorValueWeight", new SecondActorValueWeightHandler() },
                { "SkillUsageMultiplier", new SkillUsageMultiplierHandler() },
                { "DualCastScale", new DualCastScaleHandler() },
                { "ScriptEffectAIScore", new ScriptEffectAIScoreHandler() },
                { "ScriptEffectAIDelayTime", new ScriptEffectAIDelayTimeHandler() },
                
                // Complex properties
                { "Description", new DescriptionHandler() },
                { "CounterEffects", new CounterEffectsHandler() },
                { "Sounds", new SoundsHandler() },
                
                // Complex properties
                { "VirtualMachineAdapter", new VirtualMachineAdapterHandler() },
                { "Archetype", new ArchetypeHandler() },
                { "Conditions", new ConditionsHandler() }
            };
        }

        public override IMajorRecord GetOverrideRecord(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            return winningContext.GetOrAddAsOverride(state.PatchMod);
        }

        public override void ApplyForwardedProperties(IMajorRecord record, Dictionary<string, object?> propertiesToForward)
        {
            foreach (var (propertyName, value) in propertiesToForward)
            {
                if (PropertyHandlers.TryGetValue(propertyName, out var handler))
                {
                    try
                    {
                        Console.WriteLine($"[{propertyName}] Applying value: {handler.FormatValue(value)}, Type: {value?.GetType()}");
                        handler.SetValue(record, value);
                    }
                    catch (Exception ex)
                    {
                        // Property doesn't exist on this magic effect type - just continue
                        Console.WriteLine($"     Warning: Could not apply property {propertyName}: {ex.Message}");
                    }
                }
            }
        }
    }
}