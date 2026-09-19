using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;
using DreadsMashedPatch.PropertyHandlers.Formatting;
using Noggog;

namespace DreadsMashedPatch.PropertyHandlers.General
{
    /// <summary>
    /// A generic list property handler that uses reflection to access list properties.
    /// Supports both simple FormLink lists and complex object lists.
    /// 
    /// Features:
    /// - Automatic detection of FormLink vs complex object items
    /// - Deep copying for complex objects
    /// - Explicit list semantics; callers must classify every registration
    /// - Automatic item equality detection (FormKey for FormLinks, property comparison for complex objects)
    /// </summary>
    /// <typeparam name="TItem">The type of items in the list (e.g., IFormLinkGetter&lt;IPlacedObjectGetter&gt; or ILinkedReferencesGetter)</typeparam>
    /// <typeparam name="TRecord">The record type that contains the property (e.g., IPlacedObject)</typeparam>
    /// <typeparam name="TRecordGetter">The getter record type (e.g., IPlacedObjectGetter)</typeparam>
    public class SimpleReflectionListPropertyHandler<TItem, TRecord, TRecordGetter> : AbstractListPropertyHandler<TItem>
        where TItem : class
        where TRecord : class, IMajorRecord
        where TRecordGetter : class, IMajorRecordGetter
    {
        private readonly string _propertyName;
        private readonly PropertyInfo? _getterProperty;
        private readonly PropertyInfo? _setterProperty;
        private readonly bool _isFormLinkList;
        private readonly bool _canBeNull;
        private readonly Func<TItem, object?>? _keySelector;
        private readonly Type? _mutableItemType; // For complex objects, the mutable type (e.g., LinkedReferences)

        public SimpleReflectionListPropertyHandler(
            string propertyName,
            ListSemantics semantics,
            bool? canBeNull = null,
            Func<TItem, object?>? keySelector = null)
        {
            _propertyName = propertyName;
            _semantics = semantics;
            _keySelector = keySelector;

            // Find the property on the getter interface
            _getterProperty = ReflectionPropertyResolver.Find(typeof(TRecordGetter), propertyName);

            // Find the property on the setter interface
            _setterProperty = ReflectionPropertyResolver.Find(typeof(TRecord), propertyName);

            if (_getterProperty == null)
            {
                throw new ArgumentException(
                    $"Property '{propertyName}' not found on {typeof(TRecordGetter).Name}");
            }

            _canBeNull = canBeNull ?? ReflectionPropertyResolver.IsNullable(_getterProperty);

            // Detect if items are FormLinks
            _isFormLinkList = IsFormLinkType(typeof(TItem));

            // For complex objects, try to find the mutable type
            if (!_isFormLinkList)
            {
                _mutableItemType = FindMutableType(typeof(TItem));
            }
        }

        public override string PropertyName => _propertyName;

        private readonly ListSemantics _semantics;

        public override ListSemantics Semantics => _semantics;
        protected override bool CanBeNull => _canBeNull;

        public override List<TItem>? GetValue(IMajorRecordGetter record)
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
                var value = _getterProperty.GetValue(typedRecord);
                if (value == null)
                {
                    return null;
                }

                // Handle different collection types
                List<TItem>? list;
                if (value is IEnumerable<TItem> enumerable)
                {
                    list = enumerable.ToList();
                }
                else if (value is IEnumerable enumerableNonGeneric)
                {
                    list = enumerableNonGeneric.Cast<TItem>().ToList();
                }
                else
                {
                    return null;
                }

                if (list != null && _isFormLinkList && PropertyName == "LocationRefTypes")
                {
                    var keysBefore = string.Join(", ", list.Select((item, i) => $"[{i}] {GetFormKey(item)}"));
                    var refsBefore = string.Join(", ", list.Select((item, i) => $"[{i}] {RuntimeHelpers.GetHashCode(item)}"));
                    Console.WriteLine($"[DEBUG {PropertyName}] GetValue: count={list.Count} before norm. FormKeys=[{keysBefore}]");
                    Console.WriteLine($"[DEBUG {PropertyName}] GetValue: refs (hash) before norm = [{refsBefore}]");
                }

                // For FormLink lists: normalize so items with the same FormKey share the same reference.
                // This ensures the abstract class's GroupBy(item => item) and addition logic don't treat
                // duplicate FormKeys as separate groups (which would add 2+2+1+1=6 instead of 2+1+1=4).
                if (list != null && _isFormLinkList && list.Count > 1)
                {
                    var byKey = new Dictionary<FormKey, TItem>();
                    for (int i = 0; i < list.Count; i++)
                    {
                        var key = GetFormKey(list[i]);
                        if (byKey.TryGetValue(key, out var existing))
                            list[i] = existing;
                        else
                            byKey[key] = list[i];
                    }
                }

                if (list != null && _isFormLinkList && PropertyName == "LocationRefTypes")
                {
                    var keysAfter = string.Join(", ", list.Select((item, i) => $"[{i}] {GetFormKey(item)}"));
                    var refsAfter = string.Join(", ", list.Select((item, i) => $"[{i}] {RuntimeHelpers.GetHashCode(item)}"));
                    Console.WriteLine($"[DEBUG {PropertyName}] GetValue: count={list.Count} after norm. FormKeys=[{keysAfter}]");
                    Console.WriteLine($"[DEBUG {PropertyName}] GetValue: refs (hash) after norm = [{refsAfter}]");
                }
                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting property '{PropertyName}' via reflection: {ex.Message}");
                return null;
            }
        }

        public override void SetValue(IMajorRecord record, List<TItem>? value)
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
                var currentList = _setterProperty.GetValue(typedRecord);

                // Handle nullable properties (like LocationRefTypes)
                if (_canBeNull && value == null)
                {
                    _setterProperty.SetValue(typedRecord, null);
                    return;
                }

                // If currentList is null, we need to create a new list
                if (currentList == null && value != null)
                {
                    // Check if the property type is nullable
                    var propertyType = _setterProperty.PropertyType;
                    Type? listType = null;
                    bool propertyIsNullable = false;

                    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        listType = propertyType.GetGenericArguments()[0];
                        propertyIsNullable = true;
                    }
                    else
                    {
                        listType = propertyType;
                    }

                    // If property is not nullable and we have a value, create a new list
                    if (!propertyIsNullable && listType != null && listType.IsGenericType)
                    {
                        var genericArgs = listType.GetGenericArguments();
                        if (genericArgs.Length > 0)
                        {
                            var itemType = genericArgs[0];
                            var newList = CreateListInstance(listType, itemType);
                            if (newList != null)
                            {
                                foreach (var item in value)
                                {
                                    if (item == null) continue;

                                    object? itemToAdd = null;
                                    if (_isFormLinkList)
                                    {
                                        itemToAdd = CreateFormLinkFromGetter(item);
                                    }
                                    else
                                    {
                                        itemToAdd = DeepCopyComplexObject(item);
                                    }

                                    if (itemToAdd != null)
                                    {
                                        AddItem(newList, itemToAdd);
                                    }
                                }
                                _setterProperty.SetValue(typedRecord, newList);
                                return;
                            }
                        }
                    }

                    // If we can't create a list and property is not nullable, warn and return
                    if (!propertyIsNullable)
                    {
                        Console.WriteLine($"Warning: Property '{PropertyName}' is null and cannot be set (property is not nullable and list creation failed)");
                        return;
                    }
                }

                // Clear the existing list
                if (currentList != null)
                {
                    var clearMethod = currentList.GetType().GetMethod("Clear");
                    if (clearMethod != null)
                    {
                        clearMethod.Invoke(currentList, null);
                    }
                }

                // Add new items to existing list
                if (value != null && currentList != null)
                {
                    foreach (var item in value)
                    {
                        if (item == null) continue;

                        object? itemToAdd = null;

                        if (_isFormLinkList)
                        {
                            // For FormLinks, create new FormLink from FormKey
                            itemToAdd = CreateFormLinkFromGetter(item);
                        }
                        else
                        {
                            // For complex objects, create mutable instance and copy properties
                            itemToAdd = DeepCopyComplexObject(item);
                        }

                        if (itemToAdd != null)
                        {
                            AddItem(currentList, itemToAdd);
                        }
                    }
                }
                else if (value != null && currentList == null && _canBeNull)
                {
                    // Create a new list if property is nullable
                    var listType = _setterProperty.PropertyType;
                    if (listType.IsGenericType)
                    {
                        var genericArgs = listType.GetGenericArguments();
                        if (genericArgs.Length > 0)
                        {
                            var itemType = genericArgs[0];
                            var newList = CreateListInstance(listType, itemType);
                            if (newList != null)
                            {
                                foreach (var item in value)
                                {
                                    if (item == null) continue;

                                    object? itemToAdd = null;
                                    if (_isFormLinkList)
                                    {
                                        itemToAdd = CreateFormLinkFromGetter(item);
                                    }
                                    else
                                    {
                                        itemToAdd = DeepCopyComplexObject(item);
                                    }

                                    if (itemToAdd != null)
                                    {
                                        AddItem(newList, itemToAdd);
                                    }
                                }
                                _setterProperty.SetValue(typedRecord, newList);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting property '{PropertyName}' via reflection: {ex.Message}");
            }
        }

        protected override bool IsItemEqual(TItem? item1, TItem? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            if (_isFormLinkList)
            {
                // For FormLinks, compare by FormKey
                var formKey1 = GetFormKey(item1);
                var formKey2 = GetFormKey(item2);
                return formKey1.Equals(formKey2);
            }
            else if (typeof(TItem).IsValueType || item1 is string)
            {
                // Scalar items use typed equality. Reflecting over string would inspect its
                // indexed Chars property and throw TargetParameterCountException.
                return EqualityComparer<TItem>.Default.Equals(item1, item2);
            }
            else
            {
                // For complex objects, compare all properties
                return CompareComplexObjects(item1, item2);
            }
        }

        protected override bool IsItemIdentityEqual(TItem? item1, TItem? item2)
        {
            if (_keySelector == null)
            {
                return base.IsItemIdentityEqual(item1, item2);
            }

            if (item1 == null || item2 == null)
            {
                return item1 == null && item2 == null;
            }

            return Equals(_keySelector(item1), _keySelector(item2));
        }

        protected override IReadOnlyList<object?> GetSortKey(TItem item)
        {
            if (_keySelector != null)
            {
                var key = _keySelector(item);
                return key is IReadOnlyList<object?> composite ? composite : [key];
            }

            if (_isFormLinkList)
            {
                return [GetFormKey(item)];
            }

            if (item is string || item is IComparable || item.GetType().IsEnum)
            {
                return [item];
            }

            return base.GetSortKey(item);
        }

        private static void AddItem(object list, object item)
        {
            if (list is IList nonGenericList)
            {
                nonGenericList.Add(item);
                return;
            }

            var addMethod = list.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(method =>
                {
                    if (method.Name != "Add") return false;
                    var parameters = method.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType.IsInstanceOfType(item);
                });

            if (addMethod == null)
            {
                throw new InvalidOperationException($"No compatible Add method found on {list.GetType().Name} for {item.GetType().Name}.");
            }

            addMethod.Invoke(list, new[] { item });
        }

        protected override string FormatItem(TItem? item)
        {
            if (item == null) return "null";

            if (_isFormLinkList)
            {
                var formKey = GetFormKey(item);
                return $"FormLink({formKey})";
            }
            else
            {
                // For complex objects, format key properties
                return FormatComplexObject(item);
            }
        }

        /// <summary>
        /// Checks if a type is a FormLink type.
        /// </summary>
        private bool IsFormLinkType(Type type)
        {
            if (type == null) return false;

            // Check if it implements IFormLinkGetter or IFormLinkNullableGetter specifically
            // (not IFormLinkContainerGetter which is implemented by complex objects)
            var interfaces = type.GetInterfaces();
            if (interfaces.Any(i =>
                (i.IsGenericType &&
                 (i.GetGenericTypeDefinition().Name == "IFormLinkGetter`1" ||
                  i.GetGenericTypeDefinition().Name == "IFormLinkNullableGetter`1")) ||
                (i.Name == "IFormLinkGetter" || i.Name == "IFormLinkNullableGetter")))
            {
                return true;
            }

            // Check if it's a FormLink or FormLinkNullable type
            if (type.Name == "FormLink`1" || type.Name == "FormLinkNullable`1" ||
                (type.IsGenericType &&
                 (type.GetGenericTypeDefinition().Name == "FormLink`1" ||
                  type.GetGenericTypeDefinition().Name == "FormLinkNullable`1")))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the FormKey from a FormLink getter.
        /// </summary>
        private FormKey GetFormKey(TItem formLink)
        {
            var formKeyProperty = formLink.GetType().GetProperty("FormKey");
            if (formKeyProperty != null)
            {
                var formKey = formKeyProperty.GetValue(formLink);
                if (formKey is FormKey fk)
                {
                    return fk;
                }
            }
            return FormKey.Null;
        }

        /// <summary>
        /// Creates a new FormLink from a FormLink getter. For null FormKeys returns a default FormLinkNullable
        /// so that null entries are preserved in lists (e.g. LitWater).
        /// </summary>
        private object? CreateFormLinkFromGetter(TItem formLinkGetter)
        {
            var formKey = GetFormKey(formLinkGetter);
            var getterType = formLinkGetter.GetType();
            var interfaces = getterType.GetInterfaces();
            var formLinkInterface = interfaces.FirstOrDefault(i =>
                i.IsGenericType && (i.Name.StartsWith("IFormLink") || i.Name.StartsWith("IFormLinkNullable")));
            if (formLinkInterface == null)
                return null;

            var targetType = formLinkInterface.GetGenericArguments()[0];
            var isNullable = formLinkInterface.Name.StartsWith("IFormLinkNullable");
            if (formKey.IsNull)
            {
                if (isNullable)
                {
                    var formLinkNullableType = typeof(FormLinkNullable<>).MakeGenericType(targetType);
                    return System.Activator.CreateInstance(formLinkNullableType);
                }
                // Non-nullable list (e.g. IFormLinkGetter): still add FormLink(T)(FormKey.Null) so null entry is written
                var formLinkTypeNull = typeof(FormLink<>).MakeGenericType(targetType);
                var ctorNull = formLinkTypeNull.GetConstructor(new[] { typeof(FormKey) });
                return ctorNull != null ? System.Activator.CreateInstance(formLinkTypeNull, FormKey.Null) : null;
            }

            var formLinkType = isNullable
                ? typeof(FormLinkNullable<>).MakeGenericType(targetType)
                : typeof(FormLink<>).MakeGenericType(targetType);
            var constructor = formLinkType.GetConstructor(new[] { typeof(FormKey) });
            return constructor != null ? System.Activator.CreateInstance(formLinkType, formKey) : null;
        }

        /// <summary>
        /// Finds the mutable type for a getter interface (e.g., LinkedReferences from ILinkedReferencesGetter).
        /// </summary>
        private Type? FindMutableType(Type getterType)
        {
            // Try removing "Getter" suffix and "I" prefix
            var typeName = getterType.Name;
            if (typeName.StartsWith("I") && typeName.EndsWith("Getter"))
            {
                var baseName = typeName.Substring(1, typeName.Length - 7); // Remove "I" and "Getter"
                var mutableTypeName = baseName;

                // Try to find the type in the same namespace
                var namespaceName = getterType.Namespace;
                if (namespaceName != null)
                {
                    var assembly = getterType.Assembly;
                    var mutableType = assembly.GetType($"{namespaceName}.{mutableTypeName}");
                    if (mutableType != null)
                    {
                        return mutableType;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Sets a property to null or its default value. For value types (e.g. FormLinkNullable struct),
        /// uses default instance instead of null to avoid reflection SetValue(null) throwing on structs.
        /// </summary>
        private static void SetPropertyValueOrDefault(object instance, PropertyInfo property, object? value)
        {
            if (value != null)
            {
                try
                {
                    property.SetValue(instance, value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not set property {property.Name}: {ex.Message}");
                }
                return;
            }
            // Try Mutagen's SetTo(null) pattern first for FormLinkNullable-style properties
            if (TrySetFormLinkToNull(instance, property))
                return;
            var propType = property.PropertyType;
            if (propType.IsValueType)
            {
                try
                {
                    var defaultValue = System.Activator.CreateInstance(propType);
                    property.SetValue(instance, defaultValue);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not create default for {propType.Name}: {ex.Message}");
                }
            }
            else
            {
                try
                {
                    property.SetValue(instance, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not set property {property.Name} to null: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Tries to set a FormLinkNullable-style property to null by calling SetTo(FormKey.Null) or SetTo(null)
        /// on the property's current value (Mutagen pattern). Returns true if successful.
        /// </summary>
        private static bool TrySetFormLinkToNull(object instance, PropertyInfo property)
        {
            try
            {
                if (!property.CanRead) return false;
                var linkHolder = property.GetValue(instance);
                if (linkHolder == null) return false;
                var type = linkHolder.GetType();
                // Prefer SetTo(FormKey) with FormKey.Null (clear link)
                var setToFormKey = type.GetMethod("SetTo", new[] { typeof(FormKey) });
                if (setToFormKey != null)
                {
                    setToFormKey.Invoke(linkHolder, new object[] { FormKey.Null });
                    return true;
                }
                var setToNullable = type.GetMethod("SetTo", new[] { typeof(FormKey?) });
                if (setToNullable != null)
                {
                    setToNullable.Invoke(linkHolder, new object?[] { null });
                    return true;
                }
                var setToObject = type.GetMethod("SetTo", new[] { typeof(object) });
                if (setToObject != null)
                {
                    setToObject.Invoke(linkHolder, new object?[] { null });
                    return true;
                }
            }
            catch (Exception)
            {
                // Ignore and fall back to SetValue
            }
            return false;
        }

        /// <summary>
        /// Deep copies a complex object by creating a mutable instance and copying properties.
        /// </summary>
        private object? DeepCopyComplexObject(TItem getter)
        {
            // Immutable scalar reference types do not need mutable-type discovery.
            // In particular, string lists otherwise lose every item because there is
            // no generated mutable counterpart named "String".
            if (getter is string)
            {
                return getter;
            }

            if (_mutableItemType == null)
            {
                Console.WriteLine($"Warning: Could not find mutable type for {typeof(TItem).Name}, returning null");
                return null;
            }

            try
            {
                var mutableInstance = System.Activator.CreateInstance(_mutableItemType);
                if (mutableInstance == null)
                {
                    Console.WriteLine($"Warning: Could not create instance of {_mutableItemType.Name}");
                    return null;
                }

                // Copy all public properties
                var getterProperties = typeof(TItem).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var mutableProperties = _mutableItemType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var getterProp in getterProperties)
                {
                    if (!getterProp.CanRead) continue;

                    var mutableProp = mutableProperties.FirstOrDefault(p => p.Name == getterProp.Name && p.CanWrite);
                    if (mutableProp == null) continue;

                    object? value;
                    try
                    {
                        value = getterProp.GetValue(getter);
                    }
                    catch (Exception getEx)
                    {
                        // Getter can throw for invalid/null FormLink (e.g. binary overlay); set default and continue
                        Console.WriteLine($"Warning: Could not read property '{getterProp.Name}' in {typeof(TItem).Name}: {getEx.InnerException?.Message ?? getEx.Message}");
                        SetPropertyValueOrDefault(mutableInstance, mutableProp, null);
                        continue;
                    }
                    if (value == null)
                    {
                        SetPropertyValueOrDefault(mutableInstance, mutableProp, null);
                        continue;
                    }

                    var propType = mutableProp.PropertyType;
                    var valueType = value.GetType();

                    // Handle nullable types
                    if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        propType = propType.GetGenericArguments()[0];
                    }

                    // Mutagen getter interfaces expose binary payloads as ReadOnlyMemorySlice<byte>
                    // while mutable implementations accept MemorySlice<byte>.
                    if (value is ReadOnlyMemorySlice<byte> byteSlice && propType == typeof(MemorySlice<byte>))
                    {
                        mutableProp.SetValue(mutableInstance, new MemorySlice<byte>(byteSlice.ToArray()));
                        continue;
                    }

                    // Handle FormLink types
                    if (IsFormLinkType(valueType))
                    {
                        try
                        {
                            FormKey? formKey = null;
                            try
                            {
                                var formKeyProperty = valueType.GetProperty("FormKey");
                                if (formKeyProperty != null && (formKeyProperty.GetValue(value) is FormKey fk))
                                    formKey = fk;
                            }
                            catch (Exception)
                            {
                                // Reflection can throw for null/default FormLink; treat as null
                            }
                            if (!formKey.HasValue)
                            {
                                try
                                {
                                    dynamic formLinkDynamic = value;
                                    var dynamicFormKey = formLinkDynamic.FormKey;
                                    if (dynamicFormKey is FormKey dFk)
                                        formKey = dFk;
                                    else if (dynamicFormKey != null)
                                        formKey = dynamicFormKey as FormKey?;
                                }
                                catch (Exception)
                                {
                                    // Dynamic fallback failed; treat as null FormLink
                                }
                            }
                            if (!formKey.HasValue || formKey.Value.IsNull)
                            {
                                SetPropertyValueOrDefault(mutableInstance, mutableProp, null);
                                continue;
                            }

                            var formLinkInterfaces = valueType.GetInterfaces();
                            var formLinkInterface = formLinkInterfaces.FirstOrDefault(i =>
                                i.IsGenericType &&
                                (i.GetGenericTypeDefinition().Name == "IFormLinkGetter`1" ||
                                 i.GetGenericTypeDefinition().Name == "IFormLinkNullableGetter`1"));
                            if (formLinkInterface != null)
                            {
                                var targetType = formLinkInterface.GetGenericArguments()[0];
                                var isNullable = formLinkInterface.GetGenericTypeDefinition().Name == "IFormLinkNullableGetter`1";
                                var concreteFormLinkType = isNullable
                                    ? typeof(FormLinkNullable<>).MakeGenericType(targetType)
                                    : typeof(FormLink<>).MakeGenericType(targetType);
                                var constructor = concreteFormLinkType.GetConstructor(new[] { typeof(FormKey) });
                                if (constructor != null)
                                {
                                    mutableProp.SetValue(mutableInstance, constructor.Invoke(new object[] { formKey.Value }));
                                }
                                else
                                {
                                    SetPropertyValueOrDefault(mutableInstance, mutableProp, null);
                                }
                            }
                            else
                            {
                                SetPropertyValueOrDefault(mutableInstance, mutableProp, null);
                            }
                        }
                        catch (Exception formLinkEx)
                        {
                            Console.WriteLine($"Warning: Error copying FormLink property '{getterProp.Name}' in {typeof(TItem).Name}: {formLinkEx.Message}");
                            if (formLinkEx.InnerException != null)
                                Console.WriteLine($"  Inner exception: {formLinkEx.InnerException.Message}");
                            SetPropertyValueOrDefault(mutableInstance, mutableProp, null);
                        }
                        continue;
                    }

                    // Handle value types and primitives
                    if (propType.IsValueType || propType.IsPrimitive || propType == typeof(string))
                    {
                        mutableProp.SetValue(mutableInstance, value);
                        continue;
                    }

                    // For other types, try direct assignment (may fail for complex nested objects)
                    try
                    {
                        mutableProp.SetValue(mutableInstance, value);
                    }
                    catch
                    {
                        // Skip properties that can't be copied directly
                    }
                }

                return mutableInstance;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Error deep copying {typeof(TItem).Name}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return null;
            }
        }

        /// <summary>
        /// Compares two complex objects by comparing their properties.
        /// </summary>
        private bool CompareComplexObjects(TItem item1, TItem item2)
        {
            var properties = typeof(TItem).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.GetIndexParameters().Length == 0);
            foreach (var prop in properties)
            {
                if (!prop.CanRead) continue;

                var val1 = prop.GetValue(item1);
                var val2 = prop.GetValue(item2);

                if (val1 == null && val2 == null) continue;
                if (val1 == null || val2 == null) return false;

                // Handle FormLinks - compare by FormKey
                if (IsFormLinkType(val1.GetType()))
                {
                    var formKey1 = val1.GetType().GetProperty("FormKey")?.GetValue(val1);
                    var formKey2 = val2.GetType().GetProperty("FormKey")?.GetValue(val2);
                    if (!Equals(formKey1, formKey2)) return false;
                    continue;
                }

                // Handle value types and primitives
                if (val1.GetType().IsValueType || val1.GetType().IsPrimitive || val1 is string)
                {
                    if (!Equals(val1, val2)) return false;
                    continue;
                }

                // For other types, use default equality
                if (!Equals(val1, val2)) return false;
            }

            return true;
        }

        /// <summary>
        /// Formats a complex object for display.
        /// </summary>
        private string FormatComplexObject(TItem item)
        {
            return DiagnosticValueFormatter.Format(item);
        }

        /// <summary>
        /// Creates an instance of a list type.
        /// </summary>
        private object? CreateListInstance(Type listType, Type itemType)
        {
            try
            {
                // Try ExtendedList first (common in Mutagen)
                var extendedListType = typeof(ExtendedList<>).MakeGenericType(itemType);
                if (listType.IsAssignableFrom(extendedListType))
                {
                    return System.Activator.CreateInstance(extendedListType);
                }

                // Try List<T>
                var listGenericType = typeof(List<>).MakeGenericType(itemType);
                if (listType.IsAssignableFrom(listGenericType))
                {
                    return System.Activator.CreateInstance(listGenericType);
                }

                // Try direct instantiation
                return System.Activator.CreateInstance(listType);
            }
            catch
            {
                return null;
            }
        }
    }
}
