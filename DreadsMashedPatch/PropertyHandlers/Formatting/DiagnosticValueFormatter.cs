using System.Collections;
using System.Globalization;
using System.Reflection;
using Mutagen.Bethesda.Plugins;

namespace ForwardChanges.PropertyHandlers.Formatting;

/// <summary>
/// Produces deterministic, bounded, diagnostic-only representations of handler values.
/// This class must not be used for equality, ordering, ownership, or forwarding decisions.
/// </summary>
public static class DiagnosticValueFormatter
{
    private const int MaxDepth = 4;
    private const int MaxItems = 12;
    private const int MaxProperties = 12;
    private const int MaxStringLength = 500;

    public static string Format(object? value)
    {
        try
        {
            return new FormatterState().FormatValue(value, depth: 0, nested: false);
        }
        catch (Exception ex)
        {
            // Logging must never be able to interrupt patch decisions.
            return $"<format-error:{ex.GetType().Name}>";
        }
    }

    private sealed class FormatterState
    {
        private readonly HashSet<object> _activeReferences = new(ReferenceEqualityComparer.Instance);

        public string FormatValue(object? value, int depth, bool nested)
        {
            if (value == null)
            {
                return "null";
            }

            if (value is string text)
            {
                var bounded = text.Length <= MaxStringLength
                    ? text
                    : $"{text[..MaxStringLength]}…(+{text.Length - MaxStringLength} chars)";
                return nested ? $"\"{Escape(bounded)}\"" : bounded;
            }

            if (value is char character)
            {
                return $"'{Escape(character.ToString())}'";
            }

            if (value is bool boolean)
            {
                return boolean ? "true" : "false";
            }

            var type = value.GetType();
            if (type.IsEnum)
            {
                return value.ToString() ?? Convert.ToUInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
            }

            if (IsInvariantScalar(type) && value is IFormattable formattable)
            {
                return formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty;
            }

            if (value is FormKey or ModKey or Guid or DateTime or DateTimeOffset or TimeSpan)
            {
                return value.ToString() ?? string.Empty;
            }

            if (TryFormatFormLink(value, out var formLink))
            {
                return formLink;
            }

            if (TryFormatAssetLink(value, out var assetLink))
            {
                return assetLink;
            }

            if (TryFormatBinary(value, out var binary))
            {
                return binary;
            }

            if (depth >= MaxDepth)
            {
                return $"{FriendlyTypeName(type)}{{…}}";
            }

            var trackReference = !type.IsValueType;
            if (trackReference && !_activeReferences.Add(value))
            {
                return $"{FriendlyTypeName(type)}{{<cycle>}}";
            }

            try
            {
                if (value is IDictionary dictionary)
                {
                    return FormatDictionary(dictionary, depth);
                }

                if (value is IEnumerable enumerable)
                {
                    return FormatEnumerable(enumerable, depth);
                }

                return FormatObject(value, type, depth);
            }
            finally
            {
                if (trackReference)
                {
                    _activeReferences.Remove(value);
                }
            }
        }

        private string FormatDictionary(IDictionary dictionary, int depth)
        {
            var entries = new List<string>();
            var truncated = false;

            try
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (entries.Count == MaxItems)
                    {
                        truncated = true;
                        break;
                    }

                    entries.Add($"{FormatValue(entry.Key, depth + 1, nested: true)}: {FormatValue(entry.Value, depth + 1, nested: true)}");
                }
            }
            catch (Exception ex)
            {
                entries.Add($"<enumeration-error:{ex.GetType().Name}>");
            }

            if (truncated)
            {
                entries.Add("…");
            }

            return $"{{{string.Join(", ", entries)}}}";
        }

        private string FormatEnumerable(IEnumerable enumerable, int depth)
        {
            var items = new List<string>();
            var truncated = false;

            try
            {
                foreach (var item in enumerable)
                {
                    if (items.Count == MaxItems)
                    {
                        truncated = true;
                        break;
                    }

                    items.Add(FormatValue(item, depth + 1, nested: true));
                }
            }
            catch (Exception ex)
            {
                items.Add($"<enumeration-error:{ex.GetType().Name}>");
            }

            if (truncated)
            {
                items.Add("…");
            }

            return $"[{string.Join(", ", items)}]";
        }

        private string FormatObject(object value, Type type, int depth)
        {
            var properties = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.CanRead && property.GetIndexParameters().Length == 0)
                .OrderBy(property => property.Name, StringComparer.Ordinal)
                .Take(MaxProperties + 1)
                .ToArray();

            var formattedProperties = new List<string>(Math.Min(properties.Length, MaxProperties));
            foreach (var property in properties.Take(MaxProperties))
            {
                try
                {
                    formattedProperties.Add($"{property.Name}={FormatValue(property.GetValue(value), depth + 1, nested: true)}");
                }
                catch (Exception ex)
                {
                    var actualException = ex is TargetInvocationException { InnerException: not null }
                        ? ex.InnerException
                        : ex;
                    formattedProperties.Add($"{property.Name}=<getter-error:{actualException!.GetType().Name}>");
                }
            }

            if (properties.Length > MaxProperties)
            {
                formattedProperties.Add("…");
            }

            return $"{FriendlyTypeName(type)}{{{string.Join(", ", formattedProperties)}}}";
        }

        private static bool TryFormatFormLink(object value, out string formatted)
        {
            var type = value.GetType();
            var isFormLink = type.Name.StartsWith("FormLink`", StringComparison.Ordinal)
                || type.Name.StartsWith("FormLinkNullable`", StringComparison.Ordinal)
                || type.GetInterfaces().Any(interfaceType =>
                    interfaceType.Name.StartsWith("IFormLinkGetter`", StringComparison.Ordinal)
                    || interfaceType.Name.StartsWith("IFormLinkNullableGetter`", StringComparison.Ordinal));
            if (!isFormLink)
            {
                formatted = string.Empty;
                return false;
            }

            try
            {
                var formKeyProperty = type.GetProperty("FormKey", BindingFlags.Public | BindingFlags.Instance);
                var formKey = formKeyProperty?.GetIndexParameters().Length == 0
                    ? formKeyProperty.GetValue(value)
                    : null;
                formatted = $"FormLink({formKey ?? "null"})";
            }
            catch (Exception ex)
            {
                formatted = $"FormLink(<getter-error:{ex.GetType().Name}>)";
            }

            return true;
        }

        private static bool TryFormatAssetLink(object value, out string formatted)
        {
            var type = value.GetType();
            var isAssetLink = type.Name.StartsWith("AssetLink`", StringComparison.Ordinal)
                || type.GetInterfaces().Any(interfaceType =>
                    interfaceType.Name.StartsWith("IAssetLinkGetter`", StringComparison.Ordinal));
            if (!isAssetLink)
            {
                formatted = string.Empty;
                return false;
            }

            try
            {
                var pathProperty = type.GetProperty("GivenPath", BindingFlags.Public | BindingFlags.Instance)
                    ?? type.GetProperty("DataRelativePath", BindingFlags.Public | BindingFlags.Instance);
                var path = pathProperty?.GetIndexParameters().Length == 0
                    ? pathProperty.GetValue(value)
                    : null;
                formatted = $"AssetLink({path ?? "null"})";
            }
            catch (Exception ex)
            {
                formatted = $"AssetLink(<getter-error:{ex.GetType().Name}>)";
            }

            return true;
        }

        private static bool TryFormatBinary(object value, out string formatted)
        {
            switch (value)
            {
                case byte[] bytes:
                    formatted = FormatBytes(bytes);
                    return true;
                case ReadOnlyMemory<byte> readOnlyMemory:
                    formatted = FormatBytes(readOnlyMemory.Span);
                    return true;
                case Memory<byte> memory:
                    formatted = FormatBytes(memory.Span);
                    return true;
            }

            var type = value.GetType();
            if (!type.Name.Contains("MemorySlice", StringComparison.Ordinal))
            {
                formatted = string.Empty;
                return false;
            }

            var length = TryReadIntegerProperty(value, type, "Length")
                ?? TryReadIntegerProperty(value, type, "Count");
            formatted = length.HasValue
                ? $"{FriendlyTypeName(type)}(Length={length.Value})"
                : $"{FriendlyTypeName(type)}(binary)";
            return true;
        }

        private static int? TryReadIntegerProperty(object value, Type type, string propertyName)
        {
            try
            {
                var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property?.GetIndexParameters().Length == 0)
                {
                    return Convert.ToInt32(property.GetValue(value), CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                // A length is useful but optional diagnostic information.
            }

            return null;
        }

        private static string FormatBytes(ReadOnlySpan<byte> bytes)
        {
            const int previewLength = 16;
            var shown = bytes[..Math.Min(bytes.Length, previewLength)];
            var hex = Convert.ToHexString(shown);
            return bytes.Length > previewLength
                ? $"bytes[{bytes.Length}]({hex}…)"
                : $"bytes[{bytes.Length}]({hex})";
        }

        private static bool IsInvariantScalar(Type type)
        {
            return type.IsPrimitive
                || type == typeof(decimal)
                || type == typeof(Half)
                || type == typeof(Int128)
                || type == typeof(UInt128);
        }

        private static string FriendlyTypeName(Type type)
        {
            var name = type.Name;
            var tick = name.IndexOf('`');
            return tick >= 0 ? name[..tick] : name;
        }

        private static string Escape(string value)
        {
            return value
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("\r", "\\r", StringComparison.Ordinal)
                .Replace("\n", "\\n", StringComparison.Ordinal)
                .Replace("\t", "\\t", StringComparison.Ordinal)
                .Replace("\"", "\\\"", StringComparison.Ordinal);
        }
    }
}
