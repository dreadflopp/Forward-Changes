using DreadsMashedPatch.Contexts;
using DreadsMashedPatch.Contexts.Interfaces;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;

namespace DreadsMashedPatch.PropertyHandlers.Abstracts;

/// <summary>
/// Shared atomic forwarding behavior for Papyrus script entries.
/// Script name is identity. Flags and the complete property collection are the
/// script value and are replaced together when a new change is accepted.
/// Migration note: VMAD script decision behavior is generalized here for every
/// record type. Record-specific handlers retain only adapter access/construction
/// because those Mutagen surfaces differ; script ownership semantics do not.
/// </summary>
public abstract class AbstractScriptListPropertyHandler : AbstractListPropertyHandler<IScriptEntryGetter>
{
    public override ListSemantics Semantics => ListSemantics.SortedKeyed;

    protected override IReadOnlyList<object?> GetSortKey(IScriptEntryGetter item) => [item.Name];

    public override bool AreValuesEqual(
        List<IScriptEntryGetter>? value1,
        List<IScriptEntryGetter>? value2)
    {
        if (value1 == null && value2 == null) return true;
        if (value1 == null || value2 == null) return false;

        return AreScriptCollectionsEqual(value1, value2);
    }

    protected internal static bool AreScriptCollectionsEqual(
        IReadOnlyList<IScriptEntryGetter> value1,
        IReadOnlyList<IScriptEntryGetter> value2)
    {
        if (value1.Count != value2.Count) return false;

        var unmatched = value2.ToList();
        foreach (var script in value1)
        {
            var matchIndex = unmatched.FindIndex(candidate => AreScriptsEqual(script, candidate));
            if (matchIndex < 0) return false;
            unmatched.RemoveAt(matchIndex);
        }

        return unmatched.Count == 0;
    }

    public override void InitializeContext(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> originalContext,
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPropertyContext propertyContext)
    {
        if (propertyContext is not ListPropertyContext<IScriptEntryGetter> listPropertyContext)
        {
            throw new InvalidOperationException($"Error: Property context is not a list property context for {PropertyName}");
        }

        var originalValue = GetValue(originalContext.Record);
        var scripts = originalValue ?? [];
        listPropertyContext.OriginalValueContexts = scripts
            .Select(script => CreateValueContext(script, originalContext.ModKey.ToString()))
            .ToList();
        listPropertyContext.ForwardValueContexts = scripts
            .Select(script => CreateValueContext(script, originalContext.ModKey.ToString()))
            .ToList();
        listPropertyContext.AlignmentRows = [];
        listPropertyContext.NextAlignmentRowId = 0;
        // Adapter-backed script getters return null when the parent VMAD is absent.
        // Preserve that observed state instead of normalizing it to a present empty list.
        listPropertyContext.CanBeNull = CanBeNull || originalValue == null;
        listPropertyContext.OriginalIsNull = originalValue == null;
        listPropertyContext.ForwardIsNull = listPropertyContext.OriginalIsNull;
        listPropertyContext.ForwardPresenceOwnerMod = originalContext.ModKey.ToString();
        listPropertyContext.IsResolved = false;
    }

    protected override bool IsItemEqual(IScriptEntryGetter? item1, IScriptEntryGetter? item2)
    {
        if (item1 == null && item2 == null) return true;
        if (item1 == null || item2 == null) return false;

        // Script content must never be part of list identity: doing so turns a
        // modification into a removal plus an addition and can create duplicates.
        return string.Equals(item1.Name, item2.Name, StringComparison.Ordinal);
    }

    protected override void ProcessHandlerSpecificLogic(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
        ListPropertyContext<IScriptEntryGetter> listPropertyContext,
        List<IScriptEntryGetter> recordItems,
        List<ListPropertyValueContext<IScriptEntryGetter>> currentForwardItems)
    {
        var recordMod = state.LoadOrder[context.ModKey].Mod;
        if (recordMod == null) return;

        var originalScripts = listPropertyContext.OriginalValueContexts?
            .Select(item => item.Value)
            .ToList() ?? [];

        foreach (var recordScript in recordItems)
        {
            var forwardContext = currentForwardItems.FirstOrDefault(item =>
                !item.IsRemoved && IsItemEqual(item.Value, recordScript));
            if (forwardContext == null) continue;

            var originalScript = originalScripts.FirstOrDefault(script => IsItemEqual(script, recordScript));
            ApplyAtomicScriptChange(
                forwardContext,
                recordScript,
                originalScript,
                context.ModKey.ToString(),
                owner => HasPermissionsToModify(recordMod, owner));
        }
    }

    /// <summary>
    /// Applies the standard three-way forwarding rule to one same-name script.
    /// A value different from both original and forwarded is a new atomic change
    /// and is always accepted. Returning to the original value is a reversion and
    /// requires permission from the owner of the currently forwarded value.
    /// </summary>
    protected void ApplyAtomicScriptChange(
        ListPropertyValueContext<IScriptEntryGetter> forwardContext,
        IScriptEntryGetter recordScript,
        IScriptEntryGetter? originalScript,
        string newOwner,
        Func<string?, bool> hasPermission)
    {
        var forwardScript = forwardContext.Value;
        if (AreScriptsEqual(forwardScript, recordScript)) return;

        var isReversion = originalScript != null && AreScriptsEqual(recordScript, originalScript);
        if (isReversion && !hasPermission(forwardContext.OwnerMod))
        {
            LogCollector.Add(PropertyName,
                $"[{PropertyName}] {newOwner}: Reversion of script '{recordScript.Name}' denied; current value is owned by {forwardContext.OwnerMod}");
            return;
        }

        var previousOwner = forwardContext.OwnerMod;
        forwardContext.Value = PapyrusUnusedDataPolicy.CopyScript(recordScript, forwardScript);
        forwardContext.OwnerMod = newOwner;
        LogCollector.Add(PropertyName,
            isReversion
                ? $"[{PropertyName}] {newOwner}: Reversion of script '{recordScript.Name}' allowed (previous owner: {previousOwner})"
                : $"[{PropertyName}] {newOwner}: New atomic value for script '{recordScript.Name}' accepted (previous owner: {previousOwner})");
    }

    protected internal static bool AreScriptsEqual(IScriptEntryGetter script1, IScriptEntryGetter script2)
    {
        if (!string.Equals(script1.Name, script2.Name, StringComparison.Ordinal) ||
            script1.Flags != script2.Flags ||
            script1.Properties.Count != script2.Properties.Count)
        {
            return false;
        }

        var unmatched = script2.Properties.ToList();
        foreach (var property in script1.Properties)
        {
            var matchIndex = unmatched.FindIndex(candidate => AreScriptPropertiesEqual(property, candidate));
            if (matchIndex < 0) return false;
            unmatched.RemoveAt(matchIndex);
        }

        return unmatched.Count == 0;
    }

    protected internal static bool AreScriptPropertiesEqual(
        IScriptPropertyGetter property1,
        IScriptPropertyGetter property2)
    {
        if (!string.Equals(property1.Name, property2.Name, StringComparison.Ordinal) ||
            property1.Flags != property2.Flags)
        {
            return false;
        }

        return property1 switch
        {
            IScriptBoolPropertyGetter value1 when property2 is IScriptBoolPropertyGetter value2 =>
                value1.Data == value2.Data,
            IScriptIntPropertyGetter value1 when property2 is IScriptIntPropertyGetter value2 =>
                value1.Data == value2.Data,
            IScriptFloatPropertyGetter value1 when property2 is IScriptFloatPropertyGetter value2 =>
                value1.Data.Equals(value2.Data),
            IScriptStringPropertyGetter value1 when property2 is IScriptStringPropertyGetter value2 =>
                string.Equals(value1.Data, value2.Data, StringComparison.Ordinal),
            IScriptObjectPropertyGetter value1 when property2 is IScriptObjectPropertyGetter value2 =>
                AreObjectValuesEqual(value1, value2),
            IScriptBoolListPropertyGetter value1 when property2 is IScriptBoolListPropertyGetter value2 =>
                value1.Data.SequenceEqual(value2.Data),
            IScriptIntListPropertyGetter value1 when property2 is IScriptIntListPropertyGetter value2 =>
                value1.Data.SequenceEqual(value2.Data),
            IScriptFloatListPropertyGetter value1 when property2 is IScriptFloatListPropertyGetter value2 =>
                value1.Data.SequenceEqual(value2.Data),
            IScriptStringListPropertyGetter value1 when property2 is IScriptStringListPropertyGetter value2 =>
                value1.Data.SequenceEqual(value2.Data, StringComparer.Ordinal),
            IScriptObjectListPropertyGetter value1 when property2 is IScriptObjectListPropertyGetter value2 =>
                AreObjectListsEqual(value1.Objects, value2.Objects),
            _ => false
        };
    }

    protected override string FormatItem(IScriptEntryGetter? item)
    {
        return item == null
            ? "null"
            : $"Script({item.Name}, Flags: {item.Flags}, Properties: {item.Properties.Count})";
    }

    private static ListPropertyValueContext<IScriptEntryGetter> CreateValueContext(
        IScriptEntryGetter script,
        string owner)
        => new(script, owner);

    private static bool AreObjectValuesEqual(
        IScriptObjectPropertyGetter value1,
        IScriptObjectPropertyGetter value2)
    {
        return string.Equals(value1.Name, value2.Name, StringComparison.Ordinal) &&
               value1.Flags == value2.Flags &&
               value1.Object.FormKey == value2.Object.FormKey &&
               value1.Alias == value2.Alias;
    }

    private static bool AreObjectListsEqual(
        IReadOnlyList<IScriptObjectPropertyGetter> list1,
        IReadOnlyList<IScriptObjectPropertyGetter> list2)
    {
        return list1.Count == list2.Count &&
               list1.Zip(list2).All(pair => AreObjectValuesEqual(pair.First, pair.Second));
    }

}
