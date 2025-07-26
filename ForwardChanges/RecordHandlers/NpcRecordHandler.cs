using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Npc;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using System;

namespace ForwardChanges.RecordHandlers
{
    public class NpcRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
                { "Name", new NamePropertyHandler() },
                { "DeathItem", new NpcDeathItemPropertyHandler() },
                { "CombatOverridePackageList", new NpcCombatOverridePackageListHandler() },
                { "SpectatorOverridePackageList", new NpcSpectatorOverridePackageListHandler() },
                { "Configuration.Flags", new NpcProtectionFlagsHandler() },
                { "Configuration.MagickaOffset", new NpcConfigurationMagickaOffsetPropertyHandler() },
                { "EditorID", new EditorIDPropertyHandler() },
                { "Class", new NpcClassPropertyHandler() },
                { "AIData.Confidence", new NpcAIDataConfidencePropertyHandler() },
                { "ObserveDeadBodyOverridePackageList", new NpcObserveDeadBodyOverridePackageListHandler() },
                { "Factions", new NpcFactionListPropertyHandler() },
                { "Packages", new NpcPackageListPropertyHandler() },
                { "ActorEffect", new NpcActorEffectsListPropertyHandler() },
                { "VirtualMachineAdapter.Scripts", new NpcVirtualMachineAdapterScriptsListPropertyHandler() },
                { "Items", new NpcItemListPropertyHandler() },
                { "Keywords", new KeywordListPropertyHandler() }
        };

        public override IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            return winningContext.Record
                .ToLink<INpcGetter>()
                .ResolveAllContexts<ISkyrimMod, ISkyrimModGetter, INpc, INpcGetter>(state.LinkCache)
                .ToArray();
        }

        public override IMajorRecord GetOverrideRecord(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            return state.PatchMod.Npcs.GetOrAddAsOverride(winningContext.Record);
        }

        public override void ApplyForwardedProperties(IMajorRecord record, Dictionary<string, object?> propertiesToForward)
        {
            foreach (var (propertyName, value) in propertiesToForward)
            {
                if (PropertyHandlers.TryGetValue(propertyName, out var handler))
                {
                    Console.WriteLine($"[{propertyName}] Applying value: {handler.FormatValue(value)}, Type: {value?.GetType()}");
                    if (value != null)
                    {
                        handler.SetValue(record, value);
                    }
                }
            }
        }
    }
}