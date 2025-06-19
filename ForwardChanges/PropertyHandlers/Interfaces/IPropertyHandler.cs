using System;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.PropertyHandlers.Interfaces
{
    /// <summary>
    /// Represents a handler for a specific property type in a record.
    /// </summary>
    /// <typeparam name="T">The type of value this handler manages</typeparam>
    public interface IPropertyHandler<T>
    {
        /// <summary>
        /// Gets the name of the property this handler manages.
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// Gets whether this handler manages a list property.
        /// </summary>
        bool IsListHandler { get; }

        /// <summary>
        /// Sets the value of the property on the given record.
        /// </summary>
        /// <param name="record">The record to modify</param>
        /// <param name="value">The value to set</param>
        void SetValue(IMajorRecord record, T value);

        /// <summary>
        /// Gets the value of the property from the given context.
        /// </summary>
        /// <param name="record">The record to get the value from</param>
        /// <returns>The value of the property</returns>
        T? GetValue(IMajorRecordGetter record);

        /// <summary>
        /// Compares two values of the property type for equality.
        /// </summary>
        /// <param name="value1">The first value to compare</param>
        /// <param name="value2">The second value to compare</param>
        /// <returns>True if the values are equal, false otherwise</returns>
        bool AreValuesEqual(T? value1, T? value2);

        /// <summary>
        /// Initializes the property context for the given record.
        /// </summary>
        /// <param name="originalContext">The original context</param>
        /// <param name="winningContext">The winning context</param>
        /// <param name="propertyContext">The property context</param>
        void InitializeContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> originalContext,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPropertyContext<T> propertyContext);

        void UpdatePropertyContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            IPropertyContext<T> propertyContext);
    }
}