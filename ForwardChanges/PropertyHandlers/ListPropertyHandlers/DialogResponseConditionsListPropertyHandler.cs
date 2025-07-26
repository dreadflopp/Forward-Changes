using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.ListPropertyHandlers.Abstracts;
using ForwardChanges.Contexts;
using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.PropertyHandlers.ListPropertyHandlers
{
    public class DialogResponseConditionsListPropertyHandler : AbstractListPropertyHandler<IConditionGetter>
    {
        public override string PropertyName => "Conditions";

        public override void SetValue(IMajorRecord record, List<IConditionGetter>? value)
        {
            if (record is IDialogResponses dialogResponses)
            {
                if (dialogResponses.Conditions == null)
                {
                    Console.WriteLine($"[{PropertyName}] Warning: Conditions collection is null on record {record.FormKey}");
                    return;
                }

                dialogResponses.Conditions.Clear();

                if (value != null)
                {
                    foreach (var cond in value)
                    {
                        if (cond == null)
                        {
                            Console.WriteLine($"[{PropertyName}] Warning: Skipping null condition in list");
                            continue;
                        }

                        try
                        {
                            var copied = cond.DeepCopy();
                            dialogResponses.Conditions.Add(copied);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[{PropertyName}] Error copying condition {FormatItem(cond)}: {ex.Message}");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"[{PropertyName}] Record is not IDialogResponses, actual type: {record.GetType().Name}");
            }
        }

        public override List<IConditionGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IDialogResponsesGetter dialogResponses)
            {
                return dialogResponses.Conditions?.ToList();
            }

            return null;
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

            // Compare reference if present
            if (item1.Data.Reference.FormKey != item2.Data.Reference.FormKey) return false;

            return true;
        }

        protected override string FormatItem(IConditionGetter? item)
        {
            if (item == null) return "null";

            try
            {
                var conditionType = item.GetType().Name;
                var function = item.Data.Function.ToString();
                var compareOp = item.CompareOperator.ToString();

                // Try to get more specific information based on condition type
                if (item is IConditionFloatGetter floatCondition)
                {
                    return $"{conditionType}({function}, {compareOp}, {floatCondition.ComparisonValue})";
                }
                else if (item is IConditionGlobalGetter globalCondition)
                {
                    return $"{conditionType}({function}, {compareOp}, {globalCondition.ComparisonValue})";
                }
                else
                {
                    // For other condition types, show the function and operator
                    return $"{conditionType}({function}, {compareOp})";
                }
            }
            catch
            {
                // Fallback to showing the condition type and hash code for uniqueness
                return $"{item.GetType().Name}({item.GetHashCode():X8})";
            }
        }
    }
}