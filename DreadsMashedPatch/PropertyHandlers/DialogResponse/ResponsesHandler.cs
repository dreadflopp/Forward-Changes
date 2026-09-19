using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;
using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.PropertyHandlers.DialogResponse
{
    public class ResponsesHandler : AbstractListPropertyHandler<IDialogResponseGetter>
    {
        private readonly bool _normalizeTrailingWhitespace;

        public ResponsesHandler(bool normalizeTrailingWhitespace = true)
        {
            _normalizeTrailingWhitespace = normalizeTrailingWhitespace;
        }

        public override string PropertyName => "Responses";
        public override ListSemantics Semantics => ListSemantics.AlignedOrdered;

        public override void SetValue(IMajorRecord record, List<IDialogResponseGetter>? value)
        {
            if (record is IDialogResponses dialogResponses)
            {
                if (dialogResponses.Responses == null)
                {
                    Console.WriteLine($"[{PropertyName}] Warning: Responses collection is null on record {record.FormKey}");
                    return;
                }

                dialogResponses.Responses.Clear();

                if (value != null)
                {
                    foreach (var response in value)
                    {
                        if (response == null)
                        {
                            Console.WriteLine($"[{PropertyName}] Warning: Skipping null response in list");
                            continue;
                        }

                        try
                        {
                            var copied = response.DeepCopy();
                            dialogResponses.Responses.Add(copied);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[{PropertyName}] Error copying response {FormatItem(response)}: {ex.Message}");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"[{PropertyName}] Record is not IDialogResponses, actual type: {record.GetType().Name}");
            }
        }

        public override List<IDialogResponseGetter>? GetValue(IMajorRecordGetter record)
        {
            if (record is IDialogResponsesGetter dialogResponses)
            {
                return dialogResponses.Responses?.ToList();
            }

            return null;
        }

        protected override bool IsItemEqual(IDialogResponseGetter? item1, IDialogResponseGetter? item2)
        {
            if (item1 == null && item2 == null) return true;
            if (item1 == null || item2 == null) return false;

            if (!AreNonTextFieldsEqual(item1, item2)) return false;

            // Trailing whitespace-only differences should not produce duplicate response entries.
            return string.Equals(
                NormalizeResponseText(item1.Text.String),
                NormalizeResponseText(item2.Text.String),
                StringComparison.Ordinal);
        }

        protected override void ProcessHandlerSpecificLogic(
            IModContext<ISkyrimMod, ISkyrimModGetter, IMajorRecord, IMajorRecordGetter> context,
            IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
            ListPropertyContext<IDialogResponseGetter> listPropertyContext,
            List<IDialogResponseGetter> recordItems,
            List<ListPropertyValueContext<IDialogResponseGetter>> currentForwardItems)
        {
            var activeForwardItems = currentForwardItems.Where(i => !i.IsRemoved).ToList();
            var consumedMatches = new HashSet<ListPropertyValueContext<IDialogResponseGetter>>();

            foreach (var recordItem in recordItems)
            {
                var matchedForwardItem = activeForwardItems.FirstOrDefault(forwardItem =>
                    !consumedMatches.Contains(forwardItem) && IsItemEqual(forwardItem.Value, recordItem));

                if (matchedForwardItem == null)
                {
                    continue;
                }

                consumedMatches.Add(matchedForwardItem);

                if (!HasWhitespaceOnlyTextDifference(matchedForwardItem.Value, recordItem))
                {
                    continue;
                }

                matchedForwardItem.Value = recordItem;
                matchedForwardItem.OwnerMod = context.ModKey.ToString();

                LogCollector.Add(
                    PropertyName,
                    $"[{PropertyName}] {context.ModKey}: Harmonizing whitespace-only text variant {FormatItem(recordItem)} (new owner: {matchedForwardItem.OwnerMod}) Success");
            }
        }

        private bool AreNonTextFieldsEqual(IDialogResponseGetter item1, IDialogResponseGetter item2)
        {
            if (item1.Emotion != item2.Emotion) return false;
            if (item1.EmotionValue != item2.EmotionValue) return false;
            if (item1.Unknown != item2.Unknown) return false;
            if (item1.ResponseNumber != item2.ResponseNumber) return false;
            if (item1.Flags != item2.Flags) return false;
            if (item1.Sound.FormKey != item2.Sound.FormKey) return false;
            if (item1.SpeakerIdleAnimation.FormKey != item2.SpeakerIdleAnimation.FormKey) return false;
            if (item1.ListenerIdleAnimation.FormKey != item2.ListenerIdleAnimation.FormKey) return false;
            if (item1.ScriptNotes != item2.ScriptNotes) return false;
            if (item1.Edits != item2.Edits) return false;
            return true;
        }

        private bool HasWhitespaceOnlyTextDifference(IDialogResponseGetter item1, IDialogResponseGetter item2)
        {
            if (!AreNonTextFieldsEqual(item1, item2)) return false;

            var text1 = item1.Text.String;
            var text2 = item2.Text.String;

            if (string.Equals(text1, text2, StringComparison.Ordinal))
            {
                return false;
            }

            return string.Equals(
                NormalizeResponseText(text1),
                NormalizeResponseText(text2),
                StringComparison.Ordinal);
        }

        private string NormalizeResponseText(string? text)
        {
            return StringComparisonHelper.NormalizeForComparison(text, _normalizeTrailingWhitespace);
        }

        protected override string FormatItem(IDialogResponseGetter? item)
        {
            if (item == null) return "null";

            try
            {
                var responseType = item.GetType().Name;
                var emotion = item.Emotion.ToString();
                var responseNumber = item.ResponseNumber;

                // Try to get meaningful text content (truncated if too long)
                var text = item.Text.String;
                var displayText = string.IsNullOrEmpty(text) ? "NoText" :
                    text.Length > 30 ? text.Substring(0, 30) + "..." : text;

                // Show the response type, emotion, number, and text
                return $"{responseType}({emotion}, #{responseNumber}, \"{displayText}\")";
            }
            catch
            {
                // Fallback to showing the response type and hash code for uniqueness
                return $"{item.GetType().Name}({item.GetHashCode():X8})";
            }
        }
    }
}

