using Mutagen.Bethesda.Skyrim;

namespace DreadsMashedPatch.PropertyHandlers.Abstracts;

/// <summary>
/// Keeps generated Papyrus object-property padding outside the semantic conflict
/// surface. Existing destination values are retained when semantic script data is
/// copied; new object properties receive the format default instead of inheriting
/// opaque bytes from another plugin.
/// </summary>
internal static class PapyrusUnusedDataPolicy
{
    public static ScriptEntry CopyScript(
        IScriptEntryGetter source,
        IScriptEntryGetter? destination)
    {
        var copy = new ScriptEntry();
        copy.DeepCopyIn(source);
        PreserveScriptUnusedValues(copy, destination);
        return copy;
    }

    public static void PreserveScriptUnusedValues(
        ScriptEntry copy,
        IScriptEntryGetter? destination)
    {
        foreach (var property in copy.Properties)
        {
            var destinationProperty = destination?.Properties.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, property.Name, StringComparison.Ordinal) &&
                IsSameObjectPropertyKind(property, candidate));

            switch (property)
            {
                case ScriptObjectProperty objectProperty:
                    objectProperty.Unused = destinationProperty is IScriptObjectPropertyGetter destinationObject
                        ? destinationObject.Unused
                        : (ushort)0;
                    break;

                case ScriptObjectListProperty objectListProperty:
                    var destinationObjects = destinationProperty is IScriptObjectListPropertyGetter destinationList
                        ? destinationList.Objects
                        : null;
                    for (var index = 0; index < objectListProperty.Objects.Count; index++)
                    {
                        objectListProperty.Objects[index].Unused =
                            destinationObjects != null && index < destinationObjects.Count
                                ? destinationObjects[index].Unused
                                : (ushort)0;
                    }
                    break;
            }
        }
    }

    public static void PreserveObjectUnusedValue(
        ScriptObjectProperty copy,
        IScriptObjectPropertyGetter? destination)
    {
        copy.Unused = destination?.Unused ?? (ushort)0;
    }

    private static bool IsSameObjectPropertyKind(
        IScriptPropertyGetter left,
        IScriptPropertyGetter right) =>
        left is IScriptObjectPropertyGetter && right is IScriptObjectPropertyGetter ||
        left is IScriptObjectListPropertyGetter && right is IScriptObjectListPropertyGetter;
}
