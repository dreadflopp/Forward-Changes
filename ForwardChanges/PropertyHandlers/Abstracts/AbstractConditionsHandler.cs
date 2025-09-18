using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Abstracts
{
    public abstract class AbstractConditionsHandler<TRecordGetter, TRecord> : AbstractListPropertyHandler<IConditionGetter>
        where TRecordGetter : class, IMajorRecordGetter
        where TRecord : class, IMajorRecord
    {
        public override string PropertyName => "Conditions";
        protected override ListOrdering Ordering => ListOrdering.PreserveModOrder; // Conditions always preserve mod order

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
                var dataType = data.GetType().Name;

                // Get the reference information using a generic approach that works for all condition data types
                var (reference, referenceIsNull) = GetConditionReference(data);

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
                        return $"{conditionType}(Op:{compareOp}, Flags:{flags}, U2:{unknown2}, Func:{function}, RunOn:{runOnType}, RunOnIdx:{runOnTypeIndex}, Aliases:{useAliases}, PkgData:{usePackageData}, {specificData}, CompVal:{comparisonValue}, DataType:{dataType})";
                    }
                    else
                    {
                        return $"{conditionType}(Op:{compareOp}, Flags:{flags}, U2:{unknown2}, Func:{function}, RunOn:{runOnType}, RunOnIdx:{runOnTypeIndex}, Aliases:{useAliases}, PkgData:{usePackageData}, {specificData})";
                    }
                }
                else if (item is IConditionFloatGetter floatCondition)
                {
                    var comparisonValue = floatCondition.ComparisonValue.ToString();
                    return $"{conditionType}(Op:{compareOp}, Flags:{flags}, U2:{unknown2}, Func:{function}, RunOn:{runOnType}, RunOnIdx:{runOnTypeIndex}, Aliases:{useAliases}, PkgData:{usePackageData}, Ref:{reference}(Null:{referenceIsNull}), CompVal:{comparisonValue}, DataType:{dataType})";
                }
                else if (item is IConditionGlobalGetter globalCondition)
                {
                    var comparisonValue = globalCondition.ComparisonValue.ToString();
                    return $"{conditionType}(Op:{compareOp}, Flags:{flags}, U2:{unknown2}, Func:{function}, RunOn:{runOnType}, RunOnIdx:{runOnTypeIndex}, Aliases:{useAliases}, PkgData:{usePackageData}, Ref:{reference}(Null:{referenceIsNull}), CompVal:{comparisonValue}, DataType:{dataType})";
                }
                else
                {
                    return $"{conditionType}(Op:{compareOp}, Flags:{flags}, U2:{unknown2}, Func:{function}, RunOn:{runOnType}, RunOnIdx:{runOnTypeIndex}, Aliases:{useAliases}, PkgData:{usePackageData}, Ref:{reference}(Null:{referenceIsNull}), DataType:{dataType})";
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

            // Handle specific condition types that need special comparison logic
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
                // For all other condition types, use the generic reference comparison
                if (!CompareConditionReferences(item1.Data, item2.Data)) return false;
            }

            return true;
        }

        /// <summary>
        /// Generic method to extract reference information from any condition data type using reflection.
        /// This handles all 100+ condition data types dynamically without hardcoding each one.
        /// </summary>
        private (string reference, bool isNull) GetConditionReference(IConditionDataGetter data)
        {
            try
            {
                // First, try the generic Reference property from the base ConditionData class
                var genericReference = data.Reference;
                if (genericReference != null && !genericReference.FormKey.IsNull)
                {
                    return (genericReference.FormKey.ToString(), false);
                }

                // If the generic Reference is null, try to find reference properties using reflection
                var dataType = data.GetType();
                var properties = dataType.GetProperties();

                // Look for properties that might contain reference information
                // Common patterns: Keyword, Race, Quest, Location, etc.
                foreach (var prop in properties)
                {
                    var propName = prop.Name.ToLowerInvariant();

                    // Skip base class properties and common non-reference properties
                    if (propName == "reference" || propName == "runontype" || propName == "runontypeindex" ||
                        propName == "usealiases" || propName == "usepackagedata" || propName == "function" ||
                        propName.Contains("unused") || propName.Contains("string") || propName.Contains("int"))
                        continue;

                    try
                    {
                        var propValue = prop.GetValue(data);
                        if (propValue == null) continue;

                        // Try to extract FormKey from the property value
                        var formKey = ExtractFormKeyFromProperty(propValue);
                        if (formKey.HasValue && !formKey.Value.IsNull)
                        {
                            return (formKey.Value.ToString(), false);
                        }
                    }
                    catch
                    {
                        // Continue to next property if this one fails
                        continue;
                    }
                }

                // If no reference found, return null
                return ("Null", true);
            }
            catch
            {
                return ("Null", true);
            }
        }

        /// <summary>
        /// Extracts FormKey from a property value, handling various FormLink types.
        /// </summary>
        private FormKey? ExtractFormKeyFromProperty(object propValue)
        {
            try
            {
                // Handle IFormLinkOrIndex types (like Keyword, Race, etc.)
                if (propValue.GetType().GetInterface("IFormLinkOrIndexGetter`1") != null)
                {
                    // Try to get the Link property and then FormKey
                    var linkProperty = propValue.GetType().GetProperty("Link");
                    if (linkProperty != null)
                    {
                        var link = linkProperty.GetValue(propValue);
                        if (link != null)
                        {
                            var formKeyProperty = link.GetType().GetProperty("FormKey");
                            if (formKeyProperty != null)
                            {
                                var formKeyValue = formKeyProperty.GetValue(link);
                                if (formKeyValue is FormKey formKey)
                                {
                                    return formKey;
                                }
                            }
                        }
                    }
                }

                // Handle direct FormLink types
                var directFormKeyProperty = propValue.GetType().GetProperty("FormKey");
                if (directFormKeyProperty != null)
                {
                    var formKeyValue = directFormKeyProperty.GetValue(propValue);
                    if (formKeyValue is FormKey formKey)
                    {
                        return formKey;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Generic method to compare reference information between two condition data objects.
        /// </summary>
        private bool CompareConditionReferences(IConditionDataGetter data1, IConditionDataGetter data2)
        {
            try
            {
                // First, try the generic Reference property
                if (data1.Reference.FormKey != data2.Reference.FormKey)
                    return false;

                // If generic references are equal and not null, we're done
                if (!data1.Reference.FormKey.IsNull)
                    return true;

                // If generic references are both null, try to find and compare specific reference properties
                var (ref1, isNull1) = GetConditionReference(data1);
                var (ref2, isNull2) = GetConditionReference(data2);

                return ref1 == ref2 && isNull1 == isNull2;
            }
            catch
            {
                return false;
            }
        }
    }
}