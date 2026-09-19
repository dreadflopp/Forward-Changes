using DreadsMashedPatch.Contexts.Interfaces;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;

namespace DreadsMashedPatch.PropertyHandlers.ImpactDataSet;

public sealed class ImpactsHandler : IPropertyHandler<List<IImpactDataGetter>>
{
    public string PropertyName => "Impacts";

    public bool RequiresFullLoadOrderProcessing => true;

    public List<IImpactDataGetter>? GetValue(IMajorRecordGetter record)
    {
        return record is IImpactDataSetGetter impactDataSet
            ? impactDataSet.Impacts.ToList()
            : null;
    }

    public void SetValue(IMajorRecord record, List<IImpactDataGetter>? value)
    {
        if (record is not IImpactDataSet impactDataSet)
        {
            return;
        }

        impactDataSet.Impacts.Clear();
        if (value == null)
        {
            return;
        }

        foreach (var entry in value)
        {
            impactDataSet.Impacts.Add(entry.DeepCopy());
        }
    }

    public bool AreValuesEqual(List<IImpactDataGetter>? value1, List<IImpactDataGetter>? value2)
    {
        if (value1 == null || value2 == null)
        {
            return value1 == null && value2 == null;
        }

        var valid1 = TryCreateValues(value1, out var values1, out _);
        var valid2 = TryCreateValues(value2, out var values2, out _);
        if (!valid1 || !valid2)
        {
            return SequenceEqual(values1, values2);
        }

        if (values1.Count != values2.Count)
        {
            return false;
        }

        var secondByMaterial = values2.ToDictionary(value => value.Material);
        return values1.All(value =>
            secondByMaterial.TryGetValue(value.Material, out var other)
            && value.Impact.Equals(other.Impact));
    }

    public IPropertyContext CreatePropertyContext() => new ImpactDataMergeContext();

    public void InitializeContext(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> originalContext,
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPropertyContext propertyContext)
    {
        var mergeContext = GetMergeContext(propertyContext);
        var original = GetRequiredValue(originalContext.Record);
        var winning = GetRequiredValue(winningContext.Record);

        var originalValid = TryCreateValues(original, out var originalValues, out var originalError);
        var winningValid = TryCreateValues(winning, out var winningValues, out var winningError);
        mergeContext.Initialize(originalValues, originalContext.ModKey, winningValues);

        if (!originalValid)
        {
            UseWinningFallback(mergeContext, originalContext.ModKey, originalError!);
        }
        else if (!winningValid)
        {
            UseWinningFallback(mergeContext, winningContext.ModKey, winningError!);
        }
    }

    public void UpdatePropertyContext(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
        IPropertyContext propertyContext)
    {
        var mergeContext = GetMergeContext(propertyContext);
        if (mergeContext.UseWinningValue)
        {
            return;
        }

        var recordValue = GetRequiredValue(context.Record);
        if (!TryCreateValues(recordValue, out var values, out var error))
        {
            UseWinningFallback(mergeContext, context.ModKey, error!);
            return;
        }

        var recordMod = state.LoadOrder[context.ModKey].Mod;
        if (recordMod == null)
        {
            UseWinningFallback(mergeContext, context.ModKey, "the source mod was unavailable");
            return;
        }

        mergeContext.Apply(
            context.ModKey,
            recordMod.MasterReferences.Select(reference => reference.Master),
            values,
            message => LogCollector.Add(PropertyName, $"[{PropertyName}] {context.ModKey}: {message}"));
    }

    public string FormatValue(object? value)
    {
        var values = ConvertObjectValue(value);
        if (values == null)
        {
            return "null";
        }

        if (values.Count == 0)
        {
            return "Empty";
        }

        return string.Join(", ", values.Select(FormatEntry));
    }

    void IPropertyHandler.SetValue(IMajorRecord record, object? value)
    {
        SetValue(record, ConvertObjectValue(value));
    }

    object? IPropertyHandler.GetValue(IMajorRecordGetter record) => GetValue(record);

    bool IPropertyHandler.AreValuesEqual(object? value1, object? value2)
    {
        return AreValuesEqual(ConvertObjectValue(value1), ConvertObjectValue(value2));
    }

    private List<IImpactDataGetter> GetRequiredValue(IMajorRecordGetter record)
    {
        return GetValue(record)
            ?? throw new InvalidOperationException($"Record does not implement IImpactDataSetGetter for {PropertyName}");
    }

    private static ImpactDataMergeContext GetMergeContext(IPropertyContext propertyContext)
    {
        return propertyContext as ImpactDataMergeContext
            ?? throw new InvalidOperationException("Property context is not an ImpactData merge context");
    }

    private void UseWinningFallback(
        ImpactDataMergeContext context,
        ModKey sourceMod,
        string reason)
    {
        context.FallbackToWinning();
        LogCollector.Add(
            PropertyName,
            $"[{PropertyName}] {sourceMod}: Cannot safely merge keyed entries because {reason}. Keeping the winning list unchanged.");
    }

    internal static bool TryCreateValues(
        IEnumerable<IImpactDataGetter> entries,
        out List<ImpactDataValue> values,
        out string? error)
    {
        values = [];
        error = null;
        var materials = new HashSet<FormKey>();

        foreach (var entry in entries)
        {
            if (entry == null)
            {
                error ??= "an entry is null";
                continue;
            }

            var material = entry.Material.FormKey;
            var value = new ImpactDataValue(material, entry.Impact.FormKey);
            values.Add(value);

            if (material.IsNull)
            {
                error ??= "an entry has a null Material key";
            }

            if (!materials.Add(material))
            {
                error ??= $"Material {material} occurs more than once";
            }
        }

        return error == null;
    }

    private static bool SequenceEqual(
        IReadOnlyList<ImpactDataValue> value1,
        IReadOnlyList<ImpactDataValue> value2)
    {
        return value1.Count == value2.Count
            && value1.Zip(value2, (left, right) => left.Equals(right)).All(equal => equal);
    }

    private static List<IImpactDataGetter>? ConvertObjectValue(object? value)
    {
        return value switch
        {
            null => null,
            List<IImpactDataGetter> typed => typed,
            IEnumerable<IImpactDataGetter> typedEnumerable => typedEnumerable.ToList(),
            IEnumerable<object> objects => objects.Cast<IImpactDataGetter>().ToList(),
            _ => throw new InvalidCastException($"Cannot convert {value.GetType()} to an ImpactData list")
        };
    }

    private static string FormatEntry(IImpactDataGetter entry)
    {
        return $"Material: {entry.Material.FormKey}, Impact: {entry.Impact.FormKey}";
    }
}

internal readonly record struct ImpactDataValue(FormKey Material, FormKey Impact)
{
    public IImpactDataGetter ToGetter()
    {
        var result = new ImpactData();
        result.Material.SetTo(Material);
        result.Impact.SetTo(Impact);
        return result;
    }

    public override string ToString() => $"Material: {Material}, Impact: {Impact}";
}

internal sealed class ImpactDataMergeContext : IPropertyContext
{
    private readonly List<ImpactDataEntryContext> _slots = [];
    private List<ImpactDataValue> _winningValues = [];

    public bool IsResolved { get; set; }

    public bool UseWinningValue { get; private set; }

    public void Initialize(
        IReadOnlyList<ImpactDataValue> originalValues,
        ModKey originalOwner,
        IReadOnlyList<ImpactDataValue> winningValues)
    {
        _slots.Clear();
        foreach (var value in originalValues)
        {
            _slots.Add(new ImpactDataEntryContext(
                value.Material,
                value.Impact,
                value.Impact,
                wasOriginal: true,
                originalOwner));
        }

        _winningValues = winningValues.ToList();
        UseWinningValue = false;
        IsResolved = false;
    }

    public void FallbackToWinning()
    {
        UseWinningValue = true;
        IsResolved = true;
    }

    public void Apply(
        ModKey currentMod,
        IEnumerable<ModKey> masters,
        IReadOnlyList<ImpactDataValue> recordValues,
        Action<string>? log = null)
    {
        if (UseWinningValue)
        {
            return;
        }

        var masterKeys = masters.ToHashSet();
        var recordByMaterial = recordValues.ToDictionary(value => value.Material);

        foreach (var slot in _slots.Where(slot => !slot.IsRemoved).ToList())
        {
            if (recordByMaterial.ContainsKey(slot.Material))
            {
                continue;
            }

            if (HasPermission(currentMod, masterKeys, slot.OwnerMod))
            {
                var oldOwner = slot.OwnerMod;
                slot.IsRemoved = true;
                slot.OwnerMod = currentMod;
                log?.Invoke($"Removing material {slot.Material} (was owned by {oldOwner}, new owner: {currentMod}) Success");
            }
            else
            {
                log?.Invoke($"Cannot remove material {slot.Material} - no permission. Current owner: {slot.OwnerMod}");
            }
        }

        foreach (var recordValue in recordValues)
        {
            var slot = _slots.FirstOrDefault(candidate => candidate.Material.Equals(recordValue.Material));
            if (slot == null)
            {
                _slots.Add(new ImpactDataEntryContext(
                    recordValue.Material,
                    recordValue.Impact,
                    FormKey.Null,
                    wasOriginal: false,
                    currentMod));
                log?.Invoke($"Adding material mapping {recordValue} (new owner: {currentMod}) Success");
                continue;
            }

            if (slot.IsRemoved)
            {
                if (HasPermission(currentMod, masterKeys, slot.OwnerMod))
                {
                    var oldOwner = slot.OwnerMod;
                    slot.Impact = recordValue.Impact;
                    slot.IsRemoved = false;
                    slot.OwnerMod = currentMod;
                    log?.Invoke($"Adding back material mapping {recordValue} (was owned by {oldOwner}, new owner: {currentMod}) Success");
                }
                else
                {
                    log?.Invoke($"Cannot add back material {recordValue.Material} - no permission (owned by {slot.OwnerMod})");
                }

                continue;
            }

            if (slot.Impact.Equals(recordValue.Impact))
            {
                continue;
            }

            var isReversion = slot.WasOriginal && recordValue.Impact.Equals(slot.OriginalImpact);
            if (isReversion && !HasPermission(currentMod, masterKeys, slot.OwnerMod))
            {
                log?.Invoke($"Reversion for material {recordValue.Material}: {slot.Impact} -> {recordValue.Impact} Permission denied (owned by {slot.OwnerMod})");
                continue;
            }

            var previousImpact = slot.Impact;
            slot.Impact = recordValue.Impact;
            slot.OwnerMod = currentMod;
            log?.Invoke($"{(isReversion ? "Reversion" : "Replacement")} for material {recordValue.Material}: {previousImpact} -> {recordValue.Impact} Success");
        }
    }

    public object GetForwardValue()
    {
        var values = UseWinningValue
            ? _winningValues
            : _slots
                .Where(slot => !slot.IsRemoved)
                .Select(slot => new ImpactDataValue(slot.Material, slot.Impact))
                .ToList();

        return values.Select(value => value.ToGetter()).ToList();
    }

    internal IReadOnlyList<ImpactDataValue> GetForwardValues()
    {
        return UseWinningValue
            ? _winningValues.ToList()
            : _slots
                .Where(slot => !slot.IsRemoved)
                .Select(slot => new ImpactDataValue(slot.Material, slot.Impact))
                .ToList();
    }

    private static bool HasPermission(
        ModKey currentMod,
        IReadOnlySet<ModKey> masters,
        ModKey ownerMod)
    {
        return currentMod.Equals(ownerMod)
               || PatcherSettings.HasMasterOrVirtualMaster(currentMod, masters, ownerMod);
    }

    private sealed class ImpactDataEntryContext(
        FormKey material,
        FormKey impact,
        FormKey originalImpact,
        bool wasOriginal,
        ModKey ownerMod)
    {
        public FormKey Material { get; } = material;
        public FormKey Impact { get; set; } = impact;
        public FormKey OriginalImpact { get; } = originalImpact;
        public bool WasOriginal { get; } = wasOriginal;
        public ModKey OwnerMod { get; set; } = ownerMod;
        public bool IsRemoved { get; set; }
    }
}
