using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.PlacedObject;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    public class PlacedObjectRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Base", new BaseHandler() },
            { "Owner", new OwnerHandler() },
            { "Scale", new ScaleHandler() },
            { "LocationReference", new LocationReferenceHandler() },
            { "Placement.Position", new PositionHandler() },
            { "Placement.Rotation", new RotationHandler() },
            { "LinkedReferences", new LinkedReferencesHandler() },
            { "LinkedRooms", new LinkedRoomsHandler() },
            { "ImageSpace", new ImageSpaceHandler() },
            { "LightingTemplate", new LightingTemplateHandler() },
            { "Unknown", new UnknownHandler() },
            { "BoundHalfExtents", new BoundHalfExtentsHandler() },
            { "Primitive", new PrimitiveHandler() },
            { "OcclusionPlane", new OcclusionPlaneHandler() },
            { "Portals", new PortalsHandler() },
            { "RoomPortal", new RoomPortalHandler() },
            { "Radius", new RadiusHandler() },
            { "Reflections", new ReflectionsHandler() },
            { "LitWater", new LitWaterHandler() },
            { "Emittance", new EmittanceHandler() },
            { "TeleportMessageBox", new TeleportMessageBoxHandler() },
            { "MultiboundReference", new MultiboundReferenceHandler() },
            { "SpawnContainer", new SpawnContainerHandler() },
            { "LeveledItemBaseObject", new LeveledItemBaseObjectHandler() },
            { "PersistentLocation", new PersistentLocationHandler() },
            { "EncounterZone", new EncounterZoneHandler() },
            { "NavigationDoorLink", new NavigationDoorLinkHandler() },
            { "LocationRefTypes", new LocationRefTypesHandler() },
            { "IsMultiBoundPrimitive", new IsMultiBoundPrimitiveHandler() },
            { "IsIgnoredBySandbox", new IsIgnoredBySandboxHandler() },
            { "IsOpenByDefault", new IsOpenByDefaultHandler() },
            { "FactionRank", new FactionRankHandler() },
            { "ItemCount", new ItemCountHandler() },
            { "Charge", new ChargeHandler() },
            { "HeadTrackingWeight", new HeadTrackingWeightHandler() },
            { "FavorCost", new FavorCostHandler() },
            { "CollisionLayer", new CollisionLayerHandler() },
            { "LevelModifier", new LevelModifierHandler() },
            { "TeleportDestination", new TeleportDestinationHandler() },
            { "ActivateParents", new ActivateParentsHandler() },
            { "Lock", new LockHandler() },
            { "AttachRef", new AttachRefHandler() },
            { "Action", new ActionHandler() },
            { "LightData", new LightDataHandler() },
            { "Alpha", new AlphaHandler() },
            { "Patrol", new PatrolHandler() },
            { "MapMarker", new MapMarkerHandler() },
            { "Placement", new PlacementHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IPlacedObjectGetter placedObjectRecord)
            {
                throw new InvalidOperationException($"Expected IPlacedObjectGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = placedObjectRecord
                .ToLink<IPlacedObjectGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IPlacedObject, IPlacedObjectGetter>(state.LinkCache)
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
                        // Property doesn't exist on this placed object type - just continue
                        Console.WriteLine($"Warning: Property {propertyName} not available on placed object {record.FormKey}: {ex.Message}");
                    }
                }
            }
        }
    }
}