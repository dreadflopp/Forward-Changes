using System;
using System.Collections.Generic;
using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.Contexts
{
    /// <summary>
    /// Represents a context for a list property value.
    /// </summary>
    /// <typeparam name="T">The type of the property value</typeparam>
    public class ListPropertyValueContext<T>(T item, string ownerMod) : IPropertyValueContext<T>
    {
        public T Value { get; set; } = item;
        public string OwnerMod { get; set; } = ownerMod;
        public bool IsRemoved { get; set; } = false;
        public List<string> ItemsBefore { get; set; } = [];
        public List<string> ItemsAfter { get; set; } = [];
    }
}