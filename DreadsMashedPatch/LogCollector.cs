using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DreadsMashedPatch
{
    public static class LogCollector
    {
        private static readonly object _sync = new();
        private static readonly Dictionary<string, List<string>> _logsByIdentifier = [];
        private static readonly List<string> _identifierOrder = [];
        private static bool _currentDeepDiveRecord;
        private static bool _currentDetailedRecord;

        /// <summary>True when the current record is in deep-dive mode.</summary>
        public static bool IsDeepDiveMode
        {
            get
            {
                lock (_sync)
                {
                    return _currentDeepDiveRecord;
                }
            }
        }

        /// <summary>True when the current record should emit detailed logs.</summary>
        public static bool IsDetailedMode
        {
            get
            {
                lock (_sync)
                {
                    return _currentDetailedRecord;
                }
            }
        }

        public static void Add(string identifier, string line)
        {
            lock (_sync)
            {
                if (!ShouldEmit(identifier, line))
                {
                    return;
                }

                AddCore(identifier, line);
            }
        }

        /// <summary>Adds an actionable warning regardless of the configured verbosity.</summary>
        public static void AddWarning(string identifier, string message, Exception? exception = null)
        {
            lock (_sync)
            {
                AddCore(identifier, FormatDiagnostic("Warning", message, exception));
            }
        }

        /// <summary>Adds an error regardless of the configured verbosity.</summary>
        public static void AddError(string identifier, string message, Exception? exception = null)
        {
            lock (_sync)
            {
                AddCore(identifier, FormatDiagnostic("Error", message, exception));
            }
        }

        /// <summary>
        /// Adds a non-actionable diagnostic regardless of verbosity. Use this when an
        /// exception is recovered by a supported fallback and should be recorded in the
        /// full log without increasing the warning or error counts.
        /// </summary>
        public static void AddDiagnostic(string identifier, string message, Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            lock (_sync)
            {
                AddCore(identifier, FormatDiagnostic("Diagnostic", message, exception));
            }
        }

        /// <summary>
        /// Adds a compact final-decision audit line. These lines are allowed in
        /// ContextChanges mode without enabling verbose property-handler tracing.
        /// </summary>
        public static void AddDecisionAudit(string identifier, string line)
        {
            lock (_sync)
            {
                if (_currentDeepDiveRecord && !LoggingSettings.ShouldLogProperty(identifier, deepDiveRecord: true))
                {
                    return;
                }

                AddCore(identifier, line);
            }
        }

        private static void AddCore(string identifier, string line)
        {
            if (!_logsByIdentifier.ContainsKey(identifier))
            {
                _logsByIdentifier[identifier] = [];
                _identifierOrder.Add(identifier);
            }
            _logsByIdentifier[identifier].Add(line);
        }

        private static string FormatDiagnostic(string severity, string message, Exception? exception)
        {
            if (exception == null)
            {
                return $"[{severity}] {message}";
            }

            var detail = exception.InnerException?.Message ?? exception.Message;
            return $"[{severity}] {message} ({exception.GetType().Name}: {detail})";
        }

        public static void SetRecordLoggingContext(bool deepDiveRecord, bool detailedRecord)
        {
            lock (_sync)
            {
                _currentDeepDiveRecord = deepDiveRecord;
                _currentDetailedRecord = detailedRecord;
            }
        }

        private static bool ShouldEmit(string identifier, string line)
        {
            // Actionable diagnostics must bypass verbosity filtering. Add remains
            // backwards-compatible with existing handlers while new code should use
            // AddWarning/AddError so severity does not depend on message wording.
            if (IsDiagnostic(line, "Warning") || IsDiagnostic(line, "Error"))
            {
                return true;
            }

            // Summary mode + non-deep: suppress ALL handler log lines.
            if (!_currentDetailedRecord && !_currentDeepDiveRecord)
            {
                return false;
            }

            // Deep-dive: optionally narrow to selected properties.
            if (_currentDeepDiveRecord && !LoggingSettings.ShouldLogProperty(identifier, deepDiveRecord: true))
            {
                return false;
            }

            return true;
        }

        private static bool IsDiagnostic(string line, string severity)
        {
            return line.Contains($"[{severity}]", StringComparison.OrdinalIgnoreCase)
                || line.Contains($"{severity}:", StringComparison.OrdinalIgnoreCase)
                || line.TrimStart().StartsWith(severity, StringComparison.OrdinalIgnoreCase)
                || line.Contains($"] {severity} ", StringComparison.OrdinalIgnoreCase);
        }

        public static void PrintAll(bool stripAllControlChars = true)
        {
            var lines = GetAll(stripAllControlChars);
            foreach (var line in lines)
            {
                Console.WriteLine(line);
            }
        }

        /// <summary>Prints all pending entries and resets the collector.</summary>
        public static void PrintAllAndClear(bool stripAllControlChars = true)
        {
            string[] lines;
            lock (_sync)
            {
                lines = CreateSnapshot(stripAllControlChars);
                _logsByIdentifier.Clear();
                _identifierOrder.Clear();
            }

            foreach (var line in lines)
            {
                Console.WriteLine(line);
            }
        }

        public static IEnumerable<string> GetAll(bool stripAllControlChars = true)
        {
            lock (_sync)
            {
                return CreateSnapshot(stripAllControlChars);
            }
        }

        private static string[] CreateSnapshot(bool stripAllControlChars)
        {
            var snapshot = new List<string>();
            foreach (var identifier in _identifierOrder)
            {
                foreach (var line in _logsByIdentifier[identifier])
                {
                    var processedLine = stripAllControlChars
                        ? StripAllControlCharacters(line)
                        : SanitizeString(line);
                    snapshot.Add($"  {processedLine}");
                }
            }

            return snapshot.ToArray();
        }

        public static void Clear()
        {
            lock (_sync)
            {
                _logsByIdentifier.Clear();
                _identifierOrder.Clear();
            }
        }

        /// <summary>
        /// Removes non-printable control characters from a string while preserving printable characters.
        /// </summary>
        /// <param name="input">The input string to sanitize</param>
        /// <returns>A sanitized string with control characters removed</returns>
        private static string SanitizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var sanitized = new StringBuilder();

            foreach (char c in input)
            {
                // Keep printable characters and essential whitespace
                if (char.IsControl(c))
                {
                    // Only preserve essential whitespace characters
                    if (c == '\t' || c == '\n' || c == '\r' || c == ' ')
                    {
                        sanitized.Append(c);
                    }
                    else
                    {
                        // Replace all other control characters with hex representation
                        sanitized.Append($"<0x{(int)c:X2}>");
                    }
                }
                else
                {
                    sanitized.Append(c);
                }
            }

            return sanitized.ToString();
        }

        /// <summary>
        /// Completely strips all control characters from a string, including whitespace control chars.
        /// Use this for clipboard operations or when you need clean text.
        /// </summary>
        /// <param name="input">The input string to sanitize</param>
        /// <returns>A string with all control characters removed</returns>
        public static string StripAllControlCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var sanitized = new StringBuilder();

            foreach (char c in input)
            {
                if (!char.IsControl(c))
                {
                    sanitized.Append(c);
                }
            }

            return sanitized.ToString();
        }

        /// <summary>
        /// Gets the count of log entries for a specific identifier
        /// </summary>
        /// <param name="identifier">The identifier to count logs for</param>
        /// <returns>The number of log entries, or 0 if identifier doesn't exist</returns>
        public static int GetCount(string identifier)
        {
            lock (_sync)
            {
                return _logsByIdentifier.TryGetValue(identifier, out var logs) ? logs.Count : 0;
            }
        }

        /// <summary>
        /// Gets the total count of all log entries across all identifiers
        /// </summary>
        /// <returns>The total number of log entries</returns>
        public static int GetTotalCount()
        {
            lock (_sync)
            {
                return _logsByIdentifier.Values.Sum(logs => logs.Count);
            }
        }

        /// <summary>
        /// Checks if there are any logs collected
        /// </summary>
        /// <returns>True if there are logs, false otherwise</returns>
        public static bool HasLogs()
        {
            lock (_sync)
            {
                return _logsByIdentifier.Count > 0;
            }
        }
    }
}
