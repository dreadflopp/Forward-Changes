using DreadsMashedPatch.PropertyHandlers.Abstracts;
using DreadsMashedPatch.PropertyHandlers.General;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace DreadsMashedPatch.PropertyHandlers.Scene;

public sealed class ScenePhasesHandler : AbstractPropertyHandler<List<IScenePhaseGetter>>
{
    private static readonly ScenePhase.TranslationMask SemanticMask = new(defaultOn: true)
    {
        Unused = new ScenePhaseUnusedData.TranslationMask(defaultOn: false),
        Unused2 = new ScenePhaseUnusedData.TranslationMask(defaultOn: false)
    };
    private static readonly ConditionsHandler<IScene, ISceneGetter> ConditionComparer = new(
        scene => scene.Conditions,
        scene => scene.Conditions);

    public override string PropertyName => "Phases";

    public override List<IScenePhaseGetter>? GetValue(IMajorRecordGetter record) =>
        record is ISceneGetter scene ? scene.Phases.ToList() : null;

    public override void SetValue(IMajorRecord record, List<IScenePhaseGetter>? value)
    {
        if (record is not IScene scene || value == null) return;

        scene.Phases.Clear();
        foreach (var phase in value)
        {
            var copy = phase.DeepCopy(SemanticMask);
            copy.Unused = null;
            copy.Unused2 = null;
            scene.Phases.Add(copy);
        }
    }

    public override bool AreValuesEqual(List<IScenePhaseGetter>? left, List<IScenePhaseGetter>? right) =>
        SceneSemanticComparison.SequenceEqual(
            left,
            right,
            ArePhasesEqual);

    private static bool ArePhasesEqual(IScenePhaseGetter left, IScenePhaseGetter right) =>
        string.Equals(left.Name ?? string.Empty, right.Name ?? string.Empty, StringComparison.Ordinal)
        && (left.EditorWidth ?? 200) == (right.EditorWidth ?? 200)
        && ConditionComparer.AreValuesEqual(
            left.StartConditions.ToList(),
            right.StartConditions.ToList())
        && ConditionComparer.AreValuesEqual(
            left.CompletionConditions.ToList(),
            right.CompletionConditions.ToList());
}

public sealed class SceneActorsHandler : AbstractPropertyHandler<List<ISceneActorGetter>>
{
    public override string PropertyName => "Actors";

    public override List<ISceneActorGetter>? GetValue(IMajorRecordGetter record) =>
        record is ISceneGetter scene ? scene.Actors.ToList() : null;

    public override void SetValue(IMajorRecord record, List<ISceneActorGetter>? value)
    {
        if (record is not IScene scene || value == null) return;

        scene.Actors.Clear();
        foreach (var actor in value)
        {
            scene.Actors.Add(actor.DeepCopy());
        }
    }

    public override bool AreValuesEqual(List<ISceneActorGetter>? left, List<ISceneActorGetter>? right) =>
        SceneSemanticComparison.SequenceEqual(
            left,
            right,
            (first, second) => SceneActorMixIn.Equals(first, second));
}

public sealed class SceneActionsHandler : AbstractPropertyHandler<List<ISceneActionGetter>>
{
    private static readonly SceneAction.TranslationMask SemanticMask = new(defaultOn: true)
    {
        Unused = new ScenePhaseUnusedData.TranslationMask(defaultOn: false)
    };

    public override string PropertyName => "Actions";

    public override List<ISceneActionGetter>? GetValue(IMajorRecordGetter record) =>
        record is ISceneGetter scene ? scene.Actions.ToList() : null;

    public override void SetValue(IMajorRecord record, List<ISceneActionGetter>? value)
    {
        if (record is not IScene scene || value == null) return;

        scene.Actions.Clear();
        foreach (var action in value)
        {
            var copy = action.DeepCopy(SemanticMask);
            copy.Unused = null;
            scene.Actions.Add(copy);
        }
    }

    public override bool AreValuesEqual(List<ISceneActionGetter>? left, List<ISceneActionGetter>? right) =>
        SceneSemanticComparison.SequenceEqual(
            left,
            right,
            (first, second) => SceneActionMixIn.Equals(first, second, SemanticMask));
}

internal static class SceneSemanticComparison
{
    public static bool SequenceEqual<T>(
        IReadOnlyList<T>? left,
        IReadOnlyList<T>? right,
        Func<T, T, bool> equals)
    {
        if (left == null || right == null) return left == null && right == null;
        if (left.Count != right.Count) return false;

        for (var index = 0; index < left.Count; index++)
        {
            if (!equals(left[index], right[index])) return false;
        }

        return true;
    }
}
