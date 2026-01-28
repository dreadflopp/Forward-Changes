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
            _getterProperty = typeof(TRecordGetter).GetProperty(propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            
            // Find the property on the setter interface
            _setterProperty = typeof(TRecord).GetProperty(propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

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
                return value as IFormLinkNullableGetter<TTarget>;
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
                if (value != null && !value.FormKey.IsNull)
                {
                    // Create a new FormLinkNullable with the FormKey
                    var formLinkNullableType = typeof(FormLinkNullable<>).MakeGenericType(typeof(TTarget));
                    var constructor = formLinkNullableType.GetConstructor(new[] { typeof(FormKey) });
                    if (constructor != null)
                    {
                        var newFormLink = constructor.Invoke(new object[] { value.FormKey });
                        _setterProperty.SetValue(typedRecord, newFormLink);
                    }
                    else
                    {
                        Console.WriteLine($"Error: Could not find constructor for FormLinkNullable<{typeof(TTarget).Name}>");
                    }
                }
                else
                {
                    // Clear the FormLink - try to call Clear() method if available
                    var currentValue = _setterProperty.GetValue(typedRecord);
                    if (currentValue != null)
                    {
                        var clearMethod = currentValue.GetType().GetMethod("Clear");
                        if (clearMethod != null)
                        {
                            clearMethod.Invoke(currentValue, null);
                        }
                        else
                        {
                            // Fallback: set to null if Clear() is not available
                            _setterProperty.SetValue(typedRecord, null);
                        }
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
