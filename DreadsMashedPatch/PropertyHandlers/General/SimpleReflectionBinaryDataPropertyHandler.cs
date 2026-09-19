using System;
using System.Linq;
using System.Reflection;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;
using Noggog;

namespace ForwardChanges.PropertyHandlers.General
{
    /// <summary>
    /// A generic property handler that uses reflection to access ReadOnlyMemorySlice&lt;byte&gt;? properties.
    /// This handler is designed for binary data properties that need to be copied as byte arrays.
    /// </summary>
    /// <typeparam name="TRecord">The record type that contains the property (e.g., IPlacedObject)</typeparam>
    /// <typeparam name="TRecordGetter">The getter record type (e.g., IPlacedObjectGetter)</typeparam>
    public class SimpleReflectionBinaryDataPropertyHandler<TRecord, TRecordGetter> : AbstractPropertyHandler<ReadOnlyMemorySlice<byte>?>
        where TRecord : class, IMajorRecord
        where TRecordGetter : class, IMajorRecordGetter
    {
        private readonly string _propertyName;
        private readonly string[] _propertyPath;
        private readonly PropertyInfo? _getterProperty;
        private readonly PropertyInfo? _setterProperty;
        private readonly PropertyInfo[]? _getterPathProperties;
        private readonly PropertyInfo[]? _setterPathProperties;
        private readonly Type[]? _setterPathTypes;

        public SimpleReflectionBinaryDataPropertyHandler(string propertyName)
        {
            _propertyName = propertyName;
            _propertyPath = propertyName.Split('.');

            // Find the property on the getter interface
            _getterProperty = FindProperty(typeof(TRecordGetter), _propertyPath);

            // Find the property on the setter interface
            _setterProperty = FindProperty(typeof(TRecord), _propertyPath);

            if (_getterProperty == null)
            {
                throw new ArgumentException(
                    $"Property '{propertyName}' not found on {typeof(TRecordGetter).Name}");
            }

            if (_propertyPath.Length > 1)
            {
                _getterPathProperties = BuildPropertyPath(typeof(TRecordGetter), _propertyPath, out _);
                _setterPathProperties = BuildPropertyPath(typeof(TRecord), _propertyPath, out _setterPathTypes);
            }
        }

        public override string PropertyName => _propertyName;

        private static PropertyInfo? FindProperty(Type type, string[] path)
        {
            var currentType = type;
            PropertyInfo? property = null;

            for (var i = 0; i < path.Length; i++)
            {
                property = ReflectionPropertyResolver.Find(
                    currentType,
                    path[i],
                    i < path.Length - 1 ? path[i + 1] : null);
                if (property == null)
                {
                    return null;
                }

                if (i < path.Length - 1)
                {
                    currentType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                }
            }

            return property;
        }

        private static PropertyInfo[] BuildPropertyPath(Type startType, string[] path, out Type[] pathTypes)
        {
            var properties = new PropertyInfo[path.Length - 1];
            pathTypes = new Type[path.Length - 1];
            var currentType = startType;

            for (var i = 0; i < path.Length - 1; i++)
            {
                var property = ReflectionPropertyResolver.Find(currentType, path[i], path[i + 1])
                    ?? throw new ArgumentException($"Property '{path[i]}' not found in path '{string.Join('.', path)}'");
                properties[i] = property;
                currentType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                pathTypes[i] = currentType;
            }

            return properties;
        }

        public override ReadOnlyMemorySlice<byte>? GetValue(IMajorRecordGetter record)
        {
            if (record is not TRecordGetter typedRecord)
            {
                Console.WriteLine($"Error: Record does not implement {typeof(TRecordGetter).Name} for {PropertyName}");
                return null;
            }

            if (_getterProperty == null)
            {
                return null;
            }

            try
            {
                object? currentObject = typedRecord;
                if (_getterPathProperties != null)
                {
                    foreach (var pathProperty in _getterPathProperties)
                    {
                        if (currentObject == null)
                        {
                            return null;
                        }

                        currentObject = pathProperty.GetValue(currentObject);
                    }
                }

                if (currentObject == null)
                {
                    return null;
                }

                var value = _getterProperty.GetValue(currentObject);
                if (value is ReadOnlyMemorySlice<byte> slice)
                {
                    return slice;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting property '{PropertyName}' via reflection: {ex.Message}");
                return null;
            }
        }

        public override void SetValue(IMajorRecord record, ReadOnlyMemorySlice<byte>? value)
        {
            if (record is not TRecord typedRecord)
            {
                Console.WriteLine($"Error: Record does not implement {typeof(TRecord).Name} for {PropertyName}");
                return;
            }

            if (_setterProperty == null)
            {
                Console.WriteLine($"Error: Property '{PropertyName}' is read-only or not found on {typeof(TRecord).Name}");
                return;
            }

            try
            {
                object? currentObject = typedRecord;
                if (_setterPathProperties != null && _setterPathTypes != null)
                {
                    for (var i = 0; i < _setterPathProperties.Length; i++)
                    {
                        var pathProperty = _setterPathProperties[i];
                        var intermediateValue = pathProperty.GetValue(currentObject);
                        if (intermediateValue == null && value != null)
                        {
                            var newInstance = System.Activator.CreateInstance(_setterPathTypes[i]);
                            if (newInstance == null)
                            {
                                Console.WriteLine($"Error: Could not create instance of {_setterPathTypes[i].Name} for property path '{PropertyName}'");
                                return;
                            }

                            pathProperty.SetValue(currentObject, newInstance);
                            intermediateValue = newInstance;
                        }

                        currentObject = intermediateValue;
                        if (currentObject == null)
                        {
                            return;
                        }
                    }
                }

                if (currentObject == null)
                {
                    return;
                }

                var propertyType = _setterProperty.PropertyType;

                // Handle nullable MemorySlice<byte>?
                Type? underlyingType = null;
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    underlyingType = propertyType.GetGenericArguments()[0];
                }
                else
                {
                    underlyingType = propertyType;
                }

                object? valueToSet = null;

                if (value == null)
                {
                    valueToSet = null;
                }
                else if (underlyingType != null && underlyingType.IsGenericType)
                {
                    var genericTypeDef = underlyingType.GetGenericTypeDefinition();
                    var genericTypeDefName = genericTypeDef.Name;
                    var genericArgs = underlyingType.GetGenericArguments();

                    // Check if it's MemorySlice<byte> or ReadOnlyMemorySlice<byte>
                    if (genericArgs.Length == 1 && genericArgs[0] == typeof(byte))
                    {
                        if (genericTypeDefName == "MemorySlice`1" || genericTypeDefName.Contains("MemorySlice"))
                        {
                            // Create MemorySlice<byte> from byte array
                            var arrayValue = value.Value.ToArray();

                            // Use the actual underlying type (which is MemorySlice<byte>)
                            // Create instance using the constructor that takes byte[]
                            try
                            {
                                // MemorySlice<byte> has a constructor that takes byte[]
                                var constructor = underlyingType.GetConstructor(new[] { typeof(byte[]) });
                                if (constructor != null)
                                {
                                    valueToSet = constructor.Invoke(new object[] { arrayValue });
                                }
                                else
                                {
                                    // Try using Activator
                                    valueToSet = System.Activator.CreateInstance(underlyingType, arrayValue);
                                }
                            }
                            catch (Exception createEx)
                            {
                                Console.WriteLine($"Warning: Could not create {underlyingType.Name} from byte array: {createEx.Message}");
                                // Fallback: try direct assignment (might have implicit conversion)
                                valueToSet = arrayValue;
                            }
                        }
                        else if (genericTypeDefName == "ReadOnlyMemorySlice`1")
                        {
                            // Property accepts ReadOnlyMemorySlice, use directly
                            valueToSet = value.Value;
                        }
                        else
                        {
                            // Fallback: try byte[] (some properties accept byte[])
                            valueToSet = value.Value.ToArray();
                        }
                    }
                    else
                    {
                        // Fallback: try byte[]
                        valueToSet = value.Value.ToArray();
                    }
                }
                else if (propertyType == typeof(byte[]) || (underlyingType != null && underlyingType == typeof(byte[])))
                {
                    // Property accepts byte[] directly
                    valueToSet = value.Value.ToArray();
                }
                else
                {
                    // Fallback: try byte[]
                    valueToSet = value.Value.ToArray();
                }

                _setterProperty.SetValue(currentObject, valueToSet);
            }
            catch (Exception ex)
            {
                var propertyType = _setterProperty?.PropertyType;
                Console.WriteLine($"Error setting property '{PropertyName}' via reflection. Property type: {propertyType?.FullName ?? "unknown"}, Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        public override bool AreValuesEqual(ReadOnlyMemorySlice<byte>? value1, ReadOnlyMemorySlice<byte>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare byte arrays using SequenceEqual
            return value1.Value.Span.SequenceEqual(value2.Value.Span);
        }

        public override string FormatValue(object? value)
        {
            if (value is ReadOnlyMemorySlice<byte> slice)
            {
                return $"{PropertyName}({slice.Length} bytes)";
            }
            return value?.ToString() ?? "null";
        }
    }
}
