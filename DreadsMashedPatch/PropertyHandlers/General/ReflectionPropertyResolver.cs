using System.Reflection;

namespace DreadsMashedPatch.PropertyHandlers.General;

internal static class ReflectionPropertyResolver
{
    public static PropertyInfo? Find(Type type, string propertyName, string? nextSegment = null)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        return InspectionTypes(type)
            .SelectMany(inspectionType => inspectionType.GetProperties(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(property =>
                property.Name == propertyName
                && property.GetIndexParameters().Length == 0)
            .OrderByDescending(property => nextSegment != null && HasProperty(property.PropertyType, nextSegment))
            .ThenByDescending(property => property.DeclaringType == type)
            .ThenBy(property => property.DeclaringType?.FullName, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    public static bool HasProperty(Type type, string propertyName) =>
        Find(type, propertyName) != null;

    public static bool IsNullable(PropertyInfo property)
    {
        if (Nullable.GetUnderlyingType(property.PropertyType) != null)
        {
            return true;
        }

        if (property.PropertyType.IsValueType)
        {
            return false;
        }

        return new NullabilityInfoContext().Create(property).ReadState == NullabilityState.Nullable;
    }

    private static IEnumerable<Type> InspectionTypes(Type type)
    {
        yield return type;

        if (!type.IsInterface)
        {
            yield break;
        }

        foreach (var interfaceType in type.GetInterfaces().OrderBy(item => item.FullName, StringComparer.Ordinal))
        {
            yield return interfaceType;
        }
    }
}
