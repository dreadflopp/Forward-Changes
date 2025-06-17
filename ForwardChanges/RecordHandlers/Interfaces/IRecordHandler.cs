using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.RecordHandlers.Interfaces
{
    public interface IRecordHandler
    {
        void Process(IPatcherState<ISkyrimMod, ISkyrimModGetter> state);
        bool CanHandle(IMajorRecord record);
        bool ShouldBreakEarly(IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] recordContexts);

        // Protected methods that are part of the contract
        IEnumerable<IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>> GetWinningContexts(
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state);

        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter>[] GetRecordContexts(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state);

        IMajorRecord GetOverrideRecord(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state);

        void ApplyForwardedProperties(IMajorRecord record, Dictionary<string, object?> propertiesToForward);

        // Property that all record handlers must implement
        Dictionary<string, IPropertyHandlerBase> PropertyHandlers { get; }
    }
}
