using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.ArmorAddon;
using ForwardChanges.PropertyHandlers.General;
using System;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: the three semantic BodyTemplate leaves use the same exact dotted-property pattern as Armor.
    // - Split: WorldModel and FirstPersonModel filenames and alternate textures are tracked independently for male and female models.
    // - Split: priority, weight-slider state, skin textures, and texture-swap lists are tracked independently by gender.
    // - Kept specialized: AdditionalRaces retains list ownership semantics; remaining links and scalar values use general handlers.
    // - Intentionally excluded: BodyTemplate.ActsLike44 is Mutagen serialization state; Unknown* fields are outside the semantic conflict surface.
    // - Intentionally excluded: model information (MO2T/MO3T/MO4T/MO5T) is generated metadata and does not independently drive forwarding.
    // - Rationale: semantic BodyTemplate values are forwardable, and independent gender/model fields prevent one change from masking another;
    //   model information travels with a forwarded filename, while the BodyTemplate binary-layout discriminator is not forwarded independently.
    public class ArmorAddonRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "WeightSliderEnabled.Male", new GenderedItemSideHandler<bool, IArmorAddon, IArmorAddonGetter>("WeightSliderEnabled", MaleFemaleGender.Male, record => record.WeightSliderEnabled, (record, value) => { if (value != null) record.WeightSliderEnabled = value; }, value => value) },
            { "WeightSliderEnabled.Female", new GenderedItemSideHandler<bool, IArmorAddon, IArmorAddonGetter>("WeightSliderEnabled", MaleFemaleGender.Female, record => record.WeightSliderEnabled, (record, value) => { if (value != null) record.WeightSliderEnabled = value; }, value => value) },
            { "WorldModel.Male.File", new GenderedModelFileHandler<IArmorAddon, IArmorAddonGetter>("WorldModel", MaleFemaleGender.Male, record => record.WorldModel, (record, value) => record.WorldModel = value) },
            { "WorldModel.Male.AlternateTextures", new GenderedModelAlternateTexturesHandler<IArmorAddon, IArmorAddonGetter>("WorldModel", MaleFemaleGender.Male, record => record.WorldModel, (record, value) => record.WorldModel = value) },
            { "WorldModel.Female.File", new GenderedModelFileHandler<IArmorAddon, IArmorAddonGetter>("WorldModel", MaleFemaleGender.Female, record => record.WorldModel, (record, value) => record.WorldModel = value) },
            { "WorldModel.Female.AlternateTextures", new GenderedModelAlternateTexturesHandler<IArmorAddon, IArmorAddonGetter>("WorldModel", MaleFemaleGender.Female, record => record.WorldModel, (record, value) => record.WorldModel = value) },
            { "FirstPersonModel.Male.File", new GenderedModelFileHandler<IArmorAddon, IArmorAddonGetter>("FirstPersonModel", MaleFemaleGender.Male, record => record.FirstPersonModel, (record, value) => record.FirstPersonModel = value) },
            { "FirstPersonModel.Male.AlternateTextures", new GenderedModelAlternateTexturesHandler<IArmorAddon, IArmorAddonGetter>("FirstPersonModel", MaleFemaleGender.Male, record => record.FirstPersonModel, (record, value) => record.FirstPersonModel = value) },
            { "FirstPersonModel.Female.File", new GenderedModelFileHandler<IArmorAddon, IArmorAddonGetter>("FirstPersonModel", MaleFemaleGender.Female, record => record.FirstPersonModel, (record, value) => record.FirstPersonModel = value) },
            { "FirstPersonModel.Female.AlternateTextures", new GenderedModelAlternateTexturesHandler<IArmorAddon, IArmorAddonGetter>("FirstPersonModel", MaleFemaleGender.Female, record => record.FirstPersonModel, (record, value) => record.FirstPersonModel = value) },
            { "AdditionalRaces", new AdditionalRacesHandler() },
            { "BodyTemplate.FirstPersonFlags", new SimpleReflectionFlagPropertyHandler<BipedObjectFlag, IArmorAddon, IArmorAddonGetter>("BodyTemplate.FirstPersonFlags", preserveUnknownBits: true, includeUnnamedBits: true) },
            { "BodyTemplate.Flags", new SimpleReflectionFlagPropertyHandler<BodyTemplate.Flag, IArmorAddon, IArmorAddonGetter>("BodyTemplate.Flags", preserveUnknownBits: true) },
            { "BodyTemplate.ArmorType", new SimpleReflectionPropertyHandler<ArmorType, IArmorAddon, IArmorAddonGetter>("BodyTemplate.ArmorType") },
            { "Priority.Male", new GenderedItemSideHandler<byte, IArmorAddon, IArmorAddonGetter>("Priority", MaleFemaleGender.Male, record => record.Priority, (record, value) => { if (value != null) record.Priority = value; }, value => value) },
            { "Priority.Female", new GenderedItemSideHandler<byte, IArmorAddon, IArmorAddonGetter>("Priority", MaleFemaleGender.Female, record => record.Priority, (record, value) => { if (value != null) record.Priority = value; }, value => value) },
            { "DetectionSoundValue", new SimpleReflectionPropertyHandler<byte, IArmorAddon, IArmorAddonGetter>("DetectionSoundValue") },
            { "WeaponAdjust", new SimpleReflectionPropertyHandler<float, IArmorAddon, IArmorAddonGetter>("WeaponAdjust") },
            { "Race", new SimpleReflectionFormLinkPropertyHandler<IRaceGetter, IArmorAddon, IArmorAddonGetter>("Race") },
            { "FootstepSound", new SimpleReflectionFormLinkPropertyHandler<IFootstepSetGetter, IArmorAddon, IArmorAddonGetter>("FootstepSound") },
            { "ArtObject", new SimpleReflectionFormLinkPropertyHandler<IArtObjectGetter, IArmorAddon, IArmorAddonGetter>("ArtObject") },
            { "SkinTexture.Male", new GenderedItemSideHandler<IFormLinkNullableGetter<ITextureSetGetter>, IArmorAddon, IArmorAddonGetter>("SkinTexture", MaleFemaleGender.Male, record => record.SkinTexture, (record, value) => record.SkinTexture = value, value => value == null ? new FormLinkNullable<ITextureSetGetter>() : new FormLinkNullable<ITextureSetGetter>(value.FormKey), (left, right) => left?.FormKey == right?.FormKey) },
            { "SkinTexture.Female", new GenderedItemSideHandler<IFormLinkNullableGetter<ITextureSetGetter>, IArmorAddon, IArmorAddonGetter>("SkinTexture", MaleFemaleGender.Female, record => record.SkinTexture, (record, value) => record.SkinTexture = value, value => value == null ? new FormLinkNullable<ITextureSetGetter>() : new FormLinkNullable<ITextureSetGetter>(value.FormKey), (left, right) => left?.FormKey == right?.FormKey) },
            { "TextureSwapList.Male", new GenderedItemSideHandler<IFormLinkNullableGetter<IFormListGetter>, IArmorAddon, IArmorAddonGetter>("TextureSwapList", MaleFemaleGender.Male, record => record.TextureSwapList, (record, value) => record.TextureSwapList = value, value => value == null ? new FormLinkNullable<IFormListGetter>() : new FormLinkNullable<IFormListGetter>(value.FormKey), (left, right) => left?.FormKey == right?.FormKey) },
            { "TextureSwapList.Female", new GenderedItemSideHandler<IFormLinkNullableGetter<IFormListGetter>, IArmorAddon, IArmorAddonGetter>("TextureSwapList", MaleFemaleGender.Female, record => record.TextureSwapList, (record, value) => record.TextureSwapList = value, value => value == null ? new FormLinkNullable<IFormListGetter>() : new FormLinkNullable<IFormListGetter>(value.FormKey), (left, right) => left?.FormKey == right?.FormKey) }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IArmorAddonGetter armorAddonRecord)
            {
                throw new InvalidOperationException($"Expected IArmorAddonGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = armorAddonRecord
                .ToLink<IArmorAddonGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IArmorAddon, IArmorAddonGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}
