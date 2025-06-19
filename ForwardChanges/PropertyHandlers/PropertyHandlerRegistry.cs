using System;
using System.Collections.Generic;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers
{
    /// <summary>
    /// Maintains a type-safe registry of property handlers for different record types and properties.
    /// </summary>
    public class PropertyHandlerRegistry
    {
        private readonly Dictionary<string, (IPropertyHandler Handler, Type ValueType)> _handlers;

        public PropertyHandlerRegistry()
        {
            _handlers = new Dictionary<string, (IPropertyHandler, Type)>();
        }

        /// <summary>
        /// Registers a property handler with its associated type.
        /// </summary>
        /// <typeparam name="T">The type of value this handler manages</typeparam>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="handler">The property handler instance</param>
        public void Register<T>(string propertyName, IPropertyHandler<T> handler)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException(nameof(propertyName));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _handlers[propertyName] = (handler, typeof(T));
        }

        /// <summary>
        /// Gets a property handler of the specified type.
        /// </summary>
        /// <typeparam name="T">The expected type of the handler</typeparam>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>The property handler if found and of the correct type, null otherwise</returns>
        public IPropertyHandler<T>? GetHandler<T>(string propertyName)
        {
            if (_handlers.TryGetValue(propertyName, out var handler) &&
                handler.ValueType == typeof(T))
            {
                return (IPropertyHandler<T>)handler.Handler;
            }
            return null;
        }

        /// <summary>
        /// Gets a value from a property handler for a given context.
        /// </summary>
        /// <typeparam name="T">The expected type of the value</typeparam>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="context">The mod context</param>
        /// <returns>The value if found and of the correct type, default otherwise</returns>
        public T? GetValue<T>(string propertyName, IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context)
        {
            var handler = GetHandler<T>(propertyName);
            return handler?.GetValue(context);
        }

        /// <summary>
        /// Sets a value using the appropriate property handler.
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="record">The record to modify</param>
        /// <param name="value">The value to set</param>
        /// <returns>True if the value was set successfully, false otherwise</returns>
        public bool SetValue<T>(string propertyName, IMajorRecord record, T value)
        {
            var handler = GetHandler<T>(propertyName);
            if (handler != null)
            {
                handler.SetValue(record, value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a property handler exists for the given property name and type.
        /// </summary>
        /// <typeparam name="T">The expected type of the handler</typeparam>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>True if a handler exists for the property and type, false otherwise</returns>
        public bool HasHandler<T>(string propertyName)
        {
            return _handlers.TryGetValue(propertyName, out var handler) &&
                   handler.ValueType == typeof(T);
        }

        /// <summary>
        /// Gets all registered property names.
        /// </summary>
        /// <returns>An enumerable of all registered property names</returns>
        public IEnumerable<string> GetRegisteredProperties()
        {
            return _handlers.Keys;
        }
    }
}