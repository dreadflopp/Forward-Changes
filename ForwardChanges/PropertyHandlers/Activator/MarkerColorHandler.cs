using System.Drawing;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Activator
{
    public class MarkerColorHandler : AbstractPropertyHandler<Color?>
    {
        public override string PropertyName => "MarkerColor";

        public override void SetValue(IMajorRecord record, Color? value)
        {
            if (record is IActivator activator)
            {
                activator.MarkerColor = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IActivator for {PropertyName}");
            }
        }

        public override Color? GetValue(IMajorRecordGetter record)
        {
            if (record is IActivatorGetter activator)
            {
                return activator.MarkerColor;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IActivatorGetter for {PropertyName}");
            }
            return null;
        }

        public override bool AreValuesEqual(Color? value1, Color? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            return value1.Value.Equals(value2.Value);
        }

        public override string FormatValue(object? value)
        {
            if (value is Color color)
            {
                return $"R:{color.R} G:{color.G} B:{color.B} A:{color.A}";
            }
            return value?.ToString() ?? "null";
        }
    }
}
