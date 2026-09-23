using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using DreadsMashedPatch.PropertyHandlers.PlacedNpc;
using DreadsMashedPatch.PropertyHandlers.General;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using System;

namespace DreadsMashedPatch.RecordHandlers
{
    // Migration note:
    // - Generalized: placed-NPC location links and ragdoll byte payloads use shared reflection handlers.
    //   XLRL/LocationReference intentionally stays on this conflict-aware path: a newly added value that
    //   survives into the winner already produces no patch. A later omission may remove it only when that
    //   mod has the adding mod as an actual or configured virtual master; otherwise the addition is retained.
    // - Specialized: Placement is one cohesive value with xEdit-precision position equality, circular normalized-angle
    //   equality, and degree diagnostics. A recognized safe UDR keeps Initially Disabled, Placement, and EnableParent
    //   on the snapshot selected by the approved flag handler. ActivateParents and NPC list behavior remain specialized.
    // - Intentionally non-migrated: ordinary Initially Disabled references are not treated as UDRs, and
    //   LocationReference remains independently conflict-resolved because it is not part of the safe-disable bundle.
    // - Rationale: exact float equality creates invisible conflicts, and independent UDR fields can otherwise produce
    //   contradictory hybrids. Mutagen 0.54.4 exposes LocationReference as ILocationGetter.
    public class PlacedNpcRecordHandler : AbstractRecordHandler
    {
        protected override PropertyForwardingCoordination CoordinateForwardedProperties(
            IReadOnlyList<IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>> recordContexts)
        {
            return PlacedReferenceUdrCoordinator.Coordinate(recordContexts, PropertyContexts);
        }

        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
            { "EditorID", new EditorIDHandler() },
            { "MajorRecordFlagsRaw", new MajorRecordFlagsRawHandler() },
            { "SkyrimMajorRecordFlags", new SkyrimMajorRecordFlagsHandler() },
            { "MajorFlags", new MajorFlagsHandler() },
            { "Base", new SimpleReflectionFormLinkPropertyHandler<INpcGetter, IPlacedNpc, IPlacedNpcGetter>("Base") },
            { "EncounterZone", new SimpleReflectionFormLinkPropertyHandler<IEncounterZoneGetter, IPlacedNpc, IPlacedNpcGetter>("EncounterZone") },
            { "RagdollData", new SimpleReflectionBinaryDataPropertyHandler<IPlacedNpc, IPlacedNpcGetter>("RagdollData") },
            { "RagdollBipedData", new SimpleReflectionBinaryDataPropertyHandler<IPlacedNpc, IPlacedNpcGetter>("RagdollBipedData") },
            { "Patrol", new ComplexReflectionPropertyHandler<IPatrolGetter, IPlacedNpc, IPlacedNpcGetter>("Patrol") },
            { "LevelModifier", new SimpleReflectionPropertyHandler<Level?, IPlacedNpc, IPlacedNpcGetter>("LevelModifier") },
            { "MerchantContainer", new SimpleReflectionFormLinkPropertyHandler<IPlacedObjectGetter, IPlacedNpc, IPlacedNpcGetter>("MerchantContainer") },
            { "Count", new SimpleReflectionPropertyHandler<int?, IPlacedNpc, IPlacedNpcGetter>("Count") },
            { "Radius", new SimpleReflectionPropertyHandler<float?, IPlacedNpc, IPlacedNpcGetter>("Radius") },
            { "Health", new SimpleReflectionPropertyHandler<float?, IPlacedNpc, IPlacedNpcGetter>("Health") },
            { "LinkedReferences", new SimpleReflectionListPropertyHandler<ILinkedReferencesGetter, IPlacedNpc, IPlacedNpcGetter>("LinkedReferences", ListSemantics.SortedKeyed, keySelector: entry => entry.KeywordOrReference.FormKey) },
            { "ActivateParents", new GeneratedCopyReflectionPropertyHandler<IActivateParentsGetter, ActivateParents, IPlacedNpc, IPlacedNpcGetter>("ActivateParents", value => value.DeepCopy(), ActivateParentsMixIn.Equals) },
            { "LinkedReferenceColor", new ComplexReflectionPropertyHandler<ILinkedReferenceColorGetter, IPlacedNpc, IPlacedNpcGetter>("LinkedReferenceColor") },
            { "PersistentLocation", new SimpleReflectionFormLinkPropertyHandler<ILocationGetter, IPlacedNpc, IPlacedNpcGetter>("PersistentLocation") },
            { "LocationReference", new SimpleReflectionFormLinkPropertyHandler<ILocationGetter, IPlacedNpc, IPlacedNpcGetter>("LocationReference") },
            { "IsIgnoredBySandbox", new SimpleReflectionPropertyHandler<bool, IPlacedNpc, IPlacedNpcGetter>("IsIgnoredBySandbox") },
            { "LocationRefTypes", new SimpleReflectionListPropertyHandler<IFormLinkGetter<ILocationReferenceTypeGetter>, IPlacedNpc, IPlacedNpcGetter>("LocationRefTypes", ListSemantics.AlignedOrdered, canBeNull: true) },
            { "HeadTrackingWeight", new SimpleReflectionPropertyHandler<float?, IPlacedNpc, IPlacedNpcGetter>("HeadTrackingWeight") },
            { "Horse", new SimpleReflectionFormLinkPropertyHandler<IPlacedNpcGetter, IPlacedNpc, IPlacedNpcGetter>("Horse") },
            { "FavorCost", new SimpleReflectionPropertyHandler<float?, IPlacedNpc, IPlacedNpcGetter>("FavorCost") },
            { "EnableParent", new ComplexReflectionPropertyHandler<IEnableParentGetter, IPlacedNpc, IPlacedNpcGetter>("EnableParent") },
            { "Owner", new SimpleReflectionFormLinkPropertyHandler<IOwnerGetter, IPlacedNpc, IPlacedNpcGetter>("Owner") },
            { "FactionRank", new SimpleReflectionPropertyHandler<int?, IPlacedNpc, IPlacedNpcGetter>("FactionRank") },
            { "Emittance", new SimpleReflectionFormLinkPropertyHandler<IEmittanceGetter, IPlacedNpc, IPlacedNpcGetter>("Emittance") },
            { "MultiBoundReference", new SimpleReflectionFormLinkPropertyHandler<IPlacedObjectGetter, IPlacedNpc, IPlacedNpcGetter>("MultiBoundReference") },
            { "IsIgnoredBySandbox2", new SimpleReflectionPropertyHandler<bool, IPlacedNpc, IPlacedNpcGetter>("IsIgnoredBySandbox2") },
            { "Scale", new SimpleReflectionPropertyHandler<float?, IPlacedNpc, IPlacedNpcGetter>("Scale") },
            { "Placement", new PlacementHandler() },
            { "VirtualMachineAdapter", new VirtualMachineAdapterHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (winningContext.Record is not IPlacedNpcGetter placedNpcRecord)
            {
                throw new InvalidOperationException($"Expected IPlacedNpcGetter but got {winningContext.Record.GetType()}");
            }
            var contexts = placedNpcRecord
                .ToLink<IPlacedNpcGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, IPlacedNpc, IPlacedNpcGetter>(state.LinkCache)
                .ToArray();

            return contexts;
        }

        // GetOverrideRecord and ApplyForwardedProperties are now handled by the base class
        // The base class automatically handles flag property coordination
    }
}
