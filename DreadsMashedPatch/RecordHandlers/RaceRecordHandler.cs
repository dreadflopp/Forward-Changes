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
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.PropertyHandlers.Race;
using DreadsMashedPatch.RecordHandlers.Abstracts;

namespace DreadsMashedPatch.RecordHandlers;

// Migration note:
// - Generalized: RACE scalar, list, form-link, nested complex fields, and dictionary fields.
// - Kept specialized: gendered aggregates use typed male/female copying and equality; Skill Boosts are
//   grouped as xEdit's fixed sorted array keyed by Skill instead of Mutagen's seven physical slots.
// - Intentionally excluded: DATADataTypeState and ExportingExtraNam2 are serialization state; Unknown is outside the semantic conflict surface.
// - Rationale: semantic fields are forwarded while the winning record retains its binary DATA layout and empty NAM2 marker state.
public class RaceRecordHandler : AbstractRecordHandler
{
    public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
    {
        { "EditorID", new EditorIDHandler() },
        { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
        { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
        { "Name", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IRace, IRaceGetter>("Name") },
        { "Description", new ComplexReflectionPropertyHandler<ITranslatedStringGetter, IRace, IRaceGetter>("Description") },
        { "ActorEffect", new SimpleReflectionListPropertyHandler<IFormLinkGetter<ISpellRecordGetter>, IRace, IRaceGetter>("ActorEffect", ListSemantics.SortedKeyed) },
        { "Skin", new SimpleReflectionFormLinkPropertyHandler<IArmorGetter, IRace, IRaceGetter>("Skin") },
        { "BodyTemplate", new ComplexReflectionPropertyHandler<IBodyTemplateGetter, IRace, IRaceGetter>("BodyTemplate") },
        { "Keywords", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IKeywordGetter>, IRace, IRaceGetter>("Keywords", ListSemantics.SortedKeyed) },
        { "SkillBoosts", new RaceSkillBoostsHandler() },
        { "Height", new GenderedItemHandler<float, float, IRace, IRaceGetter>("Height", record => record.Height, (record, value) => { if (value != null) record.Height = value; }, value => value) },
        { "Weight", new GenderedItemHandler<float, float, IRace, IRaceGetter>("Weight", record => record.Weight, (record, value) => { if (value != null) record.Weight = value; }, value => value) },
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
        { "SkeletalModel", new GenderedItemHandler<ISimpleModelGetter?, SimpleModel?, IRace, IRaceGetter>("SkeletalModel", record => record.SkeletalModel, (record, value) => record.SkeletalModel = value, value => value?.DeepCopy(), (left, right) => left == null ? right == null : right != null && left.Equals(right)) },
        { "MovementTypeNames", new SimpleReflectionListPropertyHandler<string, IRace, IRaceGetter>("MovementTypeNames", ListSemantics.SortedKeyed) },
        { "Voices", new GenderedItemHandler<IFormLinkGetter<IVoiceTypeGetter>, IFormLinkGetter<IVoiceTypeGetter>, IRace, IRaceGetter>("Voices", record => record.Voices, (record, value) => { if (value != null) record.Voices = value; }, value => new FormLink<IVoiceTypeGetter>(value.FormKey), (left, right) => left.FormKey == right.FormKey) },
        { "DecapitateArmors", new GenderedItemHandler<IFormLinkGetter<IArmorGetter>, IFormLinkGetter<IArmorGetter>, IRace, IRaceGetter>("DecapitateArmors", record => record.DecapitateArmors, (record, value) => record.DecapitateArmors = value, value => new FormLink<IArmorGetter>(value.FormKey), (left, right) => left.FormKey == right.FormKey) },
        { "DefaultHairColors", new GenderedItemHandler<IFormLinkGetter<IColorRecordGetter>, IFormLinkGetter<IColorRecordGetter>, IRace, IRaceGetter>("DefaultHairColors", record => record.DefaultHairColors, (record, value) => record.DefaultHairColors = value, value => new FormLink<IColorRecordGetter>(value.FormKey), (left, right) => left.FormKey == right.FormKey) },
        { "NumberOfTintsInList", new SimpleReflectionPropertyHandler<ushort?, IRace, IRaceGetter>("NumberOfTintsInList") },
        { "FacegenMainClamp", new SimpleReflectionPropertyHandler<float, IRace, IRaceGetter>("FacegenMainClamp") },
        { "FacegenFaceClamp", new SimpleReflectionPropertyHandler<float, IRace, IRaceGetter>("FacegenFaceClamp") },
        { "AttackRace", new SimpleReflectionFormLinkPropertyHandler<IRaceGetter, IRace, IRaceGetter>("AttackRace") },
        { "Attacks", new SimpleReflectionListPropertyHandler<IAttackGetter, IRace, IRaceGetter>("Attacks", ListSemantics.SortedKeyed, keySelector: attack => attack.AttackEvent) },
        { "BodyData", new GenderedItemHandler<IBodyDataGetter?, BodyData?, IRace, IRaceGetter>("BodyData", record => record.BodyData, (record, value) => { if (value != null) record.BodyData = value; }, value => value?.DeepCopy(), (left, right) => left == null ? right == null : right != null && left.Equals(right)) },
        { "Hairs", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IHairGetter>, IRace, IRaceGetter>("Hairs", ListSemantics.SortedKeyed) },
        { "Eyes", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IEyesGetter>, IRace, IRaceGetter>("Eyes", ListSemantics.SortedKeyed) },
        { "BodyPartData", new SimpleReflectionFormLinkPropertyHandler<IBodyPartDataGetter, IRace, IRaceGetter>("BodyPartData") },
        { "BehaviorGraph", new GenderedItemHandler<IModelBehaviorGetter?, ModelBehavior?, IRace, IRaceGetter>("BehaviorGraph", record => record.BehaviorGraph, (record, value) => { if (value != null) record.BehaviorGraph = value; }, value => value?.DeepCopy(), (left, right) => left == null ? right == null : right != null && left.Equals(right)) },
        { "MaterialType", new SimpleReflectionFormLinkPropertyHandler<IMaterialTypeGetter, IRace, IRaceGetter>("MaterialType") },
        { "ImpactDataSet", new SimpleReflectionFormLinkPropertyHandler<IImpactDataSetGetter, IRace, IRaceGetter>("ImpactDataSet") },
        { "DecapitationFX", new SimpleReflectionFormLinkPropertyHandler<IArtObjectGetter, IRace, IRaceGetter>("DecapitationFX") },
        { "OpenLootSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IRace, IRaceGetter>("OpenLootSound") },
        { "CloseLootSound", new SimpleReflectionFormLinkPropertyHandler<ISoundDescriptorGetter, IRace, IRaceGetter>("CloseLootSound") },
        { "BipedObjectNames", new RaceBipedObjectNamesHandler() },
        { "MovementTypes", new SimpleReflectionListPropertyHandler<IRaceMovementTypeGetter, IRace, IRaceGetter>("MovementTypes", ListSemantics.SortedKeyed, keySelector: movement => movement.MovementType.FormKey) },
        { "EquipmentFlags", new SimpleReflectionPropertyHandler<EquipTypeFlag?, IRace, IRaceGetter>("EquipmentFlags") },
        { "EquipmentSlots", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IEquipTypeGetter>, IRace, IRaceGetter>("EquipmentSlots", ListSemantics.SortedKeyed) },
        { "UnarmedEquipSlot", new SimpleReflectionFormLinkPropertyHandler<IEquipTypeGetter, IRace, IRaceGetter>("UnarmedEquipSlot") },
        { "FaceFxPhonemes", new ComplexReflectionPropertyHandler<IFaceFxPhonemesGetter, IRace, IRaceGetter>("FaceFxPhonemes") },
        { "BaseMovementDefaultWalk", new SimpleReflectionFormLinkPropertyHandler<IMovementTypeGetter, IRace, IRaceGetter>("BaseMovementDefaultWalk") },
        { "BaseMovementDefaultRun", new SimpleReflectionFormLinkPropertyHandler<IMovementTypeGetter, IRace, IRaceGetter>("BaseMovementDefaultRun") },
        { "BaseMovementDefaultSwim", new SimpleReflectionFormLinkPropertyHandler<IMovementTypeGetter, IRace, IRaceGetter>("BaseMovementDefaultSwim") },
        { "BaseMovementDefaultFly", new SimpleReflectionFormLinkPropertyHandler<IMovementTypeGetter, IRace, IRaceGetter>("BaseMovementDefaultFly") },
        { "BaseMovementDefaultSneak", new SimpleReflectionFormLinkPropertyHandler<IMovementTypeGetter, IRace, IRaceGetter>("BaseMovementDefaultSneak") },
        { "BaseMovementDefaultSprint", new SimpleReflectionFormLinkPropertyHandler<IMovementTypeGetter, IRace, IRaceGetter>("BaseMovementDefaultSprint") },
        { "HeadData", new GenderedItemHandler<IHeadDataGetter?, HeadData?, IRace, IRaceGetter>("HeadData", record => record.HeadData, (record, value) => record.HeadData = value, value => value?.DeepCopy(), (left, right) => left == null ? right == null : right != null && left.Equals(right)) },
        { "MorphRace", new SimpleReflectionFormLinkPropertyHandler<IRaceGetter, IRace, IRaceGetter>("MorphRace") },
        { "ArmorRace", new SimpleReflectionFormLinkPropertyHandler<IRaceGetter, IRace, IRaceGetter>("ArmorRace") },
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
