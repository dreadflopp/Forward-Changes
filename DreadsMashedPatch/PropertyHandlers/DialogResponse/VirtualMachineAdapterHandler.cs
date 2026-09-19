using System;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogResponse
{
    public class VirtualMachineAdapterHandler : AbstractPropertyHandler<IDialogResponsesAdapterGetter?>
    {
        public override string PropertyName => "VirtualMachineAdapter";

        public override void SetValue(IMajorRecord record, IDialogResponsesAdapterGetter? value)
        {
            if (record is IDialogResponses dialogResponseRecord)
            {
                if (value == null)
                {
                    dialogResponseRecord.VirtualMachineAdapter = null;
                    return;
                }

                // Create a new DialogResponsesAdapter instance
                var newAdapter = new DialogResponsesAdapter();

                // Copy Scripts if they exist
                if (value.Scripts != null && value.Scripts.Any())
                {
                    foreach (var script in value.Scripts)
                    {
                        if (script == null) continue;

                        var newScript = new ScriptEntry
                        {
                            Name = script.Name,
                            Flags = script.Flags
                        };

                        // Copy Script Properties
                        if (script.Properties != null)
                        {
                            foreach (var prop in script.Properties)
                            {
                                if (prop == null) continue;

                                var newProp = new ScriptProperty
                                {
                                    Name = prop.Name,
                                    Flags = prop.Flags
                                };

                                newScript.Properties.Add(newProp);
                            }
                        }

                        newAdapter.Scripts.Add(newScript);
                    }
                }

                dialogResponseRecord.VirtualMachineAdapter = newAdapter;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogResponses for {PropertyName}");
            }
        }

        public override IDialogResponsesAdapterGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IDialogResponsesGetter dialogResponseRecord)
            {
                return dialogResponseRecord.VirtualMachineAdapter;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogResponsesGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(IDialogResponsesAdapterGetter? value1, IDialogResponsesAdapterGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare Scripts
            if (value1.Scripts == null && value2.Scripts == null) { }
            else if (value1.Scripts == null || value2.Scripts == null) return false;
            else
            {
                if (value1.Scripts.Count != value2.Scripts.Count) return false;

                for (int i = 0; i < value1.Scripts.Count; i++)
                {
                    var script1 = value1.Scripts[i];
                    var script2 = value2.Scripts[i];

                    if (script1?.Name != script2?.Name) return false;
                    if (script1?.Flags != script2?.Flags) return false;

                    // Compare Script Properties
                    if (script1?.Properties == null && script2?.Properties == null) { }
                    else if (script1?.Properties == null || script2?.Properties == null) return false;
                    else
                    {
                        if (script1.Properties.Count != script2.Properties.Count) return false;

                        for (int j = 0; j < script1.Properties.Count; j++)
                        {
                            var prop1 = script1.Properties[j];
                            var prop2 = script2.Properties[j];

                            if (prop1?.Name != prop2?.Name) return false;
                            if (prop1?.Flags != prop2?.Flags) return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}