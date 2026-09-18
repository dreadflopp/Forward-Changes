using ForwardChanges.Contexts;
using ForwardChanges.Contexts.Interfaces;
using ForwardChanges.PropertyHandlers.Interfaces;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using TextureSetFlag = Mutagen.Bethesda.Skyrim.TextureSet.Flag;

namespace ForwardChanges.PropertyHandlers.TextureSet;

/// <summary>
/// Resolves TXST's optional DNAM presence and each named flag bit independently.
/// </summary>
public sealed class FlagsHandler : IPropertyHandler<TextureSetFlag?>
{
    private static readonly TextureSetFlag[] AllFlags = Enum.GetValues<TextureSetFlag>()
        .Where(flag => Convert.ToInt64(flag) != 0)
        .ToArray();

    private static readonly long KnownMask = AllFlags.Aggregate(
        0L,
        (mask, flag) => mask | Convert.ToInt64(flag));

    public string PropertyName => "Flags";
    public bool RequiresFullLoadOrderProcessing => true;

    public TextureSetFlag? GetValue(IMajorRecordGetter record)
        => record is ITextureSetGetter textureSet
            ? textureSet.Flags
            : throw new InvalidOperationException($"Expected ITextureSetGetter but got {record.GetType()}");

    public void SetValue(IMajorRecord record, TextureSetFlag? value)
    {
        if (record is not ITextureSet textureSet)
        {
            throw new InvalidOperationException($"Expected ITextureSet but got {record.GetType()}");
        }

        if (!value.HasValue)
        {
            textureSet.Flags = null;
            return;
        }

        var currentBits = textureSet.Flags.HasValue
            ? Convert.ToInt64(textureSet.Flags.Value)
            : 0L;
        var requestedBits = Convert.ToInt64(value.Value);
        var mergedBits = (currentBits & ~KnownMask) | (requestedBits & KnownMask);
        textureSet.Flags = (TextureSetFlag)Enum.ToObject(typeof(TextureSetFlag), mergedBits);
    }

    public bool AreValuesEqual(TextureSetFlag? value1, TextureSetFlag? value2)
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

    public IPropertyContext CreatePropertyContext()
        => new NullableFlagPropertyContext<TextureSetFlag>();

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
            .Select(flag => new FlagPropertyValueContext<TextureSetFlag>(
                flag,
                IsSet(originalFlags, flag),
                originalContext.ModKey.ToString()))
            .ToList();
        context.ForwardFlagContexts = context.OriginalFlagContexts
            .Select(flag => new FlagPropertyValueContext<TextureSetFlag>(
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
        var recordMod = state.LoadOrder[recordContext.ModKey].Mod;
        if (recordMod == null)
        {
            throw new InvalidOperationException($"Could not load {recordContext.ModKey} while resolving {PropertyName}");
        }

        UpdatePresence(context, recordValue.HasValue, recordContext, recordMod);

        var recordFlags = recordValue.GetValueOrDefault();
        foreach (var flag in AllFlags)
        {
            var original = context.OriginalFlagContexts.Single(item => item.Flag.Equals(flag));
            var forward = context.ForwardFlagContexts.Single(item => item.Flag.Equals(flag));
            var recordIsSet = IsSet(recordFlags, flag);

            if (recordIsSet == forward.IsSet)
            {
                continue;
            }

            if (recordIsSet != original.IsSet || CanModify(recordMod, forward.OwnerMod))
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

        if (value is not TextureSetFlag flags)
        {
            return value.ToString() ?? "null";
        }

        var names = AllFlags
            .Where(flag => IsSet(flags, flag))
            .Select(flag => flag.ToString())
            .ToArray();
        return names.Length == 0 ? "None (present)" : string.Join(", ", names);
    }

    private static NullableFlagPropertyContext<TextureSetFlag> GetContext(IPropertyContext context)
        => context as NullableFlagPropertyContext<TextureSetFlag>
            ?? throw new InvalidOperationException("Property context is not a nullable TXST flag context");

    private static bool IsSet(TextureSetFlag flags, TextureSetFlag flag)
        => (flags & flag) == flag;

    private static bool CanModify(ISkyrimModGetter mod, string ownerMod)
        => mod.MasterReferences.Any(master =>
            string.Equals(master.Master.ToString(), ownerMod, StringComparison.OrdinalIgnoreCase));

    private static void UpdatePresence(
        NullableFlagPropertyContext<TextureSetFlag> context,
        bool hasValue,
        IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> recordContext,
        ISkyrimModGetter recordMod)
    {
        if (hasValue == context.ForwardHasValue)
        {
            return;
        }

        if (hasValue != context.OriginalHasValue || CanModify(recordMod, context.PresenceOwnerMod))
        {
            context.ForwardHasValue = hasValue;
            context.PresenceOwnerMod = recordContext.ModKey.ToString();
        }
    }

    void IPropertyHandler.SetValue(IMajorRecord record, object? value)
        => SetValue(record, value is TextureSetFlag flags ? flags : null);

    object? IPropertyHandler.GetValue(IMajorRecordGetter record)
        => GetValue(record);

    bool IPropertyHandler.AreValuesEqual(object? value1, object? value2)
        => AreValuesEqual(
            value1 is TextureSetFlag flags1 ? flags1 : null,
            value2 is TextureSetFlag flags2 ? flags2 : null);
}
