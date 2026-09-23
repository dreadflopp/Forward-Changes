using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Strings;
using DreadsMashedPatch;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using Noggog;

namespace DreadsMashedPatch.PropertyHandlers.General
{
    /// <summary>
    /// A generic property handler that uses reflection to access complex object properties.
    /// This handler performs deep copying for objects that contain collections, FormLinks, or nested objects.
    /// 
    /// Supports nested property paths (e.g., "SomeProperty.SubProperty") and will automatically
    /// create intermediate objects if they are null when setting values.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value (must be a class/interface, not a value type)</typeparam>
    /// <typeparam name="TRecord">The record type that contains the property (e.g., IPlacedObject)</typeparam>
    /// <typeparam name="TRecordGetter">The getter record type (e.g., IPlacedObjectGetter)</typeparam>
    public class ComplexReflectionPropertyHandler<TValue, TRecord, TRecordGetter> : AbstractPropertyHandler<TValue>
        where TValue : class
        where TRecord : class, IMajorRecord
        where TRecordGetter : class, IMajorRecordGetter
    {
        private readonly string _propertyName;
        private readonly string[] _propertyPath;
        private readonly PropertyInfo? _getterProperty;
        private readonly PropertyInfo? _setterProperty;
        private readonly PropertyInfo[]? _pathProperties;
        private readonly Type[]? _pathTypes;

        public ComplexReflectionPropertyHandler(string propertyName)
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

            // Build path for nested properties
            if (_propertyPath.Length > 1)
            {
                _pathProperties = new PropertyInfo[_propertyPath.Length - 1];
                _pathTypes = new Type[_propertyPath.Length - 1];
                BuildPropertyPath(typeof(TRecordGetter), _propertyPath, _pathProperties, _pathTypes);
            }
        }

        public override string PropertyName => _propertyName;

        private PropertyInfo? FindProperty(Type type, string[] path)
        {
            Type currentType = type;
            PropertyInfo? property = null;

            for (int i = 0; i < path.Length; i++)
            {
                property = ReflectionPropertyResolver.Find(
                    currentType,
                    path[i],
                    i < path.Length - 1 ? path[i + 1] : null);

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

        private void BuildPropertyPath(Type startType, string[] path, PropertyInfo[] pathProperties, Type[] pathTypes)
        {
            Type currentType = startType;

            for (int i = 0; i < path.Length - 1; i++)
            {
                var property = ReflectionPropertyResolver.Find(currentType, path[i], path[i + 1]);

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
        }

        public override TValue? GetValue(IMajorRecordGetter record)
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

                // Navigate through nested properties
                if (_pathProperties != null && _pathTypes != null)
                {
                    for (int i = 0; i < _pathProperties.Length; i++)
                    {
                        if (currentObject == null)
                        {
                            return null;
                        }

                        currentObject = _pathProperties[i].GetValue(currentObject);

                        // If we got a null value and there are more properties to navigate, return null
                        if (currentObject == null && i < _pathProperties.Length - 1)
                        {
                            return null;
                        }
                    }
                }

                if (currentObject == null)
                {
                    return null;
                }

                var value = _getterProperty.GetValue(currentObject);
                return value as TValue;
            }
            catch (Exception ex)
            {
                LogCollector.AddError(PropertyName, "Could not read the complex property via reflection", ex);
                return null;
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
                if (_pathProperties != null && _pathTypes != null)
                {
                    for (int i = 0; i < _pathProperties.Length; i++)
                    {
                        var pathProperty = _pathProperties[i];
                        var pathType = _pathTypes[i];

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

                // Deep copy the value if it's not null
                object? valueToSet = value != null ? DeepCopy(value) : null;

                // Check if we got a binary overlay that couldn't be copied
                // If the setter expects a mutable type but we have an overlay, try to create mutable instance
                if (valueToSet != null && valueToSet.GetType().Name.Contains("Overlay"))
                {
                    var setterPropertyType = _setterProperty.PropertyType;
                    // Handle nullable types
                    Type targetType = setterPropertyType;
                    if (setterPropertyType.IsGenericType && setterPropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        targetType = setterPropertyType.GetGenericArguments()[0];
                    }

                    // If setter expects a mutable type (not an overlay), try to create it
                    if (!targetType.Name.Contains("Overlay") && targetType != valueToSet.GetType())
                    {
                        try
                        {
                            // Try to create mutable instance and copy properties
                            var mutableInstance = System.Activator.CreateInstance(targetType);
                            if (mutableInstance != null)
                            {
                                // Copy properties from overlay to mutable instance
                                var overlayProperties = valueToSet.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                                var mutableProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                                foreach (var overlayProp in overlayProperties)
                                {
                                    if (!overlayProp.CanRead) continue;

                                    var mutableProp = mutableProperties.FirstOrDefault(p => p.Name == overlayProp.Name && p.CanWrite);
                                    if (mutableProp != null)
                                    {
                                        try
                                        {
                                            var propValue = overlayProp.GetValue(valueToSet);

                                            // Mutagen binary overlays expose opaque byte payloads as
                                            // ReadOnlyMemorySlice<byte>, while their mutable counterparts
                                            // require MemorySlice<byte>. Preserve the complete payload.
                                            if (propValue is ReadOnlyMemorySlice<byte> byteSlice
                                                && mutableProp.PropertyType == typeof(MemorySlice<byte>))
                                            {
                                                propValue = new MemorySlice<byte>(byteSlice.ToArray());
                                            }

                                            mutableProp.SetValue(mutableInstance, propValue);
                                        }
                                        catch (Exception propertyCopyEx)
                                        {
                                            LogCollector.AddWarning(
                                                PropertyName,
                                                $"Skipped property '{overlayProp.Name}' while converting a binary overlay",
                                                propertyCopyEx);
                                        }
                                    }
                                }

                                valueToSet = mutableInstance;
                                Console.WriteLine($"[{PropertyName}] Converted binary overlay {valueToSet.GetType().Name} to mutable type {targetType.Name}");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogCollector.AddWarning(
                                PropertyName,
                                "Could not convert the binary overlay to a mutable type; trying direct assignment",
                                ex);
                            // Fall through to try setting the overlay (will likely fail, but at least we tried)
                        }
                    }
                }

                _setterProperty.SetValue(currentObject, valueToSet);
            }
            catch (Exception ex)
            {
                LogCollector.AddError(PropertyName, "Could not apply the complex property via reflection", ex);
            }
        }

        /// <summary>
        /// Performs a deep copy of a complex object, handling collections, FormLinks, and nested objects.
        /// </summary>
        private object DeepCopy(object source)
        {
            if (source == null) return null!;

            var sourceType = source.GetType();

            // Handle TranslatedString before collection checks, because it can surface as enumerable.
            if (IsTranslatedStringType(sourceType))
            {
                return DeepCopyTranslatedString(source);
            }

            // Handle FormLink types - create new FormLink with FormKey
            if (IsFormLinkType(sourceType))
            {
                return DeepCopyFormLink(source);
            }

            // Handle collections
            if (source is IEnumerable enumerable && !(source is string))
            {
                return DeepCopyCollection(enumerable, sourceType);
            }

            // Handle value types and primitives - return as-is
            if (sourceType.IsValueType || sourceType.IsPrimitive || sourceType == typeof(string))
            {
                return source;
            }

            // Handle binary overlays - these can't be instantiated, return original
            // Binary overlays typically have "Overlay" or "BinaryOverlay" in their name
            // Note: This will be handled in SetValue by converting to a mutable type
            if (sourceType.Name.Contains("Overlay") || sourceType.Name.Contains("BinaryOverlay"))
            {
                // Return original - SetValue will convert it to a mutable type if needed
                return source;
            }

            // Handle complex objects - create new instance and copy properties
            try
            {
                var newInstance = System.Activator.CreateInstance(sourceType);
                if (newInstance == null)
                {
                    Console.WriteLine($"Warning: Could not create instance of {sourceType.Name}, returning original");
                    return source;
                }

                // Copy all public properties
                var properties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var prop in properties)
                {
                    if (!prop.CanRead || !prop.CanWrite)
                        continue;

                    var value = prop.GetValue(source);
                    if (value == null)
                    {
                        prop.SetValue(newInstance, null);
                        continue;
                    }

                    var propType = prop.PropertyType;
                    var valueType = value.GetType();

                    // Handle nullable types
                    if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        propType = propType.GetGenericArguments()[0];
                    }

                    // Handle FormLink types
                    if (IsFormLinkType(valueType))
                    {
                        prop.SetValue(newInstance, DeepCopyFormLink(value));
                        continue;
                    }

                    // Handle TranslatedString types
                    if (IsTranslatedStringType(valueType))
                    {
                        prop.SetValue(newInstance, DeepCopyTranslatedString(value));
                        continue;
                    }

                    // Handle ReadOnlyMemory<T> and ReadOnlySpan<T> (convert to arrays)
                    if (IsReadOnlyMemoryType(valueType) || IsReadOnlySpanType(valueType))
                    {
                        prop.SetValue(newInstance, ConvertReadOnlyMemoryToArray(value));
                        continue;
                    }

                    // Handle collections
                    if (value is IEnumerable collection && !(value is string))
                    {
                        prop.SetValue(newInstance, DeepCopyCollection(collection, propType));
                        continue;
                    }

                    // Handle value types and primitives
                    if (propType.IsValueType || propType.IsPrimitive || propType == typeof(string))
                    {
                        prop.SetValue(newInstance, value);
                        continue;
                    }

                    // Handle nested complex objects - recursive deep copy
                    prop.SetValue(newInstance, DeepCopy(value));
                }

                return newInstance;
            }
            catch (Exception ex)
            {
                LogCollector.AddWarning(PropertyName, $"Could not deep-copy {sourceType.Name}; using the original value", ex);
                return source;
            }
        }

        /// <summary>
        /// Checks if a type is a TranslatedString type (ITranslatedStringGetter, TranslatedString).
        /// </summary>
        private bool IsTranslatedStringType(Type type)
        {
            if (type == null) return false;

            // Check if it implements ITranslatedStringGetter
            var interfaces = type.GetInterfaces();
            if (interfaces.Any(i => i.Name == "ITranslatedStringGetter" || i.Name == "ITranslatedString"))
            {
                return true;
            }

            // Check if it's a TranslatedString type
            if (type.Name == "TranslatedString" || type.Name == "ITranslatedStringGetter")
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deep copies a TranslatedString by creating a new instance and copying the String property.
        /// </summary>
        private object DeepCopyTranslatedString(object translatedString)
        {
            try
            {
                // Get the String property from the getter
                var stringProperty = translatedString.GetType().GetProperty("String");
                if (stringProperty == null)
                {
                    Console.WriteLine($"Warning: Could not find String property on TranslatedString");
                    return translatedString;
                }

                var stringValue = stringProperty.GetValue(translatedString);

                // Create new TranslatedString
                var newTranslatedString = new TranslatedString(Language.English);
                if (stringValue != null)
                {
                    newTranslatedString.String = stringValue.ToString();
                }

                return newTranslatedString;
            }
            catch (Exception ex)
            {
                LogCollector.AddWarning(PropertyName, "Could not copy TranslatedString; using the original value", ex);
                return translatedString;
            }
        }

        /// <summary>
        /// Checks if a type is a FormLink type (IFormLink, IFormLinkNullable, FormLink, FormLinkNullable).
        /// </summary>
        private bool IsFormLinkType(Type type)
        {
            if (type == null) return false;

            // Check if it implements IFormLinkGetter or IFormLinkNullableGetter
            var interfaces = type.GetInterfaces();
            if (interfaces.Any(i =>
                (i.IsGenericType && i.GetGenericTypeDefinition().Name.StartsWith("IFormLink")) ||
                i.Name.StartsWith("IFormLink")))
            {
                return true;
            }

            // Check if it's a FormLink or FormLinkNullable type
            if (type.Name.StartsWith("FormLink"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deep copies a FormLink by extracting the FormKey and creating a new FormLink instance.
        /// </summary>
        private object DeepCopyFormLink(object formLink)
        {
            try
            {
                // Get the FormKey property
                var formKeyProperty = formLink.GetType().GetProperty("FormKey");
                if (formKeyProperty == null)
                {
                    return formLink;
                }

                var formKey = formKeyProperty.GetValue(formLink);
                if (formKey == null)
                {
                    return formLink;
                }

                // Determine the target type from the FormLink's generic parameter
                var formLinkType = formLink.GetType();
                Type? targetType = null;

                // Try to get the generic argument
                if (formLinkType.IsGenericType)
                {
                    var genericArgs = formLinkType.GetGenericArguments();
                    if (genericArgs.Length > 0)
                    {
                        targetType = genericArgs[0];
                    }
                }

                // Try to find the target type from interfaces
                if (targetType == null)
                {
                    var interfaces = formLinkType.GetInterfaces();
                    var formLinkInterface = interfaces.FirstOrDefault(i =>
                        i.IsGenericType && i.Name.StartsWith("IFormLink"));
                    if (formLinkInterface != null)
                    {
                        var genericArgs = formLinkInterface.GetGenericArguments();
                        if (genericArgs.Length > 0)
                        {
                            targetType = genericArgs[0];
                        }
                    }
                }

                if (targetType == null)
                {
                    Console.WriteLine($"Warning: Could not determine target type for FormLink {formLinkType.Name}");
                    return formLink;
                }

                // Create new FormLink or FormLinkNullable
                if (formLinkType.Name.Contains("Nullable"))
                {
                    var formLinkNullableType = typeof(FormLinkNullable<>).MakeGenericType(targetType);
                    var constructor = formLinkNullableType.GetConstructor(new[] { typeof(FormKey) });
                    if (constructor != null)
                    {
                        return constructor.Invoke(new[] { formKey });
                    }
                }
                else
                {
                    var formLinkTypeGeneric = typeof(FormLink<>).MakeGenericType(targetType);
                    var constructor = formLinkTypeGeneric.GetConstructor(new[] { typeof(FormKey) });
                    if (constructor != null)
                    {
                        return constructor.Invoke(new[] { formKey });
                    }
                }

                return formLink;
            }
            catch (Exception ex)
            {
                LogCollector.AddWarning(PropertyName, "Could not copy FormLink; using the original value", ex);
                return formLink;
            }
        }

        /// <summary>
        /// Deep copies a collection by creating a new collection and deep copying each item.
        /// </summary>
        private object DeepCopyCollection(IEnumerable collection, Type collectionType)
        {
            try
            {
                // Try to create a new instance of the collection type
                object? newCollection = null;

                // Handle ExtendedList<T> and similar Mutagen collection types
                if (collectionType.IsGenericType)
                {
                    var genericArgs = collectionType.GetGenericArguments();
                    if (genericArgs.Length > 0)
                    {
                        var itemType = genericArgs[0];
                        var genericListType = typeof(List<>).MakeGenericType(itemType);

                        // Try to create ExtendedList if the original was ExtendedList
                        if (collectionType.Name.Contains("ExtendedList"))
                        {
                            var extendedListType = typeof(ExtendedList<>).MakeGenericType(itemType);
                            newCollection = System.Activator.CreateInstance(extendedListType);
                        }
                        else
                        {
                            // Try to create the same collection type, or fall back to List<T>
                            try
                            {
                                newCollection = System.Activator.CreateInstance(collectionType);
                            }
                            catch (Exception ex)
                            {
                                LogCollector.AddDiagnostic(
                                    PropertyName,
                                    $"Could not instantiate collection type {collectionType.Name}; using List<T> fallback",
                                    ex);
                                newCollection = System.Activator.CreateInstance(genericListType);
                            }
                        }

                        if (newCollection != null)
                        {
                            // Get the Add method
                            var addMethod = newCollection.GetType().GetMethod("Add");
                            if (addMethod != null)
                            {
                                foreach (var item in collection)
                                {
                                    if (item == null)
                                    {
                                        addMethod.Invoke(newCollection, new object?[] { null });
                                    }
                                    else
                                    {
                                        var copiedItem = DeepCopy(item);
                                        addMethod.Invoke(newCollection, new[] { copiedItem });
                                    }
                                }
                            }
                        }
                    }
                }

                // Fallback: try to create the same type and use ICollection interface
                if (newCollection == null)
                {
                    try
                    {
                        newCollection = System.Activator.CreateInstance(collectionType);
                        if (newCollection is ICollection<object> genericCollection)
                        {
                            foreach (var item in collection)
                            {
                                genericCollection.Add(item != null ? DeepCopy(item) : null!);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogCollector.AddWarning(
                            PropertyName,
                            $"Could not deep-copy collection type {collectionType.Name}; using the original collection",
                            ex);
                        return collection;
                    }
                }

                return newCollection ?? collection;
            }
            catch (Exception ex)
            {
                LogCollector.AddWarning(PropertyName, "Could not deep-copy collection; using the original collection", ex);
                return collection;
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

            // Try to use the type's Equals method if it implements IEquatable with a getter interface
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
                        var result = equalsMethod.Invoke(value1, new object[] { value2 });
                        if (result is bool boolResult)
                        {
                            return boolResult;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogCollector.AddDiagnostic(
                            PropertyName,
                            $"IEquatable comparison failed for {valueType.Name}; trying another equality strategy",
                            ex);
                    }
                }
            }

            // Try to use the type's Equals method
            try
            {
                return value1.Equals(value2);
            }
            catch (Exception ex)
            {
                LogCollector.AddDiagnostic(
                    PropertyName,
                    $"Equals failed for {valueType.Name}; comparing properties recursively",
                    ex);
                return CompareProperties(value1, value2);
            }
        }

        /// <summary>
        /// Compares two objects by comparing all their properties recursively.
        /// </summary>
        private bool CompareProperties(object obj1, object obj2)
        {
            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null || obj2 == null) return false;
            if (obj1.GetType() != obj2.GetType()) return false;

            var properties = obj1.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                if (!prop.CanRead) continue;

                var val1 = prop.GetValue(obj1);
                var val2 = prop.GetValue(obj2);

                if (val1 == null && val2 == null) continue;
                if (val1 == null || val2 == null) return false;

                // Handle ReadOnlyMemory<T> and ReadOnlySpan<T> - convert to arrays and compare
                var val1Type = val1.GetType();
                var val2Type = val2.GetType();
                if (IsReadOnlyMemoryType(val1Type) || IsReadOnlySpanType(val1Type) ||
                    IsReadOnlyMemoryType(val2Type) || IsReadOnlySpanType(val2Type))
                {
                    var arr1 = ConvertReadOnlyMemoryToArray(val1);
                    var arr2 = ConvertReadOnlyMemoryToArray(val2);
                    if (arr1 is Array a1 && arr2 is Array a2)
                    {
                        if (a1.Length != a2.Length) return false;
                        for (int i = 0; i < a1.Length; i++)
                        {
                            if (!Equals(a1.GetValue(i), a2.GetValue(i))) return false;
                        }
                        continue;
                    }
                    if (!Equals(arr1, arr2)) return false;
                    continue;
                }

                // Handle collections
                if (val1 is IEnumerable collection1 && !(val1 is string))
                {
                    if (!(val2 is IEnumerable collection2)) return false;
                    if (!CompareCollections(collection1, collection2)) return false;
                    continue;
                }

                // Handle FormLink - compare by FormKey
                if (IsFormLinkType(val1.GetType()))
                {
                    var formKey1 = val1.GetType().GetProperty("FormKey")?.GetValue(val1);
                    var formKey2 = val2.GetType().GetProperty("FormKey")?.GetValue(val2);
                    if (!Equals(formKey1, formKey2)) return false;
                    continue;
                }

                // Handle TranslatedString - compare by String property
                if (IsTranslatedStringType(val1.GetType()))
                {
                    var string1 = val1.GetType().GetProperty("String")?.GetValue(val1);
                    var string2 = val2.GetType().GetProperty("String")?.GetValue(val2);
                    if (!StringComparisonHelper.EqualsNormalized(string1?.ToString(), string2?.ToString())) return false;
                    continue;
                }

                // Handle value types and primitives
                if (val1.GetType().IsValueType || val1.GetType().IsPrimitive || val1 is string)
                {
                    if (!Equals(val1, val2)) return false;
                    continue;
                }

                // Handle nested objects - recursive comparison
                if (!CompareProperties(val1, val2)) return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if a type is ReadOnlyMemory<T>.
        /// </summary>
        private bool IsReadOnlyMemoryType(Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition().FullName == "System.ReadOnlyMemory`1";
        }

        /// <summary>
        /// Checks if a type is ReadOnlySpan<T>.
        /// </summary>
        private bool IsReadOnlySpanType(Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition().FullName == "System.ReadOnlySpan`1";
        }

        /// <summary>
        /// Converts ReadOnlyMemory<T> or ReadOnlySpan<T> to an array.
        /// </summary>
        private object? ConvertReadOnlyMemoryToArray(object value)
        {
            try
            {
                var valueType = value.GetType();

                // Handle ReadOnlyMemory<T>
                if (IsReadOnlyMemoryType(valueType))
                {
                    // Use reflection to call ToArray() method
                    var toArrayMethod = valueType.GetMethod("ToArray");
                    if (toArrayMethod != null)
                    {
                        return toArrayMethod.Invoke(value, null);
                    }

                    // Fallback: use Span property
                    var spanProperty = valueType.GetProperty("Span");
                    if (spanProperty != null)
                    {
                        var span = spanProperty.GetValue(value);
                        if (span != null)
                        {
                            var spanToArrayMethod = span.GetType().GetMethod("ToArray");
                            if (spanToArrayMethod != null)
                            {
                                return spanToArrayMethod.Invoke(span, null);
                            }
                        }
                    }
                }

                // Handle ReadOnlySpan<T>
                if (IsReadOnlySpanType(valueType))
                {
                    // Use reflection to call ToArray() method
                    var toArrayMethod = valueType.GetMethod("ToArray");
                    if (toArrayMethod != null)
                    {
                        return toArrayMethod.Invoke(value, null);
                    }
                }

                Console.WriteLine($"Warning: Could not convert {valueType.Name} to array");
                return value;
            }
            catch (Exception ex)
            {
                LogCollector.AddWarning(PropertyName, "Could not convert ReadOnlyMemory/Span to an array; using the original value", ex);
                return value;
            }
        }

        /// <summary>
        /// Compares two collections by comparing their items.
        /// </summary>
        private bool CompareCollections(IEnumerable collection1, IEnumerable collection2)
        {
            var list1 = collection1.Cast<object>().ToList();
            var list2 = collection2.Cast<object>().ToList();

            if (list1.Count != list2.Count) return false;

            for (int i = 0; i < list1.Count; i++)
            {
                var item1 = list1[i];
                var item2 = list2[i];

                if (item1 == null && item2 == null) continue;
                if (item1 == null || item2 == null) return false;

                // Handle FormLink - compare by FormKey
                if (IsFormLinkType(item1.GetType()))
                {
                    var formKey1 = item1.GetType().GetProperty("FormKey")?.GetValue(item1);
                    var formKey2 = item2.GetType().GetProperty("FormKey")?.GetValue(item2);
                    if (!Equals(formKey1, formKey2)) return false;
                    continue;
                }

                // Handle value types and primitives
                if (item1.GetType().IsValueType || item1.GetType().IsPrimitive || item1 is string)
                {
                    if (!Equals(item1, item2)) return false;
                    continue;
                }

                // Handle complex objects - recursive comparison
                if (!CompareProperties(item1, item2)) return false;
            }

            return true;
        }
    }
}
