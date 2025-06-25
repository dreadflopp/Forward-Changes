using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers;
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
                { "SpectatorOverridePackageList", new SpectatorOverridePackageListHandler() },
                { "Configuration.Flags", new NpcProtectionFlagsHandler() },
                { "Configuration.MagickaOffset", new NpcConfigurationMagickaOffsetPropertyHandler() },
                { "EditorID", new EditorIDPropertyHandler() },
                { "Class", new NpcClassPropertyHandler() },
                { "AIData.Confidence", new AIDataConfidencePropertyHandler() },
                { "ObserveDeadBodyOverridePackageList", new NpcObserveDeadBodyOverridePackageListHandler() },
                { "Factions", new NpcFactionListPropertyHandler() },
                { "Packages", new NpcPackageListPropertyHandler() },
                { "ActorEffect", new NpcActorEffectsListPropertyHandler() },
                { "VirtualMachineAdapter.Scripts", new NpcVirtualMachineAdapterScriptsListPropertyHandler() }
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
                    Console.WriteLine($"[{propertyName}] Applying value: {value}, Type: {value?.GetType()}");
                    if (value != null)
                    {
                        handler.SetValue(record, value);
                    }
                }
            }
        }
    }
}