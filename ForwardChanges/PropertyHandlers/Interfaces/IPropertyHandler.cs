using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.Contexts;

namespace ForwardChanges.PropertyHandlers.Interfaces
{
    /// <summary>
    /// Base interface for all property handlers that defines the common contract
    /// </summary>
    public interface IPropertyHandlerBase
    {
        /// <summary>
        /// The name of the property this handler manages
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// Whether this handler manages a list property
        /// </summary>
        bool IsListHandler { get; }

        /// <summary>
        /// Sets the value of the property on a record
        /// </summary>
        void SetValue(IMajorRecord record, object? value);

        /// <summary>
        /// Gets the current value of the property from a record context
        /// </summary>
        object? GetValue(IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context);

        /// <summary>
        /// Compares two values for equality
        /// </summary>
        bool AreValuesEqual(object? value1, object? value2);

        /// <summary>
        /// Updates the property context with the current state of the property
        /// </summary>
        void UpdatePropertyContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            PropertyContext propertyContext);
    }

    /// <summary>
    /// Generic interface for property handlers that provides type-safe operations
    /// </summary>
    /// <typeparam name="TItem">The type of value this handler manages</typeparam>
    public interface IPropertyHandler<TItem> : IPropertyHandlerBase
    {
        /// <summary>
        /// Sets the value of the property on a record
        /// </summary>
        void SetValue(IMajorRecord record, TItem? value);

        /// <summary>
        /// Gets the current value of the property from a record context
        /// </summary>
        new TItem? GetValue(IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context);

        /// <summary>
        /// Compares two values for equality
        /// </summary>
        bool AreValuesEqual(TItem? value1, TItem? value2);
    }
}