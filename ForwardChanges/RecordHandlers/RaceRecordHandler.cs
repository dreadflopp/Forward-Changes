using System;
using System.Collections.Generic;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Strings;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.Race;
using ForwardChanges.RecordHandlers.Abstracts;

namespace ForwardChanges.RecordHandlers;

// Migration note:
// - Generalized: RACE scalar, list, form-link, nested complex fields, and dictionary fields.
// - Kept specialized: none.
public class RaceRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "ExportingExtraNam2", new SimpleReflectionPropertyHandler<bool, IRace, IRaceGetter>("ExportingExtraNam2") },
        { "Name", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IRace, IRaceGetter>("Name") },
        { "Description", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IRace, IRaceGetter>("Description") },
        { "ActorEffect", new SimpleReflectionListPropertyHandler<IFormLinkGetter<ISpellRecordGetter>, IRace, IRaceGetter>("ActorEffect", ListOrdering.None) },
        { "Skin", new SimpleReflectionFormLinkPropertyHandler<IArmorGetter, IRace, IRaceGetter>("Skin") },
        { "BodyTemplate", new ComplexReflectionPropertyHandler<IBodyTemplateGetter, IRace, IRaceGetter>("BodyTemplate") },
        { "Keywords", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IKeywordGetter>, IRace, IRaceGetter>("Keywords", ListOrdering.None) },
        { "SkillBoost0", new ComplexReflectionPropertyHandler<ISkillBoostGetter, IRace, IRaceGetter>("SkillBoost0") },
        { "SkillBoost1", new ComplexReflectionPropertyHandler<ISkillBoostGetter, IRace, IRaceGetter>("SkillBoost1") },
        { "SkillBoost2", new ComplexReflectionPropertyHandler<ISkillBoostGetter, IRace, IRaceGetter>("SkillBoost2") },
        { "SkillBoost3", new ComplexReflectionPropertyHandler<ISkillBoostGetter, IRace, IRaceGetter>("SkillBoost3") },
        { "SkillBoost4", new ComplexReflectionPropertyHandler<ISkillBoostGetter, IRace, IRaceGetter>("SkillBoost4") },
        { "SkillBoost5", new ComplexReflectionPropertyHandler<ISkillBoostGetter, IRace, IRaceGetter>("SkillBoost5") },
        { "SkillBoost6", new ComplexReflectionPropertyHandler<ISkillBoostGetter, IRace, IRaceGetter>("SkillBoost6") },
        { "Unknown", new SimpleReflectionPropertyHandler<short, IRace, IRaceGetter>("Unknown") },
        { "Height", new ComplexReflectionPropertyHandler<IGenderedItemGetter<float>, IRace, IRaceGetter>("Height") },
        { "Weight", new ComplexReflectionPropertyHandler<IGenderedItemGetter<float>, IRace, IRaceGetter>("Weight") },
        { "Flags", new SimpleReflectionFlagPropertyHandler<Race.Flag, IRace, IRaceGetter>("Flags") },
        { "Starting", new RaceStartingHandler() },
        { "BaseCarryWeight", new SimpleReflectionPropertyHandler<float, IRace, IRaceGetter>("BaseCarryWeight") },
        { "BaseMass", new SimpleReflectionPropertyHandler<float, IRace, IRaceGetter>("BaseMass") },
        { "AccelerationRate", new SimpleReflectionPropertyHandler<float, IRace, IRaceGetter>("AccelerationRate") },
        { "DecelerationRate", new SimpleReflectionPropertyHandler<float, IRace, IRaceGetter>("DecelerationRate") },
        { "Size", new SimpleReflectionPropertyHandler<Size, IRace, IRaceGetter>("Size") },
        { "HeadBipedObject", new SimpleReflectionPropertyHandler<BipedObject, IRace, IRaceGetter>("HeadBipedObject") },
        { "HairBipedObject", new SimpleReflectionPropertyHandler<BipedObject, IRace, IRaceGetter>("HairBipedObject") },
        { "InjuredHealthPercent", new SimpleReflectionPropertyHandler<float, IRace, IRaceGetter>("InjuredHealthPercent") },
        { "ShieldBipedObject", new SimpleReflectionPropertyHandler<BipedObject, IRace, IRaceGetter>("ShieldBipedObject") },
        { "Regen", new RaceRegenHandler() },
        { "UnarmedDamage", new SimpleReflectionPropertyHandler<float, IRace, IRaceGetter>("UnarmedDamage") },
        { "UnarmedReach", new SimpleReflectionPropertyHandler<float, IRace, IRaceGetter>("UnarmedReach") },
        { "BodyBipedObject", new SimpleReflectionPropertyHandler<BipedObject, IRace, IRaceGetter>("BodyBipedObject") },
        { "AimAngleTolerance", new SimpleReflectionPropertyHandler<float, IRace, IRaceGetter>("AimAngleTolerance") },
        { "FlightRadius", new SimpleReflectionPropertyHandler<float, IRace, IRaceGetter>("FlightRadius") },
        { "AngularAccelerationRate", new SimpleReflectionPropertyHandler<float, IRace, IRaceGetter>("AngularAccelerationRate") },
        { "AngularTolerance", new SimpleReflectionPropertyHandler<float, IRace, IRaceGetter>("AngularTolerance") },
        { "MountData", new ComplexReflectionPropertyHandler<IMountDataGetter, IRace, IRaceGetter>("MountData") },
        { "SkeletalModel", new ComplexReflectionPropertyHandler<IGenderedItemGetter<ISimpleModelGetter>, IRace, IRaceGetter>("SkeletalModel") },
        { "MovementTypeNames", new SimpleReflectionListPropertyHandler<string, IRace, IRaceGetter>("MovementTypeNames", ListOrdering.None) },
        { "Voices", new ComplexReflectionPropertyHandler<IGenderedItemGetter<IFormLinkGetter<IVoiceTypeGetter>>, IRace, IRaceGetter>("Voices") },
        { "DecapitateArmors", new ComplexReflectionPropertyHandler<IGenderedItemGetter<IFormLinkGetter<IArmorGetter>>, IRace, IRaceGetter>("DecapitateArmors") },
        { "DefaultHairColors", new ComplexReflectionPropertyHandler<IGenderedItemGetter<IFormLinkGetter<IColorRecordGetter>>, IRace, IRaceGetter>("DefaultHairColors") },
        { "NumberOfTintsInList", new SimpleReflectionPropertyHandler<ushort?, IRace, IRaceGetter>("NumberOfTintsInList") },
        { "FacegenMainClamp", new SimpleReflectionPropertyHandler<float, IRace, IRaceGetter>("FacegenMainClamp") },
        { "FacegenFaceClamp", new SimpleReflectionPropertyHandler<float, IRace, IRaceGetter>("FacegenFaceClamp") },
        { "AttackRace", new SimpleReflectionFormLinkPropertyHandler<IRaceGetter, IRace, IRaceGetter>("AttackRace") },
        { "Attacks", new SimpleReflectionListPropertyHandler<IAttackGetter, IRace, IRaceGetter>("Attacks", ListOrdering.None) },
        { "BodyData", new ComplexReflectionPropertyHandler<IGenderedItemGetter<IBodyDataGetter>, IRace, IRaceGetter>("BodyData") },
        { "Hairs", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IHairGetter>, IRace, IRaceGetter>("Hairs", ListOrdering.None) },
        { "Eyes", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IEyesGetter>, IRace, IRaceGetter>("Eyes", ListOrdering.None) },
        { "BodyPartData", new SimpleReflectionFormLinkPropertyHandler<IBodyPartDataGetter, IRace, IRaceGetter>("BodyPartData") },
        { "BehaviorGraph", new ComplexReflectionPropertyHandler<IGenderedItemGetter<IModelGetter>, IRace, IRaceGetter>("BehaviorGraph") },
        { "MaterialType", new SimpleReflectionFormLinkPropertyHandler<IMaterialTypeGetter, IRace, IRaceGetter>("MaterialType") },
        { "ImpactDataSet", new SimpleReflectionFormLinkPropertyHandler<IImpactDataSetGetter, IRace, IRaceGetter>("ImpactDataSet") },
        { "DecapitationFX", new SimpleReflectionFormLinkPropertyHandler<IArtObjectGetter, IRace, IRaceGetter>("DecapitationFX") },
        { "OpenLootSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IRace, IRaceGetter>("OpenLootSound") },
        { "CloseLootSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IRace, IRaceGetter>("CloseLootSound") },
        { "BipedObjectNames", new RaceBipedObjectNamesHandler() },
        { "MovementTypes", new SimpleReflectionListPropertyHandler<IRaceMovementTypeGetter, IRace, IRaceGetter>("MovementTypes", ListOrdering.None) },
        { "EquipmentFlags", new SimpleReflectionPropertyHandler<EquipTypeFlag?, IRace, IRaceGetter>("EquipmentFlags") },
        { "EquipmentSlots", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IEquipTypeGetter>, IRace, IRaceGetter>("EquipmentSlots", ListOrdering.None) },
        { "UnarmedEquipSlot", new SimpleReflectionFormLinkPropertyHandler<IEquipTypeGetter, IRace, IRaceGetter>("UnarmedEquipSlot") },
        { "FaceFxPhonemes", new ComplexReflectionPropertyHandler<IFaceFxPhonemesGetter, IRace, IRaceGetter>("FaceFxPhonemes") },
        { "BaseMovementDefaultWalk", new SimpleReflectionFormLinkPropertyHandler<IMovementTypeGetter, IRace, IRaceGetter>("BaseMovementDefaultWalk") },
        { "BaseMovementDefaultRun", new SimpleReflectionFormLinkPropertyHandler<IMovementTypeGetter, IRace, IRaceGetter>("BaseMovementDefaultRun") },
        { "BaseMovementDefaultSwim", new SimpleReflectionFormLinkPropertyHandler<IMovementTypeGetter, IRace, IRaceGetter>("BaseMovementDefaultSwim") },
        { "BaseMovementDefaultFly", new SimpleReflectionFormLinkPropertyHandler<IMovementTypeGetter, IRace, IRaceGetter>("BaseMovementDefaultFly") },
        { "BaseMovementDefaultSneak", new SimpleReflectionFormLinkPropertyHandler<IMovementTypeGetter, IRace, IRaceGetter>("BaseMovementDefaultSneak") },
        { "BaseMovementDefaultSprint", new SimpleReflectionFormLinkPropertyHandler<IMovementTypeGetter, IRace, IRaceGetter>("BaseMovementDefaultSprint") },
        { "HeadData", new ComplexReflectionPropertyHandler<IGenderedItemGetter<IHeadDataGetter>, IRace, IRaceGetter>("HeadData") },
        { "MorphRace", new SimpleReflectionFormLinkPropertyHandler<IRaceGetter, IRace, IRaceGetter>("MorphRace") },
        { "ArmorRace", new SimpleReflectionFormLinkPropertyHandler<IRaceGetter, IRace, IRaceGetter>("ArmorRace") },
        { "DATADataTypeState", new SimpleReflectionPropertyHandler<Race.DATADataType, IRace, IRaceGetter>("DATADataTypeState") },
        { "MajorFlags", new SimpleReflectionFlagPropertyHandler<Race.MajorFlag, IRace, IRaceGetter>("MajorFlags") }
    };

    public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        if (winningContext.Record is not IRaceGetter record)
        {
            throw new InvalidOperationException($"Expected IRaceGetter but got {winningContext.Record.GetType()}");
        }

        return record
            .ToLink<IRaceGetter>()
            .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IRace, IRaceGetter>(state.LinkCache)
            .ToArray();
    }
}
