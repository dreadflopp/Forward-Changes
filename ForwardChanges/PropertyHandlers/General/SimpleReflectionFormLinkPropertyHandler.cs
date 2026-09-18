using System;
using System.Reflection;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.General
{
    /// <summary>
    /// A generic property handler that uses reflection to access FormLink properties.
    /// This handler is designed for simple FormLink properties that don't require special handling.
    /// 
    /// Supports both FormLink and FormLinkNullable types.
    /// </summary>
    /// <typeparam name="TTarget">The target type of the FormLink (e.g., IPlaceableObjectGetter)</typeparam>
    /// <typeparam name="TRecord">The record type that contains the property (e.g., IPlacedObject)</typeparam>
    /// <typeparam name="TRecordGetter">The getter record type (e.g., IPlacedObjectGetter)</typeparam>
    public class SimpleReflectionFormLinkPropertyHandler<TTarget, TRecord, TRecordGetter> : AbstractPropertyHandler<IFormLinkNullableGetter<TTarget>>
        where TTarget : class, IMajorRecordGetter
        where TRecord : class, IMajorRecord
        where TRecordGetter : class, IMajorRecordGetter
    {
        private readonly string _propertyName;
        private readonly PropertyInfo? _getterProperty;
        private readonly PropertyInfo? _setterProperty;

        public SimpleReflectionFormLinkPropertyHandler(string propertyName)
        {
            _propertyName = propertyName;

            // Find the property on the getter interface
            _getterProperty = ReflectionPropertyResolver.Find(typeof(TRecordGetter), propertyName);

            // Find the property on the setter interface
            _setterProperty = ReflectionPropertyResolver.Find(typeof(TRecord), propertyName);

            if (_getterProperty == null)
            {
                throw new ArgumentException(
                    $"Property '{propertyName}' not found on {typeof(TRecordGetter).Name}");
            }
        }

        public override string PropertyName => _propertyName;

        public override IFormLinkNullableGetter<TTarget>? GetValue(IMajorRecordGetter record)
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
                IFormLinkNullableGetter<TTarget>? castValue = null;

                if (value is IFormLinkNullableGetter<TTarget> nullableValue)
                {
                    castValue = nullableValue.FormKey.IsNull ? null : nullableValue;
                }
                else if (value is IFormLinkGetter<TTarget> nonNullableValue)
                {
                    castValue = nonNullableValue.FormKey.IsNull
                        ? null
                        : new FormLinkNullable<TTarget>(nonNullableValue.FormKey);
                }
                else if (value != null)
                {
                    // Reflection fallback for edge cases where runtime type doesn't directly match generic interfaces.
                    var formKeyProperty = value.GetType().GetProperty("FormKey", BindingFlags.Public | BindingFlags.Instance);
                    if (formKeyProperty?.GetValue(value) is FormKey reflectedFormKey && !reflectedFormKey.IsNull)
                    {
                        castValue = new FormLinkNullable<TTarget>(reflectedFormKey);
                    }
                }

                return castValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting property '{PropertyName}' via reflection: {ex.Message}");
                return null;
            }
        }

        public override void SetValue(IMajorRecord record, IFormLinkNullableGetter<TTarget>? value)
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
                var currentValue = _setterProperty.GetValue(typedRecord);

                if (value != null && !value.FormKey.IsNull)
                {
                    // Prefer mutating existing link object (works for both FormLink and FormLinkNullable)
                    if (currentValue != null)
                    {
                        var setToFormKey = currentValue.GetType().GetMethod("SetTo", new[] { typeof(FormKey) });
                        if (setToFormKey != null)
                        {
                            setToFormKey.Invoke(currentValue, new object[] { value.FormKey });
                            return;
                        }
                    }

                    // Fallback: create a new link object matching property type (nullable or non-nullable)
                    var propertyType = _setterProperty.PropertyType;
                    var concreteLinkType = propertyType;
                    if (propertyType.IsInterface || propertyType.IsAbstract)
                    {
                        concreteLinkType = typeof(FormLink<>).MakeGenericType(typeof(TTarget));
                    }

                    var constructor = concreteLinkType.GetConstructor(new[] { typeof(FormKey) });
                    if (constructor == null)
                    {
                        var nullableType = typeof(FormLinkNullable<>).MakeGenericType(typeof(TTarget));
                        constructor = nullableType.GetConstructor(new[] { typeof(FormKey) });
                        if (constructor != null)
                        {
                            concreteLinkType = nullableType;
                        }
                    }

                    if (constructor != null)
                    {
                        var newFormLink = constructor.Invoke(new object[] { value.FormKey });
                        _setterProperty.SetValue(typedRecord, newFormLink);
                        return;
                    }

                    Console.WriteLine($"Error: Could not construct link value for property '{PropertyName}'");
                }
                else
                {
                    // Clear the FormLink - prefer SetTo(FormKey.Null), then Clear(), then null assignment.
                    if (currentValue != null)
                    {
                        var setToFormKey = currentValue.GetType().GetMethod("SetTo", new[] { typeof(FormKey) });
                        if (setToFormKey != null)
                        {
                            setToFormKey.Invoke(currentValue, new object[] { FormKey.Null });
                            return;
                        }

                        var clearMethod = currentValue.GetType().GetMethod("Clear");
                        if (clearMethod != null)
                        {
                            clearMethod.Invoke(currentValue, null);
                            return;
                        }
                    }

                    // If property cannot be null, create a default null-form link instance.
                    var propertyType = _setterProperty.PropertyType;
                    var nullableType = typeof(FormLinkNullable<>).MakeGenericType(typeof(TTarget));
                    var nonNullableType = typeof(FormLink<>).MakeGenericType(typeof(TTarget));

                    if (propertyType == nonNullableType || (propertyType.IsInterface && propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IFormLinkGetter<>)))
                    {
                        _setterProperty.SetValue(typedRecord, System.Activator.CreateInstance(nonNullableType, FormKey.Null));
                    }
                    else if (propertyType == nullableType || (propertyType.IsInterface && propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IFormLinkNullableGetter<>)))
                    {
                        _setterProperty.SetValue(typedRecord, System.Activator.CreateInstance(nullableType));
                    }
                    else
                    {
                        _setterProperty.SetValue(typedRecord, null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting property '{PropertyName}' via reflection: {ex.Message}");
            }
        }

        public override bool AreValuesEqual(IFormLinkNullableGetter<TTarget>? value1, IFormLinkNullableGetter<TTarget>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.FormKey.Equals(value2.FormKey);
        }
    }
}
