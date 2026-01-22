using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Skyrim;

namespace ForwardChanges.PropertyHandlers.Quest
{
    /// <summary>
    /// Tracks ownership and removal status of script properties.
    /// Properties are identified by all their values (name, flags, and data).
    /// </summary>
    internal class ScriptPropertyOwnershipTracker
    {
        /// <summary>
        /// Maps property signature (string representation of all values) to ownership info
        /// </summary>
        private readonly Dictionary<string, PropertyOwnershipInfo> _propertyOwnership = new();

        /// <summary>
        /// Creates a signature string from a property for use as a dictionary key
        /// </summary>
        private string GetPropertySignature(IScriptPropertyGetter prop)
        {
            if (prop == null) return "null";

            // Create a signature based on all property values
            // This should match the logic in AreScriptPropertiesEqual
            var parts = new List<string> { prop.Name ?? "", prop.Flags.ToString() };

            switch (prop)
            {
                case IScriptBoolPropertyGetter boolProp:
                    parts.Add($"Bool:{boolProp.Data}");
                    break;
                case IScriptIntPropertyGetter intProp:
                    parts.Add($"Int:{intProp.Data}");
                    break;
                case IScriptFloatPropertyGetter floatProp:
                    parts.Add($"Float:{floatProp.Data}");
                    break;
                case IScriptStringPropertyGetter stringProp:
                    parts.Add($"String:{stringProp.Data ?? ""}");
                    break;
                case IScriptObjectPropertyGetter objProp:
                    parts.Add($"Object:{objProp.Object.FormKey}:{objProp.Alias}:{objProp.Unused}");
                    break;
                case IScriptBoolListPropertyGetter boolList:
                    parts.Add($"BoolList:[{string.Join(",", boolList.Data ?? new List<bool>())}]");
                    break;
                case IScriptIntListPropertyGetter intList:
                    parts.Add($"IntList:[{string.Join(",", intList.Data ?? new List<int>())}]");
                    break;
                case IScriptFloatListPropertyGetter floatList:
                    parts.Add($"FloatList:[{string.Join(",", floatList.Data ?? new List<float>())}]");
                    break;
                case IScriptStringListPropertyGetter stringList:
                    parts.Add($"StringList:[{string.Join(",", stringList.Data ?? new List<string>())}]");
                    break;
                case IScriptObjectListPropertyGetter objList:
                    if (objList.Objects != null)
                    {
                        var objStrings = objList.Objects.Select(o =>
                            o != null ? $"{o.Object.FormKey}:{o.Alias}:{o.Unused}" : "null");
                        parts.Add($"ObjectList:[{string.Join(",", objStrings)}]");
                    }
                    break;
            }

            return string.Join("|", parts);
        }

        /// <summary>
        /// Initialize ownership from original script - all original properties are owned by the original mod
        /// </summary>
        public void InitializeFromOriginal(IScriptEntryGetter? originalScript, string originalMod)
        {
            if (originalScript?.Properties == null) return;

            foreach (var prop in originalScript.Properties)
            {
                if (prop == null) continue;
                var signature = GetPropertySignature(prop);
                _propertyOwnership[signature] = new PropertyOwnershipInfo
                {
                    OwnerMod = originalMod,
                    IsRemoved = false
                };
            }
        }

        /// <summary>
        /// Initialize ownership from forward script - properties that exist in forward but not in original
        /// were added by previous mods. This should only be called if we don't have ownership info yet.
        /// </summary>
        public void InitializeFromForward(IScriptEntryGetter? forwardScript, IScriptEntryGetter? originalScript, string defaultOwnerMod)
        {
            if (forwardScript?.Properties == null) return;

            var originalProperties = originalScript?.Properties?.ToList() ?? new List<IScriptPropertyGetter>();

            foreach (var prop in forwardScript.Properties)
            {
                if (prop == null) continue;
                var signature = GetPropertySignature(prop);

                // If property is already tracked, don't overwrite - preserve existing ownership
                if (_propertyOwnership.ContainsKey(signature)) continue;

                // Check if property was in original
                bool existsInOriginal = originalProperties.Any(op =>
                    op != null && GetPropertySignature(op) == signature);

                if (!existsInOriginal)
                {
                    // Property was added by a previous mod - we don't know which one
                    // Using defaultOwnerMod is a fallback, but this is not ideal
                    // The real solution would be to persist tracker state across mod processing
                    _propertyOwnership[signature] = new PropertyOwnershipInfo
                    {
                        OwnerMod = defaultOwnerMod,
                        IsRemoved = false
                    };
                }
            }
        }

        /// <summary>
        /// Get ownership info for a property, or null if property is not tracked
        /// </summary>
        public PropertyOwnershipInfo? GetOwnership(IScriptPropertyGetter prop)
        {
            if (prop == null) return null;
            var signature = GetPropertySignature(prop);
            return _propertyOwnership.TryGetValue(signature, out var info) ? info : null;
        }

        /// <summary>
        /// Check if a property exists in the tracker (was in original or was added)
        /// </summary>
        public bool PropertyExists(IScriptPropertyGetter prop)
        {
            if (prop == null) return false;
            var signature = GetPropertySignature(prop);
            return _propertyOwnership.ContainsKey(signature);
        }

        /// <summary>
        /// Mark a property as added by a mod (takes ownership)
        /// </summary>
        public void MarkPropertyAdded(IScriptPropertyGetter prop, string ownerMod)
        {
            if (prop == null) return;
            var signature = GetPropertySignature(prop);
            var oldOwnership = _propertyOwnership.TryGetValue(signature, out var old) ? old : null;
            _propertyOwnership[signature] = new PropertyOwnershipInfo
            {
                OwnerMod = ownerMod,
                IsRemoved = false
            };

            // Debug: log ownership change
            if (oldOwnership != null && oldOwnership.OwnerMod != ownerMod)
            {
                // This shouldn't happen - ownership should only be set once
                System.Diagnostics.Debug.WriteLine($"WARNING: Property ownership changed from '{oldOwnership.OwnerMod}' to '{ownerMod}'");
            }
        }

        /// <summary>
        /// Mark a property as removed by a mod (takes ownership, marks as removed)
        /// </summary>
        public void MarkPropertyRemoved(IScriptPropertyGetter prop, string ownerMod)
        {
            if (prop == null) return;
            var signature = GetPropertySignature(prop);
            _propertyOwnership[signature] = new PropertyOwnershipInfo
            {
                OwnerMod = ownerMod,
                IsRemoved = true
            };
        }

        /// <summary>
        /// Mark a property as added back (was removed, now active again, takes ownership)
        /// </summary>
        public void MarkPropertyAddedBack(IScriptPropertyGetter prop, string ownerMod)
        {
            if (prop == null) return;
            var signature = GetPropertySignature(prop);
            _propertyOwnership[signature] = new PropertyOwnershipInfo
            {
                OwnerMod = ownerMod,
                IsRemoved = false
            };
        }

        /// <summary>
        /// Get all properties that are currently marked as removed
        /// </summary>
        public IEnumerable<string> GetRemovedPropertySignatures()
        {
            return _propertyOwnership
                .Where(kvp => kvp.Value.IsRemoved)
                .Select(kvp => kvp.Key);
        }
    }

    /// <summary>
    /// Information about property ownership and removal status
    /// </summary>
    internal class PropertyOwnershipInfo
    {
        public string OwnerMod { get; set; } = string.Empty;
        public bool IsRemoved { get; set; }
    }
}
