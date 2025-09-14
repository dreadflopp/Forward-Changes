using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Weapon;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    public class WeaponRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            // General properties (using existing handlers)
            { "Name", new NameHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "MajorFlags", new MajorFlagsHandler() },
            { "Model", new ModelHandler() },
            { "Icons", new IconsHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "Keywords", new KeywordListHandler() },
            { "VirtualMachineAdapter", new VirtualMachineAdapterHandler() },

            // Weapon-specific properties
            { "ObjectEffect", new ObjectEffectHandler() },
            { "EnchantmentAmount", new EnchantmentAmountHandler() },
            { "Destructible", new DestructibleHandler() },
            { "EquipmentType", new EquipmentTypeHandler() },
            { "BlockBashImpact", new BlockBashImpactHandler() },
            { "AlternateBlockMaterial", new AlternateBlockMaterialHandler() },
            { "PickUpSound", new PickUpSoundHandler() },
            { "PutDownSound", new PutDownSoundHandler() },
            { "Description", new DescriptionHandler() },
            { "ScopeModel", new ScopeModelHandler() },
            { "ImpactDataSet", new ImpactDataSetHandler() },
            { "FirstPersonModel", new FirstPersonModelHandler() },
            { "AttackSound", new AttackSoundHandler() },
            { "AttackSound2D", new AttackSound2DHandler() },
            { "AttackLoopSound", new AttackLoopSoundHandler() },
            { "AttackFailSound", new AttackFailSoundHandler() },
            { "IdleSound", new IdleSoundHandler() },
            { "EquipSound", new EquipSoundHandler() },
            { "UnequipSound", new UnequipSoundHandler() },
            { "Value", new ValueHandler() },
            { "Weight", new WeightHandler() },
            { "Damage", new DamageHandler() },
            { "DetectionSoundLevel", new DetectionSoundLevelHandler() },
            { "Template", new TemplateHandler() },
            { "AnimationType", new AnimationTypeHandler() },
            { "Speed", new SpeedHandler() },
            { "Reach", new ReachHandler() },
            { "Flags", new WeaponDataFlagsHandler() },
            { "SightFOV", new SightFOVHandler() },
            { "Unknown", new UnknownHandler() },
            { "BaseVATStoHitChance", new BaseVATStoHitChanceHandler() },
            { "AttackAnimation", new AttackAnimationHandler() },
            { "NumProjectiles", new NumProjectilesHandler() },
            { "EmbeddedWeaponAV", new EmbeddedWeaponAVHandler() },
            { "RangeMin", new RangeMinHandler() },
            { "RangeMax", new RangeMaxHandler() },
            { "OnHit", new OnHitHandler() },
            { "AnimationAttackMult", new AnimationAttackMultHandler() },
            { "Unknown2", new Unknown2Handler() },
            { "RumbleLeftMotorStrength", new RumbleLeftMotorStrengthHandler() },
            { "RumbleRightMotorStrength", new RumbleRightMotorStrengthHandler() },
            { "RumbleDuration", new RumbleDurationHandler() },
            { "Skill", new SkillHandler() },
            { "Unknown4", new Unknown4Handler() },
            { "Resist", new ResistHandler() },
            { "Unknown5", new Unknown5Handler() },
            { "Stagger", new StaggerHandler() },
            // CriticalData
            { "Versioning", new CriticalVersioningHandler() },
            { "CriticalDamage", new CriticalDamageHandler() },
            { "CriticalUnused", new CriticalUnusedHandler() },
            { "PercentMult", new CriticalPercentMultHandler() },
            { "CriticalFlags", new CriticalFlagsHandler() },
            { "CriticalUnused3", new CriticalUnused3Handler() },
            { "Effect", new CriticalEffectHandler() },
            { "CriticalUnused4", new CriticalUnused4Handler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IWeaponGetter weaponRecord)
            {
                throw new InvalidOperationException($"Expected IWeaponGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = weaponRecord
                .ToLink<IWeaponGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IWeapon, IWeaponGetter>(state.LinkCache)
                .ToArray();

            return contexts;
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
                        // Property doesn't exist on this weapon type - just continue
                        Console.WriteLine($"Warning: Property {propertyName} not available on weapon {record.FormKey}: {ex.Message}");
                    }
                }
            }
        }
    }
}