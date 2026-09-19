using DreadsMashedPatch.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace DreadsMashedPatch.PropertyHandlers.Scene;

public sealed class SceneVirtualMachineAdapterPresenceHandler : AbstractPropertyHandler<bool>
{
    public override string PropertyName => "VirtualMachineAdapter.Presence";

    public override bool GetValue(IMajorRecordGetter record) =>
        record is ISceneGetter scene && scene.VirtualMachineAdapter != null;

    public override void SetValue(IMajorRecord record, bool value)
    {
        if (record is not IScene scene) return;
        scene.VirtualMachineAdapter = value
            ? scene.VirtualMachineAdapter ?? new SceneAdapter()
            : null;
    }

    public override string FormatValue(object? value) => value is true ? "Present" : "Absent";
}

public sealed class SceneScriptsHandler : AbstractScriptListPropertyHandler
{
    public override string PropertyName => "VirtualMachineAdapter.Scripts";

    public override List<IScriptEntryGetter>? GetValue(IMajorRecordGetter record) =>
        record is ISceneGetter scene ? scene.VirtualMachineAdapter?.Scripts.ToList() : null;

    public override void SetValue(IMajorRecord record, List<IScriptEntryGetter>? value)
    {
        if (record is not IScene scene || value == null) return;

        scene.VirtualMachineAdapter ??= new SceneAdapter();
        var destinationScripts = scene.VirtualMachineAdapter.Scripts.ToList();
        scene.VirtualMachineAdapter.Scripts.Clear();
        foreach (var script in value)
        {
            var destination = destinationScripts.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, script.Name, StringComparison.Ordinal));
            scene.VirtualMachineAdapter.Scripts.Add(PapyrusUnusedDataPolicy.CopyScript(script, destination));
        }
    }
}

public sealed class SceneScriptFragmentsHandler : AbstractPropertyHandler<ISceneScriptFragmentsGetter>
{
    public override string PropertyName => "VirtualMachineAdapter.ScriptFragments";

    public override ISceneScriptFragmentsGetter? GetValue(IMajorRecordGetter record) =>
        record is ISceneGetter scene ? scene.VirtualMachineAdapter?.ScriptFragments : null;

    public override void SetValue(IMajorRecord record, ISceneScriptFragmentsGetter? value)
    {
        if (record is not IScene scene) return;

        if (value == null)
        {
            if (scene.VirtualMachineAdapter != null)
            {
                scene.VirtualMachineAdapter.ScriptFragments = null;
            }
            return;
        }

        scene.VirtualMachineAdapter ??= new SceneAdapter();
        scene.VirtualMachineAdapter.ScriptFragments = value.DeepCopy();
    }

    public override bool AreValuesEqual(
        ISceneScriptFragmentsGetter? left,
        ISceneScriptFragmentsGetter? right)
    {
        if (left == null || right == null) return left == null && right == null;
        return SceneScriptFragmentsMixIn.Equals(left, right);
    }
}
