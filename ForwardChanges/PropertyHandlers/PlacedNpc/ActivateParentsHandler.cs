using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class ActivateParentsHandler : AbstractPropertyHandler<IActivateParentsGetter?>
    {
        public override string PropertyName => "ActivateParents";

        public override void SetValue(IMajorRecord record, IActivateParentsGetter? value)
        {
            if (record is IPlacedNpc placedNpcRecord)
            {
                if (value != null)
                {
                    // Create new ActivateParents and copy properties
                    var newActivateParents = new ActivateParents
                    {
                        Flags = value.Flags
                    };

                    // Copy Parents collection
                    if (value.Parents != null)
                    {
                        foreach (var parent in value.Parents)
                        {
                            var newParent = new ActivateParent
                            {
                                Reference = new FormLink<IPlacedObjectGetter>(parent.Reference.FormKey),
                                Delay = parent.Delay
                            };
                            newActivateParents.Parents.Add(newParent);
                        }
                    }

                    placedNpcRecord.ActivateParents = newActivateParents;
                }
                else
                {
                    placedNpcRecord.ActivateParents = null;
                }
            }
        }

        public override IActivateParentsGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedNpcGetter placedNpcRecord)
            {
                return placedNpcRecord.ActivateParents;
            }
            Console.WriteLine($"Error: Record is not a PlacedNpc for {PropertyName}");
            return null;
        }

        public override bool AreValuesEqual(IActivateParentsGetter? value1, IActivateParentsGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare Flags
            if (value1.Flags != value2.Flags) return false;

            // Compare Parents collection
            if (value1.Parents?.Count != value2.Parents?.Count) return false;

            if (value1.Parents != null && value2.Parents != null)
            {
                for (int i = 0; i < value1.Parents.Count; i++)
                {
                    var parent1 = value1.Parents[i];
                    var parent2 = value2.Parents[i];

                    if (!parent1.Reference.FormKey.Equals(parent2.Reference.FormKey)) return false;
                    if (parent1.Delay != parent2.Delay) return false;
                }
            }

            return true;
        }

        public override string FormatValue(object? value)
        {
            if (value is not IActivateParentsGetter activateParents)
            {
                return value?.ToString() ?? "null";
            }

            var parentCount = activateParents.Parents?.Count ?? 0;
            return $"Flags: {activateParents.Flags}, Parents: {parentCount} items";
        }
    }
}