using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ForwardChanges
{
    public static class LogCollector
    {
        private static readonly Dictionary<string, List<string>> _logsByIdentifier = [];
        private static readonly List<string> _identifierOrder = [];

        public static void Add(string identifier, string line)
        {
            if (!_logsByIdentifier.ContainsKey(identifier))
            {
                _logsByIdentifier[identifier] = [];
                _identifierOrder.Add(identifier);
            }
            _logsByIdentifier[identifier].Add(line);
        }

        public static void PrintAll(bool stripAllControlChars = true)
        {
            foreach (var line in GetAll(stripAllControlChars))
            {
                Console.WriteLine(line);
            }
        }

        public static IEnumerable<string> GetAll(bool stripAllControlChars = true)
        {
            foreach (var identifier in _identifierOrder)
            {
                //yield return $"[{identifier}]";
                foreach (var line in _logsByIdentifier[identifier])
                {
                    var processedLine = stripAllControlChars
                        ? StripAllControlCharacters(line)
                        : SanitizeString(line);
                    yield return $"  {processedLine}";
                }
                //yield return string.Empty;
            }
        }

        public static void Clear()
        {
            _logsByIdentifier.Clear();
            _identifierOrder.Clear();
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
            return _logsByIdentifier.ContainsKey(identifier) ? _logsByIdentifier[identifier].Count : 0;
        }

        /// <summary>
        /// Gets the total count of all log entries across all identifiers
        /// </summary>
        /// <returns>The total number of log entries</returns>
        public static int GetTotalCount()
        {
            return _logsByIdentifier.Values.Sum(logs => logs.Count);
        }

        /// <summary>
        /// Checks if there are any logs collected
        /// </summary>
        /// <returns>True if there are logs, false otherwise</returns>
        public static bool HasLogs()
        {
            return _logsByIdentifier.Count > 0;
        }
    }
}
