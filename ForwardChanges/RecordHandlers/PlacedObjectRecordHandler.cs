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
using Noggog;

namespace ForwardChanges.RecordHandlers
{
    public class PlacedObjectRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "Base", new SimpleReflectionFormLinkPropertyHandler<IPlaceableObjectGetter, IPlacedObject, IPlacedObjectGetter>("Base") },
            { "Owner", new SimpleReflectionFormLinkPropertyHandler<IOwnerGetter, IPlacedObject, IPlacedObjectGetter>("Owner") },
            { "Scale", new SimpleReflectionPropertyHandler<float?, IPlacedObject, IPlacedObjectGetter>("Scale") },
            { "LocationReference", new SimpleReflectionFormLinkPropertyHandler<ILocationRecordGetter, IPlacedObject, IPlacedObjectGetter>("LocationReference") },
            { "Placement.Position", new SimpleReflectionPropertyHandler<P3Float?, IPlacedObject, IPlacedObjectGetter>("Placement.Position") },
            { "Placement.Rotation", new SimpleReflectionPropertyHandler<P3Float?, IPlacedObject, IPlacedObjectGetter>("Placement.Rotation") },
            { "LinkedReferences", new LinkedReferencesHandler() },
            { "LinkedRooms", new LinkedRoomsHandler() },
            { "ImageSpace", new SimpleReflectionFormLinkPropertyHandler<IImageSpaceGetter, IPlacedObject, IPlacedObjectGetter>("ImageSpace") },
            { "LightingTemplate", new SimpleReflectionFormLinkPropertyHandler<ILightingTemplateGetter, IPlacedObject, IPlacedObjectGetter>("LightingTemplate") },
            { "Unknown", new SimpleReflectionPropertyHandler<short, IPlacedObject, IPlacedObjectGetter>("Unknown") },
            { "BoundHalfExtents", new SimpleReflectionPropertyHandler<P3Float?, IPlacedObject, IPlacedObjectGetter>("BoundHalfExtents") },
            { "Primitive", new ComplexReflectionPropertyHandler<IPlacedPrimitiveGetter, IPlacedObject, IPlacedObjectGetter>("Primitive") },
            { "OcclusionPlane", new ComplexReflectionPropertyHandler<IBoundingGetter, IPlacedObject, IPlacedObjectGetter>("OcclusionPlane") },
            { "Portals", new PortalsHandler() },
            { "RoomPortal", new ComplexReflectionPropertyHandler<IBoundingGetter, IPlacedObject, IPlacedObjectGetter>("RoomPortal") },
            { "Radius", new SimpleReflectionPropertyHandler<float?, IPlacedObject, IPlacedObjectGetter>("Radius") },
            { "Reflections", new ReflectionsHandler() },
            { "LitWater", new LitWaterHandler() },
            { "Emittance", new SimpleReflectionFormLinkPropertyHandler<IEmittanceGetter, IPlacedObject, IPlacedObjectGetter>("Emittance") },
            { "TeleportMessageBox", new SimpleReflectionFormLinkPropertyHandler<IMessageGetter, IPlacedObject, IPlacedObjectGetter>("TeleportMessageBox") },
            { "MultiboundReference", new SimpleReflectionFormLinkPropertyHandler<IPlacedObjectGetter, IPlacedObject, IPlacedObjectGetter>("MultiboundReference") },
            { "SpawnContainer", new SimpleReflectionFormLinkPropertyHandler<IPlacedObjectGetter, IPlacedObject, IPlacedObjectGetter>("SpawnContainer") },
            { "LeveledItemBaseObject", new SimpleReflectionFormLinkPropertyHandler<ILeveledItemGetter, IPlacedObject, IPlacedObjectGetter>("LeveledItemBaseObject") },
            { "PersistentLocation", new SimpleReflectionFormLinkPropertyHandler<ILocationGetter, IPlacedObject, IPlacedObjectGetter>("PersistentLocation") },
            { "EncounterZone", new SimpleReflectionFormLinkPropertyHandler<IEncounterZoneGetter, IPlacedObject, IPlacedObjectGetter>("EncounterZone") },
            { "NavigationDoorLink", new ComplexReflectionPropertyHandler<INavigationDoorLinkGetter, IPlacedObject, IPlacedObjectGetter>("NavigationDoorLink") },
            { "LocationRefTypes", new LocationRefTypesHandler() },
            { "IsMultiBoundPrimitive", new SimpleReflectionPropertyHandler<bool, IPlacedObject, IPlacedObjectGetter>("IsMultiBoundPrimitive") },
            { "IsIgnoredBySandbox", new SimpleReflectionPropertyHandler<bool, IPlacedObject, IPlacedObjectGetter>("IsIgnoredBySandbox") },
            { "IsOpenByDefault", new SimpleReflectionPropertyHandler<bool, IPlacedObject, IPlacedObjectGetter>("IsOpenByDefault") },
            { "FactionRank", new SimpleReflectionPropertyHandler<int?, IPlacedObject, IPlacedObjectGetter>("FactionRank") },
            { "ItemCount", new SimpleReflectionPropertyHandler<int?, IPlacedObject, IPlacedObjectGetter>("ItemCount") },
            { "Charge", new SimpleReflectionPropertyHandler<float?, IPlacedObject, IPlacedObjectGetter>("Charge") },
            { "HeadTrackingWeight", new SimpleReflectionPropertyHandler<float?, IPlacedObject, IPlacedObjectGetter>("HeadTrackingWeight") },
            { "FavorCost", new SimpleReflectionPropertyHandler<float?, IPlacedObject, IPlacedObjectGetter>("FavorCost") },
            { "CollisionLayer", new SimpleReflectionPropertyHandler<uint?, IPlacedObject, IPlacedObjectGetter>("CollisionLayer") },
            { "LevelModifier", new SimpleReflectionPropertyHandler<Level?, IPlacedObject, IPlacedObjectGetter>("LevelModifier") },
            { "TeleportDestination", new ComplexReflectionPropertyHandler<ITeleportDestinationGetter, IPlacedObject, IPlacedObjectGetter>("TeleportDestination") },
            { "ActivateParents", new ComplexReflectionPropertyHandler<IActivateParentsGetter, IPlacedObject, IPlacedObjectGetter>("ActivateParents") },
            { "Lock", new LockHandler() },
            { "AttachRef", new SimpleReflectionFormLinkPropertyHandler<IPlacedThingGetter, IPlacedObject, IPlacedObjectGetter>("AttachRef") },
            { "Action", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.PlacedObject.ActionFlag, IPlacedObject, IPlacedObjectGetter>("Action") },
            { "LightData", new ComplexReflectionPropertyHandler<ILightDataGetter, IPlacedObject, IPlacedObjectGetter>("LightData") },
            { "Alpha", new ComplexReflectionPropertyHandler<IAlphaGetter, IPlacedObject, IPlacedObjectGetter>("Alpha") },
            { "Patrol", new ComplexReflectionPropertyHandler<IPatrolGetter, IPlacedObject, IPlacedObjectGetter>("Patrol") },
            { "MapMarker", new MapMarkerHandler() },
            { "Placement", new ComplexReflectionPropertyHandler<IPlacementGetter, IPlacedObject, IPlacedObjectGetter>("Placement") }
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

        // ApplyForwardedProperties is now handled by the base class
        // The base class automatically handles flag property coordination
    }
}
