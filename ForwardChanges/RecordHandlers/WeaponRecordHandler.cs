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
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "MajorFlags", new MajorFlagsHandler() },
            { "Model", new ModelHandler() },
            { "Icons", new IconsHandler() },
            { "ObjectBounds", new ObjectBoundsHandler() },
            { "Keywords", new KeywordListHandler() },
            { "VirtualMachineAdapter", new VirtualMachineAdapterHandler() },

            // Weapon-specific properties
            { "ObjectEffect", new SimpleReflectionFormLinkPropertyHandler<IEffectRecordGetter, IWeapon, IWeaponGetter>("ObjectEffect") },
            { "EnchantmentAmount", new SimpleReflectionPropertyHandler<ushort?, IWeapon, IWeaponGetter>("EnchantmentAmount") },
            { "Destructible", new DestructibleHandler() },
            { "EquipmentType", new SimpleReflectionFormLinkPropertyHandler<IEquipTypeGetter, IWeapon, IWeaponGetter>("EquipmentType") },
            { "BlockBashImpact", new SimpleReflectionFormLinkPropertyHandler<IImpactDataSetGetter, IWeapon, IWeaponGetter>("BlockBashImpact") },
            { "AlternateBlockMaterial", new SimpleReflectionFormLinkPropertyHandler<IMaterialTypeGetter, IWeapon, IWeaponGetter>("AlternateBlockMaterial") },
            { "PickUpSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IWeapon, IWeaponGetter>("PickUpSound") },
            { "PutDownSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IWeapon, IWeaponGetter>("PutDownSound") },
            { "Description", new DescriptionHandler() },
            { "ScopeModel", new ScopeModelHandler() },
            { "ImpactDataSet", new SimpleReflectionFormLinkPropertyHandler<IImpactDataSetGetter, IWeapon, IWeaponGetter>("ImpactDataSet") },
            { "FirstPersonModel", new SimpleReflectionFormLinkPropertyHandler<IStaticGetter, IWeapon, IWeaponGetter>("FirstPersonModel") },
            { "AttackSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IWeapon, IWeaponGetter>("AttackSound") },
            { "AttackSound2D", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IWeapon, IWeaponGetter>("AttackSound2D") },
            { "AttackLoopSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IWeapon, IWeaponGetter>("AttackLoopSound") },
            { "AttackFailSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IWeapon, IWeaponGetter>("AttackFailSound") },
            { "IdleSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IWeapon, IWeaponGetter>("IdleSound") },
            { "EquipSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IWeapon, IWeaponGetter>("EquipSound") },
            { "UnequipSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IWeapon, IWeaponGetter>("UnequipSound") },
            { "Value", new ValueHandler() },
            { "Weight", new WeightHandler() },
            { "Damage", new DamageHandler() },
            { "DetectionSoundLevel", new SimpleReflectionPropertyHandler<SoundLevel?, IWeapon, IWeaponGetter>("DetectionSoundLevel") },
            { "Template", new SimpleReflectionFormLinkPropertyHandler<IWeaponGetter, IWeapon, IWeaponGetter>("Template") },
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

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}