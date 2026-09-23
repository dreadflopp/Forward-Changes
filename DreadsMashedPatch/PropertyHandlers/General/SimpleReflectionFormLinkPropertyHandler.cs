using System;
using System.Reflection;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.Interfaces;

namespace DreadsMashedPatch.PropertyHandlers.General
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
                LogCollector.AddError(PropertyName, "Could not read the FormLink property via reflection", ex);
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
                    // Mutagen link properties expose their mutable link object through IFormLink.
                    // Prefer the typed API so nullable FormKey parameters do not need reflection matching.
                    if (currentValue is IFormLink<TTarget> currentLink)
                    {
                        currentLink.SetTo(value.FormKey);
                        return;
                    }

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
                    // Nullable links must contain an actual null FormKeyNullable so Mutagen omits
                    // the subrecord. FormKey.Null represents a present link to 00000000 instead.
                    if (currentValue is IFormLinkNullable<TTarget> nullableLink)
                    {
                        nullableLink.SetToNull();
                        return;
                    }

                    // Non-nullable links cannot represent absence; clear them to FormKey.Null.
                    if (currentValue is IFormLink<TTarget> currentLink)
                    {
                        currentLink.SetTo(FormKey.Null);
                        return;
                    }

                    // Reflection fallbacks for unusual generated link implementations.
                    if (currentValue != null)
                    {
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
                LogCollector.AddError(PropertyName, "Could not apply the FormLink property via reflection", ex);
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
