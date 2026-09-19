using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using Mutagen.Bethesda.Plugins;

namespace DreadsMashedPatch;

/// <summary>
/// Translates internal Mutagen getter interfaces into the four-character record
/// signatures and friendly names used by xEdit and plugin documentation.
/// </summary>
public static partial class RecordTypeCatalog
{
    private static readonly ConcurrentDictionary<Type, string> SignatureCache = new();

    public static string GetSignature(Type getterType)
    {
        ArgumentNullException.ThrowIfNull(getterType);
        return SignatureCache.GetOrAdd(getterType, ResolveSignature);
    }

    private static string ResolveSignature(Type getterType)
    {
        var concreteType = GetConcreteRecordType(getterType);
        var field = concreteType?.GetField(
            "GrupRecordType",
            BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        if (field?.GetValue(null) is not RecordType recordType)
        {
            throw new InvalidOperationException(
                $"Could not determine the xEdit record signature for {getterType.FullName}.");
        }

        return recordType.CheckedType;
    }

    public static string GetDisplayName(Type getterType)
    {
        var name = getterType.Name;
        if (name.StartsWith('I') && name.EndsWith("Getter", StringComparison.Ordinal))
        {
            name = name[1..^"Getter".Length];
        }

        name = WordBoundaryRegex().Replace(name, "$1 $2");
        return name
            .Replace("Npc", "NPC", StringComparison.Ordinal)
            .Replace(" Id", " ID", StringComparison.Ordinal);
    }

    public static string GetGroupedDisplayName(string signature, IEnumerable<Type> getterTypes)
    {
        return signature switch
        {
            "GLOB" => "Global",
            "GMST" => "Game Setting",
            _ => string.Join(" / ", getterTypes.Select(GetDisplayName).Distinct(StringComparer.Ordinal))
        };
    }

    private static Type? GetConcreteRecordType(Type getterType)
    {
        var name = getterType.Name;
        if (!name.StartsWith('I') || !name.EndsWith("Getter", StringComparison.Ordinal))
        {
            return null;
        }

        var concreteName = name[1..^"Getter".Length];
        return getterType.Assembly.GetType($"{getterType.Namespace}.{concreteName}");
    }

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex WordBoundaryRegex();
}
