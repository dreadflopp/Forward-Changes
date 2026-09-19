using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Formatting;
using ForwardChanges.PropertyHandlers.General;

namespace ForwardChanges.PropertyHandlers.Abstracts
{
    public abstract class AbstractConditionsHandler<TRecordGetter, TRecord> : AbstractListPropertyHandler<IConditionGetter>
        where TRecordGetter : class, IMajorRecordGetter
        where TRecord : class, IMajorRecord
    {
        private static readonly ConditionFloat.TranslationMask FloatValueComparisonMask = new(false)
        {
            ComparisonValue = true
        };

        private static readonly ConditionGlobal.TranslationMask GlobalValueComparisonMask = new(false)
        {
            ComparisonValue = true
        };

        private static readonly bool ConditionsCanBeNull =
            ReflectionPropertyResolver.Find(typeof(TRecordGetter), "Conditions") is { } property
            && ReflectionPropertyResolver.IsNullable(property);

        public override string PropertyName => "Conditions";
        protected override bool CanBeNull => ConditionsCanBeNull;
        // Conditions need xEdit-like list alignment behavior so inserts can land at the
        // beginning/middle/end according to each mod's declared order, not only appended.
        public override ListSemantics Semantics => ListSemantics.AlignedOrdered;

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
                if (value == null)
                {
                    SetConditionsNull(typedRecord);
                    return;
                }

                var conditions = new List<IConditionGetter>();

                foreach (var condition in value)
                {
                    if (condition == null)
                    {
                        continue;
                    }

                    try
                    {
                        conditions.Add(condition.DeepCopy());
                    }
                    catch
                    {
                        // Preserve the existing best-effort behavior for malformed conditions.
                    }
                }

                UpdateConditionsCollection(typedRecord, conditions);
            }
        }

        protected virtual void SetConditionsNull(TRecord record)
        {
            // Non-nullable Conditions collections represent absence as an empty list.
            UpdateConditionsCollection(record, []);
        }

        protected abstract void UpdateConditionsCollection(TRecord record, List<IConditionGetter> conditions);

        protected abstract IEnumerable<IConditionGetter>? GetConditions(TRecordGetter record);
        protected abstract IEnumerable<IConditionGetter>? GetConditions(TRecord record);

        protected override string FormatItem(IConditionGetter? item)
        {
            if (item == null) return "null";

            try
            {
                var comparisonValue = item switch
                {
                    IConditionFloatGetter floatCondition => DiagnosticValueFormatter.Format(floatCondition.ComparisonValue),
                    IConditionGlobalGetter globalCondition => DiagnosticValueFormatter.Format(globalCondition.ComparisonValue),
                    _ => "n/a"
                };
                var parameters = (IConditionParametersGetter)item.Data;

                return $"{item.GetType().Name}(Op:{item.CompareOperator}, Flags:{item.Flags}, "
                    + $"CompVal:{comparisonValue}, Data:{item.Data.GetType().Name}{{Func:{item.Data.Function}, "
                    + $"RunOn:{item.Data.RunOnType}, RunOnIdx:{item.Data.RunOnTypeIndex}, "
                    + $"Aliases:{item.Data.UseAliases}, PkgData:{item.Data.UsePackageData}, "
                    + $"Ref:{item.Data.Reference.FormKey}, "
                    + $"Param1:{DiagnosticValueFormatter.Format(parameters.Parameter1)}, "
                    + $"Param2:{DiagnosticValueFormatter.Format(parameters.Parameter2)}}})";
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

            if (item1.CompareOperator != item2.CompareOperator || item1.Flags != item2.Flags)
            {
                return false;
            }

            if (!AreConditionDataEqual(item1.Data, item2.Data))
            {
                return false;
            }

            // Compare only the generated float/global value. The rest was compared above so
            // CTDA padding and generated unused parameter storage cannot affect identity.
            return (item1, item2) switch
            {
                (IConditionFloatGetter float1, IConditionFloatGetter float2) =>
                    ConditionFloatMixIn.Equals(float1, float2, FloatValueComparisonMask),
                (IConditionGlobalGetter global1, IConditionGlobalGetter global2) =>
                    ConditionGlobalMixIn.Equals(global1, global2, GlobalValueComparisonMask),
                _ => false
            };
        }

        /// <summary>
        /// Mirrors Skyrim xEdit's condition alignment sort key: CTDA Function,
        /// Parameter #1, Parameter #2, followed by CIS1 and CIS2. Operator,
        /// comparison value, flags, run-on data, reference, and parameter #3 are
        /// deliberately excluded because they are values within the same row.
        /// </summary>
        protected override bool IsAlignmentEqual(
            IConditionGetter? item1,
            IConditionGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;
            if (item1.Data.Function != item2.Data.Function) return false;

            var parameters1 = (IConditionParametersGetter)item1.Data;
            var parameters2 = (IConditionParametersGetter)item2.Data;
            return AreConditionParametersEqual(parameters1.Parameter1, parameters2.Parameter1)
                && AreConditionParametersEqual(parameters1.Parameter2, parameters2.Parameter2)
                && AreAlignmentStringsEqual(
                    parameters1.StringParameter1,
                    parameters2.StringParameter1)
                && AreAlignmentStringsEqual(
                    parameters1.StringParameter2,
                    parameters2.StringParameter2);
        }

        private static bool AreAlignmentStringsEqual(string? value1, string? value2)
            => string.Equals(
                value1 ?? string.Empty,
                value2 ?? string.Empty,
                StringComparison.Ordinal);

        private static bool AreConditionDataEqual(IConditionDataGetter data1, IConditionDataGetter data2)
        {
            if (data1.Function != data2.Function
                || data1.RunOnType != data2.RunOnType
                || !data1.Reference.FormKey.Equals(data2.Reference.FormKey)
                || data1.RunOnTypeIndex != data2.RunOnTypeIndex
                || data1.UseAliases != data2.UseAliases
                || data1.UsePackageData != data2.UsePackageData)
            {
                return false;
            }

            var parameters1 = (IConditionParametersGetter)data1;
            var parameters2 = (IConditionParametersGetter)data2;
            return AreConditionParametersEqual(parameters1.Parameter1, parameters2.Parameter1)
                && AreConditionParametersEqual(parameters1.Parameter2, parameters2.Parameter2);
        }

        private static bool AreConditionParametersEqual(object? parameter1, object? parameter2)
        {
            if (ReferenceEquals(parameter1, parameter2)) return true;
            if (parameter1 == null || parameter2 == null) return false;

            if (parameter1 is IFormLinkGetter formLink1 && parameter2 is IFormLinkGetter formLink2)
            {
                return formLink1.FormKey.Equals(formLink2.FormKey);
            }

            if (TryGetFormLinkOrIndexValue(parameter1, out var formLinkOrIndex1)
                && TryGetFormLinkOrIndexValue(parameter2, out var formLinkOrIndex2))
            {
                return formLinkOrIndex1.UsesLink == formLinkOrIndex2.UsesLink
                    && (formLinkOrIndex1.UsesLink
                        ? formLinkOrIndex1.FormKey.Equals(formLinkOrIndex2.FormKey)
                        : formLinkOrIndex1.Index == formLinkOrIndex2.Index);
            }

            return parameter1.Equals(parameter2);
        }

        private static bool TryGetFormLinkOrIndexValue(
            object parameter,
            out (bool UsesLink, FormKey FormKey, uint? Index) value)
        {
            var type = parameter.GetType();
            var formLinkOrIndexInterface = type.GetInterfaces().FirstOrDefault(interfaceType =>
                interfaceType.IsGenericType
                && interfaceType.GetGenericTypeDefinition() == typeof(IFormLinkOrIndexGetter<>));
            if (formLinkOrIndexInterface == null)
            {
                value = default;
                return false;
            }

            var usesLink = (bool)formLinkOrIndexInterface
                .GetMethod(nameof(IFormLinkOrIndexGetter<IMajorRecordGetter>.UsesLink))!
                .Invoke(parameter, null)!;
            var index = (uint?)formLinkOrIndexInterface
                .GetProperty(nameof(IFormLinkOrIndexGetter<IMajorRecordGetter>.Index))!
                .GetValue(parameter);
            var link = formLinkOrIndexInterface
                .GetProperty(nameof(IFormLinkOrIndexGetter<IMajorRecordGetter>.Link))!
                .GetValue(parameter) as IFormLinkGetter;

            value = (usesLink, link?.FormKey ?? FormKey.Null, index);
            return true;
        }
    }
}
