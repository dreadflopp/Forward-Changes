using System;
using System.Collections.Generic;

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

        public static void PrintAll()
        {
            foreach (var identifier in _identifierOrder)
            {
                foreach (var line in _logsByIdentifier[identifier])
                {
                    Console.WriteLine(line);
                }
            }
        }

        public static IEnumerable<string> GetAll()
        {
            foreach (var identifier in _identifierOrder)
            {
                foreach (var line in _logsByIdentifier[identifier])
                {
                    yield return line;
                }
            }
        }

        public static void Clear()
        {
            _logsByIdentifier.Clear();
            _identifierOrder.Clear();
        }
    }
}
