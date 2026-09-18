using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.PlacedNpc;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    // Migration note:
    // - Generalized: placed-NPC location links and ragdoll byte payloads use shared reflection handlers.
    //   XLRL/LocationReference intentionally stays on this conflict-aware path: a newly added value that
    //   survives into the winner already produces no patch, while a later removal remains a real conflict,
    //   matching xEdit's cpBenignIfAdded behavior.
    // - Specialized: NPC placement/list behavior remains specialized; Mutagen 0.54.4 now exposes LocationReference as ILocationGetter.
    // - Rationale: only the generated link target changed, so no record-specific behavior needed replacement.
    public class PlacedNpcRecordHandler : AbstractRecordHandler
    {
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
            { "ActivateParents", new ComplexReflectionPropertyHandler<IActivateParentsGetter, IPlacedNpc, IPlacedNpcGetter>("ActivateParents") },
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
            { "Placement", new ComplexReflectionPropertyHandler<IPlacementGetter, IPlacedNpc, IPlacedNpcGetter>("Placement") },
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
