using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using DreadsMashedPatch;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.PlacedObject;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using System;
using Noggog;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: placed-object location links continue to use the shared reflection form-link handler.
    //   XLRL/LocationReference intentionally stays on this conflict-aware path: a newly added value that
    //   survives into the winner already produces no patch, while a later removal remains a real conflict,
    //   matching xEdit's cpBenignIfAdded behavior.
    // - Specialized: placement keeps whole-object forwarding and now formats its position/rotation explicitly; complex/list behavior remains specialized.
    // - Nullable aggregates: list presence is inferred from Mutagen metadata, and VMAD preserves absent versus present-empty state.
    // - Intentionally excluded: Unknown is outside the semantic conflict surface.
    // - Rationale: PlacementBinaryOverlay has no useful ToString(), so the record-specific formatter prevents type-name-only logs.
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
            { "LocationReference", new SimpleReflectionFormLinkPropertyHandler<ILocationGetter, IPlacedObject, IPlacedObjectGetter>("LocationReference") },
            { "Placement.Position", new SimpleReflectionPropertyHandler<P3Float?, IPlacedObject, IPlacedObjectGetter>("Placement.Position", P3FloatComparison.PositionEpsilon) },
            { "Placement.Rotation", new SimpleReflectionPropertyHandler<P3Float?, IPlacedObject, IPlacedObjectGetter>("Placement.Rotation", P3FloatComparison.RotationEpsilon) },
            { "LinkedReferences", new SimpleReflectionListPropertyHandler<ILinkedReferencesGetter, IPlacedObject, IPlacedObjectGetter>("LinkedReferences", ListSemantics.SortedKeyed, keySelector: entry => entry.KeywordOrReference.FormKey) },
            { "LinkedRooms", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IPlacedObjectGetter>, IPlacedObject, IPlacedObjectGetter>("LinkedRooms", ListSemantics.SortedKeyed) },
            { "ImageSpace", new SimpleReflectionFormLinkPropertyHandler<IImageSpaceGetter, IPlacedObject, IPlacedObjectGetter>("ImageSpace") },
            { "LightingTemplate", new SimpleReflectionFormLinkPropertyHandler<ILightingTemplateGetter, IPlacedObject, IPlacedObjectGetter>("LightingTemplate") },
            { "BoundHalfExtents", new SimpleReflectionPropertyHandler<P3Float?, IPlacedObject, IPlacedObjectGetter>("BoundHalfExtents") },
            { "Primitive", new ComplexReflectionPropertyHandler<IPlacedPrimitiveGetter, IPlacedObject, IPlacedObjectGetter>("Primitive") },
            { "OcclusionPlane", new ComplexReflectionPropertyHandler<IBoundingGetter, IPlacedObject, IPlacedObjectGetter>("OcclusionPlane") },
            { "Portals", new AtomicReflectionListPropertyHandler<IPortalGetter, IPlacedObject, IPlacedObjectGetter>("Portals") },
            { "RoomPortal", new ComplexReflectionPropertyHandler<IBoundingGetter, IPlacedObject, IPlacedObjectGetter>("RoomPortal") },
            { "Radius", new SimpleReflectionPropertyHandler<float?, IPlacedObject, IPlacedObjectGetter>("Radius") },
            { "Reflections", new SimpleReflectionListPropertyHandler<IWaterReflectionGetter, IPlacedObject, IPlacedObjectGetter>("Reflections", ListSemantics.SortedKeyed, keySelector: entry => entry.Water.FormKey) },
            { "LitWater", new SimpleReflectionListPropertyHandler<IFormLinkGetter<IPlacedObjectGetter>, IPlacedObject, IPlacedObjectGetter>("LitWater", ListSemantics.SortedKeyed) },
            { "Emittance", new SimpleReflectionFormLinkPropertyHandler<IEmittanceGetter, IPlacedObject, IPlacedObjectGetter>("Emittance") },
            { "TeleportMessageBox", new SimpleReflectionFormLinkPropertyHandler<IMessageGetter, IPlacedObject, IPlacedObjectGetter>("TeleportMessageBox") },
            { "MultiBoundReference", new SimpleReflectionFormLinkPropertyHandler<IPlacedObjectGetter, IPlacedObject, IPlacedObjectGetter>("MultiBoundReference") },
            { "SpawnContainer", new SimpleReflectionFormLinkPropertyHandler<IPlacedObjectGetter, IPlacedObject, IPlacedObjectGetter>("SpawnContainer") },
            { "LeveledItemBaseObject", new SimpleReflectionFormLinkPropertyHandler<ILeveledItemGetter, IPlacedObject, IPlacedObjectGetter>("LeveledItemBaseObject") },
            { "PersistentLocation", new SimpleReflectionFormLinkPropertyHandler<ILocationGetter, IPlacedObject, IPlacedObjectGetter>("PersistentLocation") },
            { "EncounterZone", new SimpleReflectionFormLinkPropertyHandler<IEncounterZoneGetter, IPlacedObject, IPlacedObjectGetter>("EncounterZone") },
            { "NavigationDoorLink", new ComplexReflectionPropertyHandler<INavigationDoorLinkGetter, IPlacedObject, IPlacedObjectGetter>("NavigationDoorLink") },
            { "LocationRefTypes", new SimpleReflectionListPropertyHandler<IFormLinkGetter<ILocationReferenceTypeGetter>, IPlacedObject, IPlacedObjectGetter>("LocationRefTypes", ListSemantics.AlignedOrdered, canBeNull: true) },
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
            { "Lock", new ComplexReflectionPropertyHandler<ILockDataGetter, IPlacedObject, IPlacedObjectGetter>("Lock") },
            { "AttachRef", new SimpleReflectionFormLinkPropertyHandler<IPlacedThingGetter, IPlacedObject, IPlacedObjectGetter>("AttachRef") },
            { "Action", new SimpleReflectionFlagPropertyHandler<Mutagen.Bethesda.Skyrim.PlacedObject.ActionFlag, IPlacedObject, IPlacedObjectGetter>("Action") },
            { "LightData", new LightDataHandler() },
            { "Alpha", new ComplexReflectionPropertyHandler<IAlphaGetter, IPlacedObject, IPlacedObjectGetter>("Alpha") },
            { "Patrol", new ComplexReflectionPropertyHandler<IPatrolGetter, IPlacedObject, IPlacedObjectGetter>("Patrol") },
            { "MapMarker", new ComplexReflectionPropertyHandler<IMapMarkerGetter, IPlacedObject, IPlacedObjectGetter>("MapMarker") },
            { "Placement", new PlacementHandler() },
            { "VirtualMachineAdapter", new SimpleReflectionVirtualMachineAdapterHandler<IPlacedObject, IPlacedObjectGetter>() },
            { "EnableParent", new ComplexReflectionPropertyHandler<IEnableParentGetter, IPlacedObject, IPlacedObjectGetter>("EnableParent") },
            { "WaterVelocity", new ComplexReflectionPropertyHandler<IWaterVelocityGetter, IPlacedObject, IPlacedObjectGetter>("WaterVelocity") },
            { "XCZR", new SimpleReflectionFormLinkPropertyHandler<ILinkedReferenceGetter, IPlacedObject, IPlacedObjectGetter>("XCZR") },
            { "XCZC", new SimpleReflectionFormLinkPropertyHandler<ICellGetter, IPlacedObject, IPlacedObjectGetter>("XCZC") },
            { "XORD", new SimpleReflectionBinaryDataPropertyHandler<IPlacedObject, IPlacedObjectGetter>("XORD") },
            { "RagdollData", new SimpleReflectionBinaryDataPropertyHandler<IPlacedObject, IPlacedObjectGetter>("RagdollData") },
            { "RagdollBipedData", new SimpleReflectionBinaryDataPropertyHandler<IPlacedObject, IPlacedObjectGetter>("RagdollBipedData") },
            { "XWCN", new SimpleReflectionBinaryDataPropertyHandler<IPlacedObject, IPlacedObjectGetter>("XWCN") },
            { "XWCS", new SimpleReflectionBinaryDataPropertyHandler<IPlacedObject, IPlacedObjectGetter>("XWCS") },
            { "XCVL", new SimpleReflectionBinaryDataPropertyHandler<IPlacedObject, IPlacedObjectGetter>("XCVL") },
            { "XCZA", new SimpleReflectionBinaryDataPropertyHandler<IPlacedObject, IPlacedObjectGetter>("XCZA") },
            { "DistantLodData", new SimpleReflectionBinaryDataPropertyHandler<IPlacedObject, IPlacedObjectGetter>("DistantLodData") }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IPlacedObjectGetter placedObjectRecord)
            {
                throw new InvalidOperationException($"Expected IPlacedObjectGetter but got {winningContext.Record.GetType()}");
            }
            return placedObjectRecord
                .ToLink<IPlacedObjectGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IPlacedObject, IPlacedObjectGetter>(state.LinkCache)
                .ToArray();
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}
