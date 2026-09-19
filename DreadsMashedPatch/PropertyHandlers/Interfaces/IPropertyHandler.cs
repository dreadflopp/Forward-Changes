using System;
using Mutagen.Bethesda.Plugins.Records;
using DreadsMashedPatch.PropertyHandlers.Formatting;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using DreadsMashedPatch.Contexts.Interfaces;

namespace DreadsMashedPatch.PropertyHandlers.Interfaces
{
    public interface IPropertyHandler
    {
        string PropertyName { get; }
        bool RequiresFullLoadOrderProcessing { get; }

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

        // Default implementation for formatting values
        string FormatValue(object? value) => DiagnosticValueFormatter.Format(value);
    }

    /// <summary>
    /// Represents a handler for a specific property type in a record.
    /// </summary>
    /// <typeparam name="T">The type of value this handler manages</typeparam>
    public interface IPropertyHandler<T> : IPropertyHandler
    {
        // Type-safe versions for handlers to implement
        void SetValue(IMajorRecord record, T? value);
        new T? GetValue(IMajorRecordGetter record);
        bool AreValuesEqual(T? value1, T? value2);
    }

    /// <summary>
    /// Optional compact, difference-oriented diagnostics for large atomic values.
    /// Used only for deep-dive logs; it never participates in forwarding decisions.
    /// </summary>
    public interface IDiagnosticDiffPropertyHandler
    {
        string FormatIdentity(object? value);
        string FormatDifference(object? olderValue, object? newerValue);
    }
}
