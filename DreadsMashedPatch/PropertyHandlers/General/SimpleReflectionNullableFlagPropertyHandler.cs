using System.Reflection;
using DreadsMashedPatch.Contexts;
using DreadsMashedPatch.Contexts.Interfaces;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;

namespace DreadsMashedPatch.PropertyHandlers.General;

/// <summary>
/// Resolves an optional enum flag field without collapsing an absent field into a
/// present zero value. Named bits and field presence retain independent ownership.
/// </summary>
public sealed class SimpleReflectionNullableFlagPropertyHandler<TFlag, TRecord, TRecordGetter>
    : IPropertyHandler<TFlag?>
    where TFlag : struct, Enum
    where TRecord : class, IMajorRecord
    where TRecordGetter : class, IMajorRecordGetter
{
    private static readonly TFlag[] AllFlags = Enum.GetValues<TFlag>()
        .Where(flag => Convert.ToInt64(flag) != 0)
        .ToArray();

    private static readonly long KnownMask = AllFlags.Aggregate(
        0L,
        (mask, flag) => mask | Convert.ToInt64(flag));

    private readonly PropertyInfo _getterProperty;
    private readonly PropertyInfo _setterProperty;
    private readonly bool _preserveUnknownBits;

    public SimpleReflectionNullableFlagPropertyHandler(
        string propertyName,
        bool preserveUnknownBits = false)
    {
        PropertyName = propertyName;
        _preserveUnknownBits = preserveUnknownBits;
        _getterProperty = ReflectionPropertyResolver.Find(typeof(TRecordGetter), propertyName)
            ?? throw new ArgumentException(
                $"Property '{propertyName}' not found on {typeof(TRecordGetter).Name}");
        _setterProperty = ReflectionPropertyResolver.Find(typeof(TRecord), propertyName)
            ?? throw new ArgumentException(
                $"Property '{propertyName}' not found on {typeof(TRecord).Name}");
    }

    public string PropertyName { get; }
    public bool RequiresFullLoadOrderProcessing => true;

    public TFlag? GetValue(IMajorRecordGetter record)
    {
        if (record is not TRecordGetter typedRecord)
        {
            throw new InvalidOperationException(
                $"Expected {typeof(TRecordGetter).Name} but got {record.GetType().Name}");
        }

        return _getterProperty.GetValue(typedRecord) is TFlag flags ? flags : null;
    }

    public void SetValue(IMajorRecord record, TFlag? value)
    {
        if (record is not TRecord typedRecord)
        {
            throw new InvalidOperationException(
                $"Expected {typeof(TRecord).Name} but got {record.GetType().Name}");
        }

        if (!value.HasValue)
        {
            _setterProperty.SetValue(typedRecord, null);
            return;
        }

        object valueToSet = value.Value;
        if (_preserveUnknownBits && _setterProperty.GetValue(typedRecord) is TFlag currentFlags)
        {
            var currentBits = Convert.ToInt64(currentFlags);
            var requestedBits = Convert.ToInt64(value.Value);
            valueToSet = (TFlag)Enum.ToObject(
                typeof(TFlag),
                (currentBits & ~KnownMask) | (requestedBits & KnownMask));
        }

        _setterProperty.SetValue(typedRecord, valueToSet);
    }

    public bool AreValuesEqual(TFlag? value1, TFlag? value2)
    {
        if (value1.HasValue != value2.HasValue)
        {
            return false;
        }

        if (!value1.HasValue)
        {
            return true;
        }

        return (Convert.ToInt64(value1.Value) & KnownMask) ==
               (Convert.ToInt64(value2!.Value) & KnownMask);
    }

    public IPropertyContext CreatePropertyContext() =>
        new NullableFlagPropertyContext<TFlag>();

    public void InitializeContext(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> originalContext,
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
        IPropertyContext propertyContext)
    {
        var context = GetContext(propertyContext);
        var original = GetValue(originalContext.Record);
        var originalFlags = original.GetValueOrDefault();

        context.OriginalHasValue = original.HasValue;
        context.ForwardHasValue = original.HasValue;
        context.PresenceOwnerMod = originalContext.ModKey.ToString();
        context.OriginalFlagContexts = AllFlags
            .Select(flag => new FlagPropertyValueContext<TFlag>(
                flag,
                IsSet(originalFlags, flag),
                originalContext.ModKey.ToString()))
            .ToList();
        context.ForwardFlagContexts = context.OriginalFlagContexts
            .Select(flag => new FlagPropertyValueContext<TFlag>(
                flag.Flag,
                flag.IsSet,
                flag.OwnerMod))
            .ToList();
        context.IsResolved = false;
    }

    public void UpdatePropertyContext(
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> recordContext,
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
        IPropertyContext propertyContext)
    {
        var context = GetContext(propertyContext);
        var recordValue = GetValue(recordContext.Record);
        var recordMod = state.LoadOrder[recordContext.ModKey].Mod
            ?? throw new InvalidOperationException(
                $"Could not load {recordContext.ModKey} while resolving {PropertyName}");

        UpdatePresence(context, recordValue.HasValue, recordContext, recordMod);

        var recordFlags = recordValue.GetValueOrDefault();
        foreach (var flag in AllFlags)
        {
            var original = context.OriginalFlagContexts.Single(item => item.Flag.Equals(flag));
            var forward = context.ForwardFlagContexts.Single(item => item.Flag.Equals(flag));
            var recordIsSet = IsSet(recordFlags, flag);

            if (recordIsSet != forward.IsSet
                && (recordIsSet != original.IsSet || CanModify(recordMod, forward.OwnerMod)))
            {
                forward.IsSet = recordIsSet;
                forward.OwnerMod = recordContext.ModKey.ToString();
            }
        }
    }

    public string FormatValue(object? value)
    {
        if (value == null)
        {
            return "<absent>";
        }

        if (value is not TFlag flags)
        {
            return value.ToString() ?? "null";
        }

        var names = AllFlags
            .Where(flag => IsSet(flags, flag))
            .Select(flag => flag.ToString())
            .ToArray();
        return names.Length == 0 ? "None (present)" : string.Join(", ", names);
    }

    private static NullableFlagPropertyContext<TFlag> GetContext(IPropertyContext context) =>
        context as NullableFlagPropertyContext<TFlag>
        ?? throw new InvalidOperationException(
            $"Property context is not a nullable {typeof(TFlag).Name} flag context");

    private static bool IsSet(TFlag flags, TFlag flag)
    {
        var flagsBits = Convert.ToInt64(flags);
        var flagBits = Convert.ToInt64(flag);
        return (flagsBits & flagBits) == flagBits;
    }

    private static bool CanModify(ISkyrimModGetter mod, string ownerMod) =>
        PatcherSettings.HasMasterOrVirtualMaster(mod, ownerMod);

    private static void UpdatePresence(
        NullableFlagPropertyContext<TFlag> context,
        bool hasValue,
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> recordContext,
        ISkyrimModGetter recordMod)
    {
        if (hasValue != context.ForwardHasValue
            && (hasValue != context.OriginalHasValue
                || CanModify(recordMod, context.PresenceOwnerMod)))
        {
            context.ForwardHasValue = hasValue;
            context.PresenceOwnerMod = recordContext.ModKey.ToString();
        }
    }

    void IPropertyHandler.SetValue(IMajorRecord record, object? value) =>
        SetValue(record, value is TFlag flags ? flags : null);

    object? IPropertyHandler.GetValue(IMajorRecordGetter record) => GetValue(record);

    bool IPropertyHandler.AreValuesEqual(object? value1, object? value2) =>
        AreValuesEqual(
            value1 is TFlag flags1 ? flags1 : null,
            value2 is TFlag flags2 ? flags2 : null);
}
