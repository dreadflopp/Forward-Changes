using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

if (args.Length < 2)
{
    Console.Error.WriteLine("Usage: RecordHandlerCoverage <repository-root> <markdown-output> [json-output] [overrides-file]");
    return 2;
}

var repositoryRoot = Path.GetFullPath(args[0]);
var markdownOutput = Path.GetFullPath(args[1]);
var jsonOutput = args.Length >= 3 ? Path.GetFullPath(args[2]) : null;
var overridesPath = args.Length >= 4 ? Path.GetFullPath(args[3]) : null;
var handlersDirectory = Path.Combine(repositoryRoot, "DreadsMashedPatch", "RecordHandlers");
var propertyHandlersDirectory = Path.Combine(repositoryRoot, "DreadsMashedPatch", "PropertyHandlers");

if (!Directory.Exists(handlersDirectory))
{
    Console.Error.WriteLine($"Record handler directory not found: {handlersDirectory}");
    return 2;
}

var getterAssembly = typeof(IArmorGetter).Assembly;
var reports = new List<HandlerReport>();
var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNameCaseInsensitive = true,
    Converters = { new JsonStringEnumConverter() }
};
var coverageOverrides = LoadOverrides(overridesPath, jsonOptions);
var usedOverrideKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
var auditedInheritedPropertyNames = new HashSet<string>(StringComparer.Ordinal)
{
    "EditorID",
    "MajorRecordFlagsRaw",
    "SkyrimMajorRecordFlags"
};
var propertyHandlerSources = Directory
    .EnumerateFiles(propertyHandlersDirectory, "*.cs", SearchOption.AllDirectories)
    .GroupBy(Path.GetFileNameWithoutExtension, StringComparer.Ordinal)
    .ToDictionary(group => group.Key!, group => string.Join(Environment.NewLine, group.Select(File.ReadAllText)), StringComparer.Ordinal);

foreach (var sourcePath in Directory.EnumerateFiles(handlersDirectory, "*RecordHandler.cs").OrderBy(Path.GetFileName))
{
    var source = File.ReadAllText(sourcePath);
    var getterName = FindGetterName(source);
    var registrations = FindRegistrations(source);

    if (getterName is null)
    {
        reports.Add(new HandlerReport(Path.GetFileName(sourcePath), null, registrations, [],
            "Could not infer the Mutagen getter interface."));
        continue;
    }

    var getterType = getterAssembly.GetType($"Mutagen.Bethesda.Skyrim.{getterName}");
    if (getterType is null)
    {
        reports.Add(new HandlerReport(Path.GetFileName(sourcePath), getterName, registrations, [],
            "Getter interface was not found in the referenced Mutagen assembly."));
        continue;
    }

    var declaredProperties = getterType
        .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .Where(property => property.GetIndexParameters().Length == 0);
    var inheritedProjectProperties = getterType
        .GetInterfaces()
        .SelectMany(interfaceType => interfaceType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        .Where(property => auditedInheritedPropertyNames.Contains(property.Name));
    var propertyReports = declaredProperties
        .Concat(inheritedProjectProperties)
        .DistinctBy(property => property.Name)
        .OrderBy(property => property.Name)
        .Select(property => AuditProperty(
            Path.GetFileName(sourcePath),
            property,
            registrations,
            propertyHandlerSources,
            coverageOverrides,
            usedOverrideKeys))
        .ToList();

    reports.Add(new HandlerReport(Path.GetFileName(sourcePath), getterName, registrations, propertyReports, null));
}

Directory.CreateDirectory(Path.GetDirectoryName(markdownOutput)!);
File.WriteAllText(markdownOutput, BuildMarkdown(reports), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

if (jsonOutput is not null)
{
    Directory.CreateDirectory(Path.GetDirectoryName(jsonOutput)!);
    File.WriteAllText(jsonOutput, JsonSerializer.Serialize(reports, jsonOptions), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

var unusedOverrides = coverageOverrides.Keys.Except(usedOverrideKeys, StringComparer.OrdinalIgnoreCase).Order().ToList();
foreach (var unusedOverride in unusedOverrides)
{
    Console.Error.WriteLine($"Unused coverage override: {unusedOverride.Replace('\0', '.')}");
}

var missingCount = reports.Sum(report => report.Properties.Count(property => property.Status == CoverageStatus.MissingCandidate));
var partialCount = reports.Sum(report => report.Properties.Count(property => property.Status == CoverageStatus.Partial));
Console.WriteLine($"Audited {reports.Count} handlers: {missingCount} missing candidates, {partialCount} partial candidates.");
Console.WriteLine($"Markdown report: {markdownOutput}");
if (jsonOutput is not null)
{
    Console.WriteLine($"JSON report: {jsonOutput}");
}

return reports.Any(report => report.Error is not null) || unusedOverrides.Count > 0 ? 1 : 0;

static Dictionary<string, CoverageOverride> LoadOverrides(string? overridesPath, JsonSerializerOptions jsonOptions)
{
    if (overridesPath is null || !File.Exists(overridesPath))
    {
        return new Dictionary<string, CoverageOverride>(StringComparer.OrdinalIgnoreCase);
    }

    var entries = JsonSerializer.Deserialize<List<CoverageOverride>>(File.ReadAllText(overridesPath), jsonOptions) ?? [];
    var result = new Dictionary<string, CoverageOverride>(StringComparer.OrdinalIgnoreCase);
    foreach (var entry in entries)
    {
        var key = OverrideKey(entry.Handler, entry.Property);
        if (!result.TryAdd(key, entry))
        {
            throw new InvalidOperationException($"Duplicate coverage override for {entry.Handler}.{entry.Property}.");
        }

        if (entry.Status is CoverageStatus.Covered or CoverageStatus.AggregateCovered or CoverageStatus.Partial or CoverageStatus.MissingCandidate)
        {
            throw new InvalidOperationException($"Override {entry.Handler}.{entry.Property} must describe an intentional classification, not inferred coverage status {entry.Status}.");
        }

        if (string.IsNullOrWhiteSpace(entry.Reason))
        {
            throw new InvalidOperationException($"Override {entry.Handler}.{entry.Property} requires a reason.");
        }
    }

    return result;
}

static string? FindGetterName(string source)
{
    var guard = Regex.Match(source, @"winningContext\.Record\s+is\s+not\s+(?<type>I[A-Za-z0-9_]+Getter)");
    if (guard.Success)
    {
        return guard.Groups["type"].Value;
    }

    var link = Regex.Match(source, @"\.ToLink<(?<type>I[A-Za-z0-9_]+Getter)>");
    return link.Success ? link.Groups["type"].Value : null;
}

static List<HandlerRegistration> FindRegistrations(string source)
{
    const string pattern = "\\{\\s*\"(?<key>[^\"]+)\"\\s*,\\s*new\\s+(?<handler>[A-Za-z0-9_.]+)(?:<[^;{}]+?>)?\\s*\\(";
    return Regex.Matches(source, pattern, RegexOptions.Multiline)
        .Where(match =>
        {
            var lineStart = source.LastIndexOf('\n', Math.Max(0, match.Index - 1)) + 1;
            var prefix = source[lineStart..match.Index];
            return !prefix.Contains("//", StringComparison.Ordinal);
        })
        .Select(match => new HandlerRegistration(
            match.Groups["key"].Value,
            match.Groups["handler"].Value,
            source.AsSpan(0, match.Index).Count('\n') + 1))
        .ToList();
}

static PropertyReport AuditProperty(
    string handlerFile,
    PropertyInfo property,
    List<HandlerRegistration> registrations,
    IReadOnlyDictionary<string, string> propertyHandlerSources,
    IReadOnlyDictionary<string, CoverageOverride> coverageOverrides,
    ISet<string> usedOverrideKeys)
{
    var overrideKey = OverrideKey(handlerFile, property.Name);
    if (coverageOverrides.TryGetValue(overrideKey, out var coverageOverride))
    {
        usedOverrideKeys.Add(overrideKey);
        return new PropertyReport(
            property.Name,
            FriendlyType(property.PropertyType),
            coverageOverride.Status,
            [],
            ExpandLeaves(property.PropertyType, property.Name, depth: 0, visited: []),
            coverageOverride.Reason);
    }

    var exact = registrations
        .Where(registration => string.Equals(registration.Key, property.Name, StringComparison.OrdinalIgnoreCase))
        .ToList();
    if (exact.Count > 0)
    {
        return new PropertyReport(property.Name, FriendlyType(property.PropertyType), CoverageStatus.Covered,
            exact.Select(registration => registration.Key).ToList(), [], null);
    }

    var dotted = registrations
        .Where(registration => registration.Key.StartsWith(property.Name + ".", StringComparison.OrdinalIgnoreCase))
        .ToList();
    var normalizedAliases = registrations
        .Where(registration =>
        {
            var normalizedKey = Normalize(registration.Key);
            var normalizedProperty = Normalize(property.Name);
            return normalizedKey.Length > normalizedProperty.Length
                && normalizedKey.StartsWith(normalizedProperty, StringComparison.OrdinalIgnoreCase);
        })
        .ToList();

    var implementationCandidates = registrations.Where(registration =>
        propertyHandlerSources.TryGetValue(registration.HandlerType, out var implementationSource)
        && Regex.IsMatch(implementationSource, $@"\.{Regex.Escape(property.Name)}\b"));
    var candidates = dotted
        .Concat(normalizedAliases)
        .Concat(implementationCandidates)
        .DistinctBy(registration => registration.Key)
        .ToList();
    var nestedLeaves = ExpandLeaves(property.PropertyType, property.Name, depth: 0, visited: []);
    var classifiedLeaves = nestedLeaves
        .Select(leaf => (Leaf: leaf, Key: OverrideKey(handlerFile, leaf)))
        .Where(item => coverageOverrides.ContainsKey(item.Key))
        .ToList();
    foreach (var classifiedLeaf in classifiedLeaves)
    {
        usedOverrideKeys.Add(classifiedLeaf.Key);
    }
    var allNestedLeavesCovered = nestedLeaves.Count > 0
        && nestedLeaves.All(leaf =>
            registrations.Any(registration => string.Equals(registration.Key, leaf, StringComparison.OrdinalIgnoreCase))
            || coverageOverrides.ContainsKey(OverrideKey(handlerFile, leaf)));
    var status = candidates.Count == 0
        ? CoverageStatus.MissingCandidate
        : IsLeaf(Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType) || allNestedLeavesCovered
            ? CoverageStatus.AggregateCovered
            : CoverageStatus.Partial;

    return new PropertyReport(
        property.Name,
        FriendlyType(property.PropertyType),
        status,
        candidates.Select(registration => registration.Key).ToList(),
        nestedLeaves,
        classifiedLeaves.Count == 0
            ? null
            : string.Join(" ",
                classifiedLeaves.Select(item => $"{item.Leaf}: {coverageOverrides[item.Key].Reason}")));
}

static string OverrideKey(string handler, string property) => $"{handler}\0{property}";

static List<string> ExpandLeaves(Type inputType, string path, int depth, HashSet<Type> visited)
{
    var type = Nullable.GetUnderlyingType(inputType) ?? inputType;
    if (IsLeaf(type) || depth >= 4 || !visited.Add(type))
    {
        return [path];
    }

    var inspectionTypes = type.IsInterface ? type.GetInterfaces().Append(type) : [type];
    var properties = inspectionTypes
        .SelectMany(inspectionType => inspectionType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        .Where(property => property.GetIndexParameters().Length == 0)
        .Where(property => property.Name is not "CommonInstance"
            and not "CommonSetterInstance"
            and not "CommonSetterTranslationInstance"
            and not "Registration"
            and not "BinaryWriteTranslator")
        .DistinctBy(property => property.Name)
        .OrderBy(property => property.Name)
        .ToList();

    if (properties.Count == 0)
    {
        return [path];
    }

    var leaves = new List<string>();
    foreach (var property in properties)
    {
        leaves.AddRange(ExpandLeaves(property.PropertyType, $"{path}.{property.Name}", depth + 1, new HashSet<Type>(visited)));
    }

    return leaves;
}

static bool IsLeaf(Type type)
{
    if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal)
        || type == typeof(FormKey) || typeof(IMajorRecordGetter).IsAssignableFrom(type))
    {
        return true;
    }

    if (type.IsArray || type.Namespace?.StartsWith("System", StringComparison.Ordinal) == true)
    {
        return true;
    }

    if (type.IsGenericType)
    {
        var definitionName = type.GetGenericTypeDefinition().Name;
        if (definitionName.Contains("FormLink", StringComparison.Ordinal)
            || definitionName.Contains("List", StringComparison.Ordinal)
            || definitionName.Contains("Enumerable", StringComparison.Ordinal)
            || definitionName.Contains("Collection", StringComparison.Ordinal))
        {
            return true;
        }
    }

    var name = type.Name;
    return name.Contains("FormLink", StringComparison.Ordinal)
        || name.Contains("AssetLink", StringComparison.Ordinal)
        || name.Contains("MemorySlice", StringComparison.Ordinal)
        || name.Contains("TranslatedString", StringComparison.Ordinal);
}

static string Normalize(string value) => Regex.Replace(value, "[^A-Za-z0-9]", string.Empty);

static string FriendlyType(Type type)
{
    if (!type.IsGenericType)
    {
        return type.Name;
    }

    var name = type.Name[..type.Name.IndexOf('`')];
    return $"{name}<{string.Join(", ", type.GetGenericArguments().Select(FriendlyType))}>";
}

static string BuildMarkdown(List<HandlerReport> reports)
{
    var builder = new StringBuilder();
    builder.AppendLine("# Record handler property coverage audit");
    builder.AppendLine();
    builder.AppendLine($"Generated: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}");
    builder.AppendLine();
    builder.AppendLine("This is a static registration audit. `Covered` is an exact registration, `AggregateCovered` is inferred from a specialized handler implementation, `Partial` indicates nested/split handling, and `MissingCandidate` has no detected handler. Reviewed aliases and non-property surfaces are classified through the tracked overrides file.");
    builder.AppendLine();
    builder.AppendLine("Direct record properties plus the project-standard inherited `EditorID`, `MajorRecordFlagsRaw`, and `SkyrimMajorRecordFlags` fields are compared. Identity/version metadata is excluded.");
    builder.AppendLine();
    builder.AppendLine("## Summary");
    builder.AppendLine();
    builder.AppendLine("| Handler | Getter | Covered | Aggregate | Partial | Missing | Classified exclusion/alias | Error |");
    builder.AppendLine("|---|---|---:|---:|---:|---:|---:|---|");
    foreach (var report in reports)
    {
        var classified = report.Properties.Count(property => property.Status is CoverageStatus.RuntimeOrNavigation or CoverageStatus.SerializationState or CoverageStatus.AliasOrDuplicate or CoverageStatus.IntentionalExclusion);
        builder.AppendLine($"| {Escape(report.Handler)} | {Escape(report.Getter ?? "-")} | {Count(report, CoverageStatus.Covered)} | {Count(report, CoverageStatus.AggregateCovered)} | {Count(report, CoverageStatus.Partial)} | {Count(report, CoverageStatus.MissingCandidate)} | {classified} | {Escape(report.Error ?? string.Empty)} |");
    }

    foreach (var report in reports.Where(report => report.Error is not null || report.Properties.Any(property => property.Status is not CoverageStatus.Covered and not CoverageStatus.AggregateCovered)))
    {
        builder.AppendLine();
        builder.AppendLine($"## {report.Handler}");
        builder.AppendLine();
        builder.AppendLine($"Getter: `{report.Getter ?? "unknown"}`");
        builder.AppendLine();
        if (report.Error is not null)
        {
            builder.AppendLine($"Error: {report.Error}");
            continue;
        }

        builder.AppendLine("| Property | Type | Status | Possible handler keys | Nested leaves | Reason |");
        builder.AppendLine("|---|---|---|---|---|---|");
        foreach (var property in report.Properties.Where(property => property.Status is not CoverageStatus.Covered and not CoverageStatus.AggregateCovered))
        {
            builder.AppendLine($"| `{Escape(property.Name)}` | `{Escape(property.Type)}` | {property.Status} | {Escape(string.Join(", ", property.HandlerKeys))} | {Escape(string.Join("<br>", property.NestedLeaves))} | {Escape(property.Reason ?? string.Empty)} |");
        }
    }

    builder.AppendLine();
    builder.AppendLine("## Registered keys by handler");
    builder.AppendLine();
    foreach (var report in reports)
    {
        builder.AppendLine($"- **{report.Handler}**: {string.Join(", ", report.Registrations.Select(registration => $"`{registration.Key}`"))}");
    }

    return builder.ToString();
}

static int Count(HandlerReport report, CoverageStatus status) => report.Properties.Count(property => property.Status == status);

static string Escape(string value) => value.Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");

public enum CoverageStatus
{
    Covered,
    AggregateCovered,
    Partial,
    MissingCandidate,
    RuntimeOrNavigation,
    SerializationState,
    AliasOrDuplicate,
    IntentionalExclusion
}

public sealed record HandlerRegistration(string Key, string HandlerType, int SourceLine);
public sealed record CoverageOverride(string Handler, string Property, CoverageStatus Status, string Reason);
public sealed record PropertyReport(string Name, string Type, CoverageStatus Status, List<string> HandlerKeys, List<string> NestedLeaves, string? Reason);
public sealed record HandlerReport(string Handler, string? Getter, List<HandlerRegistration> Registrations, List<PropertyReport> Properties, string? Error);
