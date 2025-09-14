using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Abstracts
{
    public abstract class AbstractConditionsHandler<TRecordGetter, TRecord> : AbstractListPropertyHandler<IConditionGetter>
        where TRecordGetter : class, IMajorRecordGetter
        where TRecord : class, IMajorRecord
    {
        public override string PropertyName => "Conditions";

        public override List<IConditionGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is TRecordGetter typedRecord)
            {
                var conditions = GetConditions(typedRecord);
                return conditions?.ToList();
            }

            return null;
        }

        public override void SetValue(IMajorRecord record, List<IConditionGetter>? value)
        {
            if (record is TRecord typedRecord)
            {
                var conditionsEnumerable = GetConditions(typedRecord);

                if (conditionsEnumerable == null)
                {
                    return;
                }

                // Convert to a list that we can modify
                var conditions = conditionsEnumerable.ToList();
                conditions.Clear();

                if (value != null)
                {
                    foreach (var condition in value)
                    {
                        if (condition == null)
                        {
                            continue;
                        }

                        try
                        {
                            var copied = condition.DeepCopy();
                            conditions.Add(copied);
                        }
                        catch
                        {
                            // Handle error silently or log if needed
                        }
                    }
                }

                // Update the record's conditions collection
                UpdateConditionsCollection(typedRecord, conditions);
            }
        }

        protected abstract void UpdateConditionsCollection(TRecord record, List<IConditionGetter> conditions);

        protected abstract IEnumerable<IConditionGetter>? GetConditions(TRecordGetter record);
        protected abstract IEnumerable<IConditionGetter>? GetConditions(TRecord record);

        protected override string FormatItem(IConditionGetter? item)
        {
            if (item == null) return "null";

            try
            {
                var conditionType = item.GetType().Name;
                var compareOp = item.CompareOperator.ToString();
                var flags = item.Flags.ToString();
                var unknown2 = item.Unknown2.ToString();

                var data = item.Data;
                var function = data.Function.ToString();
                var runOnType = data.RunOnType.ToString();
                var runOnTypeIndex = data.RunOnTypeIndex.ToString();
                var useAliases = data.UseAliases.ToString();
                var usePackageData = data.UsePackageData.ToString();
                var reference = data.Reference.FormKey.ToString();

                // Check for specific condition data types first
                if (data is IGetStageDoneConditionDataGetter stageData)
                {
                    var quest = stageData.Quest.Link.FormKey.ToString();
                    var stage = stageData.Stage.ToString();
                    var specificData = $"Quest:{quest}, Stage:{stage}";

                    // Add comparison value if it's a float condition
                    if (item is IConditionFloatGetter floatCondition)
                    {
                        var comparisonValue = floatCondition.ComparisonValue.ToString();
                        return $"{conditionType}(Op:{compareOp}, Flags:{flags}, U2:{unknown2}, Func:{function}, RunOn:{runOnType}, RunOnIdx:{runOnTypeIndex}, Aliases:{useAliases}, PkgData:{usePackageData}, {specificData}, CompVal:{comparisonValue})";
                    }
                    else
                    {
                        return $"{conditionType}(Op:{compareOp}, Flags:{flags}, U2:{unknown2}, Func:{function}, RunOn:{runOnType}, RunOnIdx:{runOnTypeIndex}, Aliases:{useAliases}, PkgData:{usePackageData}, {specificData})";
                    }
                }
                else if (item is IConditionFloatGetter floatCondition)
                {
                    var comparisonValue = floatCondition.ComparisonValue.ToString();
                    return $"{conditionType}(Op:{compareOp}, Flags:{flags}, U2:{unknown2}, Func:{function}, RunOn:{runOnType}, RunOnIdx:{runOnTypeIndex}, Aliases:{useAliases}, PkgData:{usePackageData}, Ref:{reference}, CompVal:{comparisonValue})";
                }
                else if (item is IConditionGlobalGetter globalCondition)
                {
                    var comparisonValue = globalCondition.ComparisonValue.ToString();
                    return $"{conditionType}(Op:{compareOp}, Flags:{flags}, U2:{unknown2}, Func:{function}, RunOn:{runOnType}, RunOnIdx:{runOnTypeIndex}, Aliases:{useAliases}, PkgData:{usePackageData}, Ref:{reference}, CompVal:{comparisonValue})";
                }
                else
                {
                    return $"{conditionType}(Op:{compareOp}, Flags:{flags}, U2:{unknown2}, Func:{function}, RunOn:{runOnType}, RunOnIdx:{runOnTypeIndex}, Aliases:{useAliases}, PkgData:{usePackageData}, Ref:{reference})";
                }
            }
            catch (Exception ex)
            {
                // Fallback to showing the condition type and hash code for uniqueness
                return $"{item.GetType().Name}({item.GetHashCode():X8}) - Error: {ex.Message}";
            }
        }

        protected override bool IsItemEqual(IConditionGetter? item1, IConditionGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            // Compare condition properties for more accurate equality
            if (item1.CompareOperator != item2.CompareOperator) return false;
            if (item1.Flags != item2.Flags) return false;
            if (item1.Unknown2 != item2.Unknown2) return false;

            // Compare the condition data
            if (item1.Data.Function != item2.Data.Function) return false;
            if (item1.Data.RunOnType != item2.Data.RunOnType) return false;
            if (item1.Data.RunOnTypeIndex != item2.Data.RunOnTypeIndex) return false;
            if (item1.Data.UseAliases != item2.Data.UseAliases) return false;
            if (item1.Data.UsePackageData != item2.Data.UsePackageData) return false;

            // Handle specific condition types
            if (item1.Data is IGetStageDoneConditionDataGetter stageData1 && item2.Data is IGetStageDoneConditionDataGetter stageData2)
            {
                // For GetStageDone conditions, compare quest and stage
                if (stageData1.Quest.Link.FormKey != stageData2.Quest.Link.FormKey) return false;
                if (stageData1.Stage != stageData2.Stage) return false;

                // Also compare comparison values if they're float conditions
                if (item1 is IConditionFloatGetter float1 && item2 is IConditionFloatGetter float2)
                {
                    if (float1.ComparisonValue != float2.ComparisonValue) return false;
                }
            }
            else
            {
                // For other conditions, compare reference if present
                if (item1.Data.Reference.FormKey != item2.Data.Reference.FormKey) return false;
            }

            return true;
        }
    }
}