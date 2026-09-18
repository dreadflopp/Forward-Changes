using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Formatting;
using ForwardChanges.PropertyHandlers.General;
using ForwardChanges.PropertyHandlers.Npc;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace ForwardChanges.Tests;

public sealed class DiagnosticValueFormatterTests
{
    [Fact]
    public void MovementTypeNamesFormatsStringsWithoutInspectingStringIndexer()
    {
        var handler = new SimpleReflectionListPropertyHandler<string, IRace, IRaceGetter>(
            "MovementTypeNames",
            ListSemantics.Unordered);

        var formatted = handler.FormatValue(new List<string> { "Walk", "Run" });

        Assert.Equal("Walk, Run", formatted);
    }

    [Fact]
    public void TintLayersUsesStructuralFormattingInsteadOfCollectionTypeName()
    {
        var handler = new TintLayersHandler();
        IReadOnlyList<ITintLayerGetter> layers =
        [
            new TintLayer
            {
                Index = 3,
                InterpolationValue = 0.5f,
                Preset = 7
            }
        ];

        var formatted = handler.FormatValue(layers);

        Assert.Contains("Index=3", formatted, StringComparison.Ordinal);
        Assert.Contains("InterpolationValue=0.5", formatted, StringComparison.Ordinal);
        Assert.DoesNotContain("System.Collections.Generic", formatted, StringComparison.Ordinal);
    }

    [Fact]
    public void FormatterSkipsIndexersAndContainsGetterFailures()
    {
        var formatted = DiagnosticValueFormatter.Format(new HostileDiagnosticValue());

        Assert.Contains("Name=\"usable\"", formatted, StringComparison.Ordinal);
        Assert.Contains("Throws=<getter-error:InvalidOperationException>", formatted, StringComparison.Ordinal);
        Assert.DoesNotContain("Item=", formatted, StringComparison.Ordinal);
    }

    [Fact]
    public void FormatterHandlesCyclesWithoutRecursingForever()
    {
        var node = new CyclicValue { Name = "root" };
        node.Next = node;

        var formatted = DiagnosticValueFormatter.Format(node);

        Assert.Contains("Name=\"root\"", formatted, StringComparison.Ordinal);
        Assert.Contains("<cycle>", formatted, StringComparison.Ordinal);
    }

    [Fact]
    public void FormattingDoesNotInvokeEqualityOrDecisionMethods()
    {
        var handler = new DecisionProbeHandler();
        var value = new DecisionProbeValue { Number = 42 };

        var formatted = handler.FormatValue(value);

        Assert.Contains("Number=42", formatted, StringComparison.Ordinal);
        Assert.Equal(0, handler.EqualityCallCount);
        Assert.Equal(0, value.EqualsCallCount);
    }

    [Fact]
    public void PreviouslyWarnedMutagenShapesProduceStructuralOutput()
    {
        object[] values =
        [
            new BookSkill(),
            new BookSpell(),
            new BookTeachesNothing(),
            new DialogResponsesAdapter(),
            new Effect(),
            new Model(),
            new Lod(),
            new ContainerEntry(),
            new LeveledNpcEntry(),
            new LeveledSpellEntry(),
            new ShoutWord(),
            new Placement(),
            new WeatherColor()
        ];

        foreach (var value in values)
        {
            var formatted = DiagnosticValueFormatter.Format(value);

            Assert.NotEqual(value.GetType().FullName, formatted);
            Assert.NotEqual(value.GetType().Name, formatted);
            Assert.DoesNotContain("<format-error:", formatted, StringComparison.Ordinal);
            Assert.Contains('{', formatted);
        }
    }

    private sealed class HostileDiagnosticValue
    {
        public string Name => "usable";
        public string this[int index] => index.ToString();
        public string Throws => throw new InvalidOperationException("diagnostic getter failure");
    }

    private sealed class CyclicValue
    {
        public string Name { get; set; } = string.Empty;
        public CyclicValue? Next { get; set; }
    }

    private sealed class DecisionProbeValue
    {
        public int Number { get; set; }
        public int EqualsCallCount { get; private set; }

        public override bool Equals(object? obj)
        {
            EqualsCallCount++;
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode() => throw new InvalidOperationException("Formatting must use reference identity hashing.");
    }

    private sealed class DecisionProbeHandler : AbstractPropertyHandler<DecisionProbeValue>
    {
        public override string PropertyName => "Probe";
        public int EqualityCallCount { get; private set; }

        public override DecisionProbeValue? GetValue(IMajorRecordGetter record) => null;

        public override void SetValue(IMajorRecord record, DecisionProbeValue? value)
        {
        }

        public override bool AreValuesEqual(DecisionProbeValue? value1, DecisionProbeValue? value2)
        {
            EqualityCallCount++;
            return ReferenceEquals(value1, value2);
        }
    }
}
