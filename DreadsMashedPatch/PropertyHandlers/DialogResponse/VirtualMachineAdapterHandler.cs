using DreadsMashedPatch.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace DreadsMashedPatch.PropertyHandlers.DialogResponse;

/// <summary>
/// DIAL VMAD is forwarded atomically because its script fragments and scripts
/// describe one compiled fragment unit. Generated copies preserve the complete
/// adapter while the shared Papyrus comparer keeps opaque Unused bytes outside
/// the semantic conflict surface.
/// </summary>
public sealed class VirtualMachineAdapterHandler
    : AbstractPropertyHandler<IDialogResponsesAdapterGetter?>
{
    public override string PropertyName => "VirtualMachineAdapter";

    public override void SetValue(IMajorRecord record, IDialogResponsesAdapterGetter? value)
    {
        if (record is not IDialogResponses dialogResponse)
        {
            Console.WriteLine($"Error: Record does not implement IDialogResponses for {PropertyName}");
            return;
        }

        if (value == null)
        {
            dialogResponse.VirtualMachineAdapter = null;
            return;
        }

        var destinationScripts = dialogResponse.VirtualMachineAdapter?.Scripts;
        var copy = value.DeepCopy();
        foreach (var script in copy.Scripts)
        {
            var destination = destinationScripts?.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, script.Name, StringComparison.Ordinal));
            PapyrusUnusedDataPolicy.PreserveScriptUnusedValues(script, destination);
        }

        dialogResponse.VirtualMachineAdapter = copy;
    }

    public override IDialogResponsesAdapterGetter? GetValue(IMajorRecordGetter record)
    {
        if (record is IDialogResponsesGetter dialogResponse)
        {
            return dialogResponse.VirtualMachineAdapter;
        }

        Console.WriteLine($"Error: Record does not implement IDialogResponsesGetter for {PropertyName}");
        return null;
    }

    public override bool AreValuesEqual(
        IDialogResponsesAdapterGetter? value1,
        IDialogResponsesAdapterGetter? value2)
    {
        if (value1 == null || value2 == null)
        {
            return value1 == null && value2 == null;
        }

        if (value1.Version != value2.Version || value1.ObjectFormat != value2.ObjectFormat)
        {
            return false;
        }

        if (!AbstractScriptListPropertyHandler.AreScriptCollectionsEqual(
                value1.Scripts,
                value2.Scripts))
        {
            return false;
        }

        if (value1.ScriptFragments == null || value2.ScriptFragments == null)
        {
            return value1.ScriptFragments == null && value2.ScriptFragments == null;
        }

        return ScriptFragmentsMixIn.Equals(value1.ScriptFragments, value2.ScriptFragments);
    }
}
