using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins.Cache;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.Contexts;
using ForwardChanges.Contexts.Interfaces;

namespace ForwardChanges.PropertyHandlers.DialogResponse
{
    public class ResponsesHandler : AbstractListPropertyHandler<IDialogResponseGetter>
    {
        public override string PropertyName => "Responses";

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

            // Compare dialog response properties for more accurate equality
            if (item1.Emotion != item2.Emotion) return false;
            if (item1.EmotionValue != item2.EmotionValue) return false;
            if (item1.Unknown != item2.Unknown) return false;
            if (item1.ResponseNumber != item2.ResponseNumber) return false;
            if (item1.Flags != item2.Flags) return false;

            // Compare sound reference
            if (item1.Sound.FormKey != item2.Sound.FormKey) return false;

            // Compare idle animations
            if (item1.SpeakerIdleAnimation.FormKey != item2.SpeakerIdleAnimation.FormKey) return false;
            if (item1.ListenerIdleAnimation.FormKey != item2.ListenerIdleAnimation.FormKey) return false;

            // Compare text content
            if (item1.Text.String != item2.Text.String) return false;

            // Compare script notes and edits
            if (item1.ScriptNotes != item2.ScriptNotes) return false;
            if (item1.Edits != item2.Edits) return false;

            return true;
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

