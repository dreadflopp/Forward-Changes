using ForwardChanges.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace ForwardChanges.PropertyHandlers.General;

/// <summary>
/// Handles polymorphic Condition lists without asking the generic reflection list
/// handler to construct the abstract Condition base type.
/// </summary>
public sealed class ConditionsHandler<TRecord, TRecordGetter>
    : AbstractConditionsHandler<TRecordGetter, TRecord>
    where TRecord : class, IMajorRecord
    where TRecordGetter : class, IMajorRecordGetter
{
    private readonly Func<TRecordGetter, IEnumerable<IConditionGetter>?> _getGetterConditions;
    private readonly Func<TRecord, ICollection<Condition>?> _getMutableConditions;

    public ConditionsHandler(
        Func<TRecordGetter, IEnumerable<IConditionGetter>?> getGetterConditions,
        Func<TRecord, ICollection<Condition>?> getMutableConditions)
    {
        _getGetterConditions = getGetterConditions;
        _getMutableConditions = getMutableConditions;
    }

    protected override IEnumerable<IConditionGetter>? GetConditions(TRecordGetter record)
        => _getGetterConditions(record);

    protected override IEnumerable<IConditionGetter>? GetConditions(TRecord record)
        => _getMutableConditions(record);

    protected override void UpdateConditionsCollection(TRecord record, List<IConditionGetter> conditions)
    {
        var target = _getMutableConditions(record);
        if (target == null)
        {
            return;
        }

        target.Clear();
        foreach (var condition in conditions)
        {
            target.Add(condition.DeepCopy());
        }
    }
}
