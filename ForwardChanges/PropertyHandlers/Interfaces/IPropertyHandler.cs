using System;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.PropertyHandlers.Interfaces
{
    public interface IPropertyHandler
    {
        string PropertyName { get; }
        bool IsListHandler { get; }

        // Non-generic versions for the registry
        void SetValue(IMajorRecord record, object? value);
        object? GetValue(IMajorRecordGetter record);
        bool AreValuesEqual(object? value1, object? value2);

        // Context management methods
        IPropertyContext CreatePropertyContext();

        void UpdatePropertyContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            IPropertyContext propertyContext);

        void InitializeContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> originalContext,
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> winningContext,
            IPropertyContext propertyContext);
    }

    /// <summary>
    /// Represents a handler for a specific property type in a record.
    /// </summary>
    /// <typeparam name="T">The type of value this handler manages</typeparam>
    public interface IPropertyHandler<T> : IPropertyHandler
    {
        // Type-safe versions for handlers to implement
        void SetValue(IMajorRecord record, T? value);
        T? GetValue(IMajorRecordGetter record);
        bool AreValuesEqual(T? value1, T? value2);
    }
}