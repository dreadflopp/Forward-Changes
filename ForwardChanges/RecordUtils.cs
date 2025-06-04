using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using System.Reflection;
using System.Linq;
using Noggog;

namespace ForwardChanges
{
    public static class RecordUtils
    {
        public static object? GetPropertyValue(PropertyInfo property, object record)
        {
            // Split the property path if it contains a dot
            var propertyPath = property.Name.Split('.');

            // Start with the record
            object? currentObject = record;

            // Traverse the property path
            foreach (var propName in propertyPath)
            {
                if (currentObject == null) return null;

                var prop = currentObject.GetType().GetProperty(propName);
                if (prop == null)
                {
                    Console.WriteLine($"Property {propName} not found on type {currentObject.GetType().Name}");
                    return null;
                }

                currentObject = prop.GetValue(currentObject);
            }

            return currentObject;
        }
    }
}