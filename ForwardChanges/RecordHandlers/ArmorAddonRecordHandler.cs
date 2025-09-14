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
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "WeightSliderEnabled", new WeightSliderEnabledHandler() },
            { "WorldModel", new WorldModelHandler() },
            { "FirstPersonModel", new FirstPersonModelHandler() },
            { "AdditionalRaces", new AdditionalRacesHandler() },
            { "BodyTemplateFlags", new BodyTemplateFlagsHandler() },
            { "BodyTemplateArmorType", new BodyTemplateArmorTypeHandler() },
            { "BodyTemplateFirstPersonFlags", new BodyTemplateFirstPersonFlagsHandler() },
            { "Priority", new PriorityHandler() },
            { "Unknown", new UnknownHandler() },
            { "DetectionSoundValue", new DetectionSoundValueHandler() },
            { "Unknown2", new Unknown2Handler() },
            { "WeaponAdjust", new WeaponAdjustHandler() },
            { "Race", new RaceHandler() },
            { "FootstepSound", new FootstepSoundHandler() },
            { "ArtObject", new ArtObjectHandler() },
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
                        // Property doesn't exist on this armor addon type - just continue
                        Console.WriteLine($"Warning: Property {propertyName} not available on armor addon {record.FormKey}: {ex.Message}");
                    }
                }
            }
        }
    }
}