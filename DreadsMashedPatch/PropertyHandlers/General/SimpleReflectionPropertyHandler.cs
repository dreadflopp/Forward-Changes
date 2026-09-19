using System;
using System.Globalization;
using System.Reflection;
using System.Linq;
using Mutagen.Bethesda.Plugins.Records;
using DreadsMashedPatch;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using Noggog;

namespace DreadsMashedPatch.PropertyHandlers.General
{
    /// <summary>
    /// A generic property handler that uses reflection to access simple properties.
    /// This handler is designed for simple properties that don't require special handling
    /// like deep copying, custom equality, or complex initialization logic.
    /// 
    /// Supports nested property paths (e.g., "Placement.Position") and will automatically
    /// create intermediate objects if they are null when setting values.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value</typeparam>
    /// <typeparam name="TRecord">The record type that contains the property (e.g., IPlacedObject)</typeparam>
    /// <typeparam name="TRecordGetter">The getter record type (e.g., IPlacedObjectGetter)</typeparam>
    public class SimpleReflectionPropertyHandler<TValue, TRecord, TRecordGetter> : AbstractPropertyHandler<TValue>
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
        private readonly float? _p3FloatEpsilon;

        public SimpleReflectionPropertyHandler(string propertyName, float? p3FloatEpsilon = null)
        {
            _propertyName = propertyName;
            _p3FloatEpsilon = p3FloatEpsilon;
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

            // Build path for nested properties
            if (_propertyPath.Length > 1)
            {
                _getterPathProperties = BuildPropertyPath(typeof(TRecordGetter), _propertyPath, out _);
                _setterPathProperties = BuildPropertyPath(typeof(TRecord), _propertyPath, out _setterPathTypes);
            }
        }

        public override string PropertyName => _propertyName;

        private PropertyInfo? FindProperty(Type type, string[] path)
        {
            Type currentType = type;
            PropertyInfo? property = null;

            for (int i = 0; i < path.Length; i++)
            {
                property = FindPathProperty(currentType, path[i], i < path.Length - 1 ? path[i + 1] : null);

                if (property == null)
                {
                    return null;
                }

                // If not the last property in the path, get the type for the next iteration
                if (i < path.Length - 1)
                {
                    currentType = property.PropertyType;
                    // Handle nullable types
                    if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        currentType = currentType.GetGenericArguments()[0];
                    }
                }
            }

            return property;
        }

        private PropertyInfo[] BuildPropertyPath(Type startType, string[] path, out Type[] pathTypes)
        {
            var pathProperties = new PropertyInfo[path.Length - 1];
            pathTypes = new Type[path.Length - 1];
            Type currentType = startType;

            for (int i = 0; i < path.Length - 1; i++)
            {
                var property = FindPathProperty(currentType, path[i], path[i + 1]);

                if (property == null)
                {
                    throw new ArgumentException($"Property '{path[i]}' not found in path '{_propertyName}'");
                }

                pathProperties[i] = property;

                var propType = property.PropertyType;
                // Handle nullable types - get the underlying type
                if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    propType = propType.GetGenericArguments()[0];
                }

                pathTypes[i] = propType;
                currentType = propType;
            }

            return pathProperties;
        }

        private static PropertyInfo? FindPathProperty(Type type, string name, string? nextSegment)
        {
            return ReflectionPropertyResolver.Find(type, name, nextSegment);
        }

        private static bool HasProperty(Type type, string name)
        {
            return ReflectionPropertyResolver.HasProperty(type, name);
        }

        public override TValue? GetValue(IMajorRecordGetter record)
        {
            if (record is not TRecordGetter typedRecord)
            {
                Console.WriteLine($"Error: Record does not implement {typeof(TRecordGetter).Name} for {PropertyName}");
                return default;
            }

            if (_getterProperty == null)
            {
                return default;
            }

            try
            {
                object? currentObject = typedRecord;

                // Navigate through nested properties
                if (_getterPathProperties != null)
                {
                    for (int i = 0; i < _getterPathProperties.Length; i++)
                    {
                        if (currentObject == null)
                        {
                            return default;
                        }

                        currentObject = _getterPathProperties[i].GetValue(currentObject);

                        // If we got a null value and there are more properties to navigate, return default
                        if (currentObject == null && i < _getterPathProperties.Length - 1)
                        {
                            return default;
                        }
                    }
                }

                if (currentObject == null)
                {
                    return default;
                }

                var value = _getterProperty.GetValue(currentObject);
                return value is TValue typedValue ? typedValue : default;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting property '{PropertyName}' via reflection: {ex.Message}");
                return default;
            }
        }

        public override void SetValue(IMajorRecord record, TValue? value)
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

                // Navigate through nested properties, creating intermediate objects if needed
                if (_setterPathProperties != null && _setterPathTypes != null)
                {
                    for (int i = 0; i < _setterPathProperties.Length; i++)
                    {
                        var pathProperty = _setterPathProperties[i];
                        var pathType = _setterPathTypes[i];

                        // Get the current value of the intermediate property
                        var intermediateValue = pathProperty.GetValue(currentObject);

                        // If null and we need to set a value, create a new instance
                        if (intermediateValue == null && value != null)
                        {
                            // Try to create an instance using the default constructor
                            var newInstance = System.Activator.CreateInstance(pathType);
                            if (newInstance == null)
                            {
                                Console.WriteLine($"Error: Could not create instance of {pathType.Name} for property path '{PropertyName}'");
                                return;
                            }

                            // Set the intermediate property
                            pathProperty.SetValue(currentObject, newInstance);
                            intermediateValue = newInstance;
                        }

                        currentObject = intermediateValue;

                        if (currentObject == null)
                        {
                            // Can't proceed if intermediate object is null and we can't create it
                            return;
                        }
                    }
                }

                if (currentObject == null)
                {
                    Console.WriteLine($"Error: Could not navigate to property '{PropertyName}'");
                    return;
                }

                // Handle nullable value types - extract value if it's a nullable type
                object? valueToSet = value;
                if (value != null && value.GetType().IsGenericType &&
                    value.GetType().GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    // Check if it has a value using reflection
                    var hasValueProperty = value.GetType().GetProperty("HasValue");
                    var valueProperty = value.GetType().GetProperty("Value");

                    if (hasValueProperty != null && valueProperty != null)
                    {
                        var hasValue = (bool)(hasValueProperty.GetValue(value) ?? false);
                        if (hasValue)
                        {
                            valueToSet = valueProperty.GetValue(value);
                        }
                        else
                        {
                            valueToSet = null;
                        }
                    }
                }

                _setterProperty.SetValue(currentObject, valueToSet);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting property '{PropertyName}' via reflection: {ex.Message}");
            }
        }

        public override bool AreValuesEqual(TValue? value1, TValue? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            if (value1 is string s1 && value2 is string s2)
            {
                return StringComparisonHelper.EqualsNormalized(s1, s2);
            }

            // For nullable float types, use epsilon comparison
            if (typeof(TValue).IsGenericType && typeof(TValue).GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = typeof(TValue).GetGenericArguments()[0];

                // Handle float? with epsilon comparison
                if (underlyingType == typeof(float))
                {
                    var v1 = value1 as float?;
                    var v2 = value2 as float?;
                    if (v1.HasValue && v2.HasValue)
                    {
                        return Math.Abs(v1.Value - v2.Value) < 0.0001f;
                    }
                }

                // Handle P3Float? - extract Value and use explicit epsilon comparison
                // Use component-wise epsilon (0.0001f) to match float? handling - ensures reversions
                // are correctly detected when values come from different mods/serialization (Position, Rotation, etc.)
                if (underlyingType == typeof(P3Float))
                {
                    // For P3Float?, we need to handle nullable comparison
                    // Use reflection to get HasValue and Value properties
                    var hasValueProp = typeof(TValue).GetProperty("HasValue");
                    var valueProp = typeof(TValue).GetProperty("Value");
                    if (hasValueProp != null && valueProp != null)
                    {
                        var hasValue1 = (bool)(hasValueProp.GetValue(value1) ?? false);
                        var hasValue2 = (bool)(hasValueProp.GetValue(value2) ?? false);

                        // Both null
                        if (!hasValue1 && !hasValue2) return true;
                        // One null, one not
                        if (!hasValue1 || !hasValue2) return false;

                        // Both have values - extract and compare using explicit epsilon
                        var v1 = valueProp.GetValue(value1);
                        var v2 = valueProp.GetValue(value2);

                        // Try to cast to P3Float - this should work since underlyingType is P3Float
                        if (v1 != null && v2 != null)
                        {
                            if (v1 is P3Float p1 && v2 is P3Float p2)
                            {
                                return P3FloatComparison.EqualsWithin(p1, p2, _p3FloatEpsilon ?? P3FloatComparison.DefaultEpsilon);
                            }

                            // Fallback: try to convert and compare directly
                            try
                            {
                                var p1Fallback = (P3Float)v1;
                                var p2Fallback = (P3Float)v2;
                                return P3FloatComparison.EqualsWithin(p1Fallback, p2Fallback, _p3FloatEpsilon ?? P3FloatComparison.DefaultEpsilon);
                            }
                            catch
                            {
                                // If conversion fails, we'll fall through to other comparison methods below
                            }
                        }

                        // If we got here, we couldn't extract/compare the values properly
                        // Fall through to other comparison methods below
                    }
                }
            }

            // For P3Float (non-nullable), use explicit epsilon comparison
            if (typeof(TValue) == typeof(P3Float))
            {
                if (value1 is P3Float p1 && value2 is P3Float p2)
                {
                    return P3FloatComparison.EqualsWithin(p1, p2, _p3FloatEpsilon ?? P3FloatComparison.DefaultEpsilon);
                }
            }

            // For float types (non-nullable), use epsilon comparison
            if (typeof(TValue) == typeof(float))
            {
                if (value1 is float f1 && value2 is float f2)
                {
                    return Math.Abs(f1 - f2) < 0.0001f;
                }
            }

            // For Mutagen types (like P3Float, Placement, etc.), they implement IEquatable with their getter interface
            // Try to find and use the Equals method from IEquatable<IGetterType>
            var valueType = value1.GetType();

            // Check all interfaces for IEquatable<T> where T might be a getter interface
            var equatableInterfaces = valueType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEquatable<>))
                .ToList();

            foreach (var equatableInterface in equatableInterfaces)
            {
                var equalsMethod = equatableInterface.GetMethod("Equals");
                if (equalsMethod != null)
                {
                    try
                    {
                        // Try to invoke Equals with value2 - it might be compatible even if types don't match exactly
                        var result = equalsMethod.Invoke(value1, new object[] { value2! });
                        if (result is bool boolResult)
                        {
                            return boolResult;
                        }
                    }
                    catch
                    {
                        // Continue to next interface or fallback
                    }
                }
            }

            // Try to use IEquatable<TValue> if available (for simple types)
            if (value1 is IEquatable<TValue> equatable1)
            {
                return equatable1.Equals(value2!);
            }

            // Use the type's Equals method (for Mutagen types that override Equals like P3Float)
            // This will use the overridden Equals which calls CommonInstance().Equals() for Mutagen types
            try
            {
                return value1.Equals(value2);
            }
            catch
            {
                // Fallback: use object.Equals
                return Equals(value1, value2);
            }
        }

        /// <summary>
        /// Format a value for display in logs. Override to provide custom formatting for P3Float values.
        /// Uses invariant culture to ensure consistent formatting with period as decimal separator.
        /// </summary>
        public override string FormatValue(object? value)
        {
            if (value == null) return "null";

            // Helper method to format a P3Float value
            string FormatP3Float(P3Float p3Float)
            {
                // Use invariant culture to ensure period as decimal separator
                // Also handle -0.0 by converting to 0.0
                float x = p3Float.X == 0f ? 0f : p3Float.X;
                float y = p3Float.Y == 0f ? 0f : p3Float.Y;
                float z = p3Float.Z == 0f ? 0f : p3Float.Z;

                return $"{x.ToString("F4", CultureInfo.InvariantCulture)}, {y.ToString("F4", CultureInfo.InvariantCulture)}, {z.ToString("F4", CultureInfo.InvariantCulture)}";
            }

            // Handle P3Float? (nullable)
            if (typeof(TValue).IsGenericType && typeof(TValue).GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = typeof(TValue).GetGenericArguments()[0];
                if (underlyingType == typeof(P3Float))
                {
                    // Use reflection to get HasValue and Value properties
                    var hasValueProp = typeof(TValue).GetProperty("HasValue");
                    var valueProp = typeof(TValue).GetProperty("Value");
                    if (hasValueProp != null && valueProp != null)
                    {
                        var hasValue = (bool)(hasValueProp.GetValue(value) ?? false);
                        if (!hasValue) return "null";

                        var p3FloatValue = valueProp.GetValue(value);
                        if (p3FloatValue is P3Float p3Float)
                        {
                            return FormatP3Float(p3Float);
                        }
                    }
                }
            }

            // Handle P3Float (non-nullable)
            if (typeof(TValue) == typeof(P3Float))
            {
                if (value is P3Float p3Float)
                {
                    return FormatP3Float(p3Float);
                }
            }

            // Default to base implementation for other types
            return base.FormatValue(value);
        }
    }
}
