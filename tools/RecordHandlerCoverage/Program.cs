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
    Console.Error.WriteLine("Usage: RecordHandlerCoverage <repository-root> <markdown-output> [json-output]");
    return 2;
}

var repositoryRoot = Path.GetFullPath(args[0]);
var markdownOutput = Path.GetFullPath(args[1]);
var jsonOutput = args.Length >= 3 ? Path.GetFullPath(args[2]) : null;
var handlersDirectory = Path.Combine(repositoryRoot, "ForwardChanges", "RecordHandlers");
var propertyHandlersDirectory = Path.Combine(repositoryRoot, "ForwardChanges", "PropertyHandlers");

if (!Directory.Exists(handlersDirectory))
{
    Console.Error.WriteLine($"Record handler directory not found: {handlersDirectory}");
    return 2;
}

var getterAssembly = typeof(IArmorGetter).Assembly;
var reports = new List<HandlerReport>();
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
        .Select(property => AuditProperty(property, registrations, propertyHandlerSources))
        .ToList();

    reports.Add(new HandlerReport(Path.GetFileName(sourcePath), getterName, registrations, propertyReports, null));
}

Directory.CreateDirectory(Path.GetDirectoryName(markdownOutput)!);
File.WriteAllText(markdownOutput, BuildMarkdown(reports), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

if (jsonOutput is not null)
{
    Directory.CreateDirectory(Path.GetDirectoryName(jsonOutput)!);
    File.WriteAllText(jsonOutput, JsonSerializer.Serialize(reports, new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    }), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

var missingCount = reports.Sum(report => report.Properties.Count(property => property.Status == CoverageStatus.Missing));
var reviewCount = reports.Sum(report => report.Properties.Count(property => property.Status == CoverageStatus.Review));
Console.WriteLine($"Audited {reports.Count} handlers: {missingCount} missing candidates, {reviewCount} review candidates.");
Console.WriteLine($"Markdown report: {markdownOutput}");
if (jsonOutput is not null)
{
    Console.WriteLine($"JSON report: {jsonOutput}");
}

return reports.Any(report => report.Error is not null) ? 1 : 0;

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
        .Select(match => new HandlerRegistration(
            match.Groups["key"].Value,
            match.Groups["handler"].Value,
            source.AsSpan(0, match.Index).Count('\n') + 1))
        .ToList();
}

static PropertyReport AuditProperty(
    PropertyInfo property,
    List<HandlerRegistration> registrations,
    IReadOnlyDictionary<string, string> propertyHandlerSources)
{
    var exact = registrations
        .Where(registration => string.Equals(registration.Key, property.Name, StringComparison.OrdinalIgnoreCase))
        .ToList();
    if (exact.Count > 0)
    {
        return new PropertyReport(property.Name, FriendlyType(property.PropertyType), CoverageStatus.Covered,
            exact.Select(registration => registration.Key).ToList(), []);
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
    var status = candidates.Count > 0 ? CoverageStatus.Review : CoverageStatus.Missing;

    return new PropertyReport(
        property.Name,
        FriendlyType(property.PropertyType),
        status,
        candidates.Select(registration => registration.Key).ToList(),
        ExpandLeaves(property.PropertyType, property.Name, depth: 0, visited: []));
}

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
    builder.AppendLine("This is a static registration audit. `Missing` means no matching handler registration was found. `Review` means a nested or specialized mapping may exist and needs inspection. `Covered` confirms registration, not equality/copy correctness or whether the property is a separately serialized xEdit field.");
    builder.AppendLine();
    builder.AppendLine("Direct record properties plus the project-standard inherited `EditorID`, `MajorRecordFlagsRaw`, and `SkyrimMajorRecordFlags` fields are compared. Identity/version metadata is excluded.");
    builder.AppendLine();
    builder.AppendLine("## Summary");
    builder.AppendLine();
    builder.AppendLine("| Handler | Getter | Covered | Review | Missing | Error |");
    builder.AppendLine("|---|---|---:|---:|---:|---|");
    foreach (var report in reports)
    {
        builder.AppendLine($"| {Escape(report.Handler)} | {Escape(report.Getter ?? "-")} | {Count(report, CoverageStatus.Covered)} | {Count(report, CoverageStatus.Review)} | {Count(report, CoverageStatus.Missing)} | {Escape(report.Error ?? string.Empty)} |");
    }

    foreach (var report in reports.Where(report => report.Error is not null || report.Properties.Any(property => property.Status != CoverageStatus.Covered)))
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

        builder.AppendLine("| Property | Type | Status | Possible handler keys | Nested leaves |");
        builder.AppendLine("|---|---|---|---|---|");
        foreach (var property in report.Properties.Where(property => property.Status != CoverageStatus.Covered))
        {
            builder.AppendLine($"| `{Escape(property.Name)}` | `{Escape(property.Type)}` | {property.Status} | {Escape(string.Join(", ", property.HandlerKeys))} | {Escape(string.Join("<br>", property.NestedLeaves))} |");
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
    Review,
    Missing
}

public sealed record HandlerRegistration(string Key, string HandlerType, int SourceLine);
public sealed record PropertyReport(string Name, string Type, CoverageStatus Status, List<string> HandlerKeys, List<string> NestedLeaves);
public sealed record HandlerReport(string Handler, string? Getter, List<HandlerRegistration> Registrations, List<PropertyReport> Properties, string? Error);
