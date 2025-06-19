using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.BasicPropertyHandlers;
using ForwardChanges.RecordHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers;

namespace ForwardChanges.RecordHandlers
{
    public class NpcRecordHandler : AbstractRecordHandler
    {
        public override Dictionary<string, IPropertyHandler> PropertyHandlers { get; } = new()
        {
                { "Name", new NamePropertyHandler() },
            //    { "DeathItem", new DeathItemPropertyHandler() },
            //    { "CombatOverridePackageList", new CombatOverridePackageListHandler() },
            //    { "SpectatorOverridePackageList", new SpectatorOverridePackageListHandler() },
            //    { "Configuration.Flags", new ProtectionFlagsHandler() },
            //    { "EditorID", new EditorIDPropertyHandler() },
            //    { "Class", new ClassPropertyHandler() },
            //    { "AIData.Confidence", new AIDataConfidencePropertyHandler() },
            //    { "Factions", new FactionListPropertyHandler() }
        };

        public override bool CanHandle(IMajorRecord record)
        {
            return record is INpc;
        }

        public override IEnumerable<IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>> GetWinningContexts(
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            return state.LoadOrder.PriorityOrder
                .WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, INpc, INpcGetter>(state.LinkCache);
        }

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