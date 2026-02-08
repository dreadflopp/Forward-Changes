using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.General
{
    /// <summary>
    /// A generic property handler that uses reflection to access VirtualMachineAdapter properties.
    /// This handler treats VirtualMachineAdapter as a list of Scripts, which is what should be patched.
    /// </summary>
    /// <typeparam name="TRecord">The record type that contains the property (e.g., IPlacedObject)</typeparam>
    /// <typeparam name="TRecordGetter">The getter record type (e.g., IPlacedObjectGetter)</typeparam>
    public class SimpleReflectionVirtualMachineAdapterHandler<TRecord, TRecordGetter> : AbstractVirtualMachineAdapterHandler<TRecordGetter, TRecord, IVirtualMachineAdapterGetter, VirtualMachineAdapter>
        where TRecord : class, IMajorRecord
        where TRecordGetter : class, IMajorRecordGetter
    {
        private readonly PropertyInfo? _getterProperty;
        private readonly PropertyInfo? _setterProperty;

        public SimpleReflectionVirtualMachineAdapterHandler()
        {
            // Find the VirtualMachineAdapter property on the getter interface
            _getterProperty = typeof(TRecordGetter).GetProperty("VirtualMachineAdapter",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            // Find the VirtualMachineAdapter property on the setter interface
            _setterProperty = typeof(TRecord).GetProperty("VirtualMachineAdapter",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            if (_getterProperty == null)
            {
                throw new ArgumentException(
                    $"Property 'VirtualMachineAdapter' not found on {typeof(TRecordGetter).Name}");
            }
        }

        protected override IVirtualMachineAdapterGetter? GetVirtualMachineAdapter(TRecordGetter record)
        {
            try
            {
                var value = _getterProperty?.GetValue(record);
                return value as IVirtualMachineAdapterGetter;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting VirtualMachineAdapter property via reflection: {ex.Message}");
                return null;
            }
        }

        protected override void SetVirtualMachineAdapter(TRecord record, VirtualMachineAdapter? value)
        {
            try
            {
                if (_setterProperty == null)
                {
                    Console.WriteLine($"Error: Property 'VirtualMachineAdapter' is read-only or not found on {typeof(TRecord).Name}");
                    return;
                }

                _setterProperty.SetValue(record, value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting VirtualMachineAdapter property via reflection: {ex.Message}");
            }
        }

        protected override VirtualMachineAdapter CreateNewAdapter()
        {
            return new VirtualMachineAdapter();
        }
    }
}
