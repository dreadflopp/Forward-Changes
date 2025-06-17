using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.Contexts;
namespace ForwardChanges.PropertyHandlers.Interfaces
{
    public interface IPropertyHandler
    {
        string PropertyName { get; }

        /// <summary>
        /// Indicates whether this handler is a list handler
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
        /// Updates the property state based on the current record context
        /// </summary>
        void UpdatePropertyContext(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            PropertyContext propertyContext);

        /// <summary>
        /// Compares two values for equality
        /// </summary>
        bool AreValuesEqual(object? value1, object? value2);
    }
}