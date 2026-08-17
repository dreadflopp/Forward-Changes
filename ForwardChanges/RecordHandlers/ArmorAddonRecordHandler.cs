using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.ArmorAddon;
using ForwardChanges.PropertyHandlers.General;
using System;

namespace ForwardChanges.RecordHandlers
{
    public class ArmorAddonRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "WeightSliderEnabled", new WeightSliderEnabledHandler() },
            { "WorldModel", new GenderedModelHandler("WorldModel") },
            { "FirstPersonModel", new GenderedModelHandler("FirstPersonModel") },
            { "AdditionalRaces", new AdditionalRacesHandler() },
            { "BodyTemplateModulatesVoice", new BodyTemplateModulatesVoiceHandler() },
            { "BodyTemplateNonPlayable", new BodyTemplateNonPlayableHandler() },
            { "BodyTemplateArmorType", new SimpleReflectionPropertyHandler<ArmorType, IArmorAddon, IArmorAddonGetter>("BodyTemplate.ArmorType") },
            { "BodyTemplateFirstPersonFlags", new BodyTemplateFirstPersonFlagsHandler() },
            { "Priority", new PriorityHandler() },
            { "Unknown", new SimpleReflectionPropertyHandler<ushort, IArmorAddon, IArmorAddonGetter>("Unknown") },
            { "DetectionSoundValue", new SimpleReflectionPropertyHandler<byte, IArmorAddon, IArmorAddonGetter>("DetectionSoundValue") },
            { "Unknown2", new SimpleReflectionPropertyHandler<byte, IArmorAddon, IArmorAddonGetter>("Unknown2") },
            { "WeaponAdjust", new SimpleReflectionPropertyHandler<float, IArmorAddon, IArmorAddonGetter>("WeaponAdjust") },
            { "Race", new SimpleReflectionFormLinkPropertyHandler<IRaceGetter, IArmorAddon, IArmorAddonGetter>("Race") },
            { "FootstepSound", new SimpleReflectionFormLinkPropertyHandler<IFootstepSetGetter, IArmorAddon, IArmorAddonGetter>("FootstepSound") },
            { "ArtObject", new SimpleReflectionFormLinkPropertyHandler<IArtObjectGetter, IArmorAddon, IArmorAddonGetter>("ArtObject") },
            { "SkinTexture", new SkinTextureHandler() },
            { "TextureSwapList", new TextureSwapListHandler() }
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