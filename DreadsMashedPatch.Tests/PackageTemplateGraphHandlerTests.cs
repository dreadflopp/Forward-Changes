using System.Text;
using DreadsMashedPatch.PropertyHandlers.Package;
using DreadsMashedPatch.RecordHandlers;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Xunit;

namespace DreadsMashedPatch.Tests;

[Collection("LogCollector")]
public sealed class PackageTemplateGraphHandlerTests
{
    private static readonly ModKey TestModKey = ModKey.FromNameAndExtension("PackageGraphTests.esp");

    [Fact]
    public void RecordHandlerTemporarilyDisablesPackageTemplateGraph()
    {
        var handlers = new PackageRecordHandler().PropertyHandlers;

        Assert.DoesNotContain("PackageTemplateGraph", handlers.Keys);
        Assert.DoesNotContain("PackageTemplate", handlers.Keys);
        Assert.DoesNotContain("DataInputVersion", handlers.Keys);
        Assert.DoesNotContain("Data", handlers.Keys);
        Assert.DoesNotContain("XnamMarker", handlers.Keys);
        Assert.DoesNotContain("ProcedureTree", handlers.Keys);
    }

    [Fact]
    public void SetValueDeepCopiesCompleteGraphAndPreservesBranchOrder()
    {
        var source = CreatePackage(0x800);
        var target = new Package(new FormKey(TestModKey, 0x801), SkyrimRelease.SkyrimSE);
        var handler = new PackageTemplateGraphHandler();

        handler.SetValue(target, handler.GetValue(source));

        Assert.True(handler.AreValuesEqual(handler.GetValue(source), handler.GetValue(target)));
        Assert.Equal(["Sequence", "Procedure"], target.ProcedureTree.Select(branch => branch.BranchType));
        Assert.Equal([0, 3, 5, 255], target.ProcedureTree[1].DataInputIndices);
        Assert.Equal(5, Assert.IsType<GetNumericPackageDataConditionData>(
            Assert.Single(target.ProcedureTree[1].Conditions).Data).PackageDataIndex);
        Assert.Equal(2, target.ProcedureTree[0].Root!.BranchCount);

        Assert.NotSame(source.Data[0], target.Data[0]);
        Assert.NotSame(source.ProcedureTree[0], target.ProcedureTree[0]);
        Assert.NotSame(source.ProcedureTree[1].Conditions[0], target.ProcedureTree[1].Conditions[0]);

        ((PackageDataFloat)source.Data[0]).Data = 99;
        source.ProcedureTree[1].DataInputIndices[0] = 42;
        ((ConditionFloat)source.ProcedureTree[1].Conditions[0]).ComparisonValue = 99;

        Assert.Equal(12.5f, Assert.IsType<PackageDataFloat>(target.Data[0]).Data);
        Assert.Equal(0, target.ProcedureTree[1].DataInputIndices[0]);
        Assert.Equal(1, Assert.IsType<ConditionFloat>(target.ProcedureTree[1].Conditions[0]).ComparisonValue);
        Assert.False(handler.AreValuesEqual(handler.GetValue(source), handler.GetValue(target)));
    }

    [Fact]
    public void DataAndProcedureTreeParticipateInOneOwnershipDecision()
    {
        var original = CreatePackage(0x803);
        var aiOverhaul = CreatePackage(0x803);
        var flowReversion = CreatePackage(0x803);
        aiOverhaul.Data[8] = new PackageDataBool { Name = "AI addition", Data = true };
        aiOverhaul.ProcedureTree.Add(new PackageBranch
        {
            BranchType = "Procedure",
            ProcedureType = "Wait"
        });
        var handler = new PackageTemplateGraphHandler();

        Assert.True(handler.AreValuesEqual(
            handler.GetValue(original),
            handler.GetValue(flowReversion)));
        Assert.False(handler.AreValuesEqual(
            handler.GetValue(original),
            handler.GetValue(aiOverhaul)));
    }

    [Fact]
    public void DeepDiveFormatShowsCompactOrderDiagnostics()
    {
        var package = CreatePackage(0x804);
        var handler = new PackageTemplateGraphHandler();

        LogCollector.SetRecordLoggingContext(deepDiveRecord: true, detailedRecord: true);
        try
        {
            var formatted = handler.FormatValue(handler.GetValue(package));

            Assert.Contains("Graph#", formatted);
            Assert.Contains("Data:4 (values:3, metadata:1)", formatted);
            Assert.Contains("Order:mismatch@0 source:[5,0,3] writer:[0,3,5]", formatted);
            Assert.Contains("Metadata:[2]", formatted);
            Assert.DoesNotContain("Data=true", formatted);
        }
        finally
        {
            LogCollector.SetRecordLoggingContext(deepDiveRecord: false, detailedRecord: false);
        }
    }

    [Fact]
    public void DiagnosticDifferenceOnlyExpandsChangedDataAndTreePositions()
    {
        var older = CreatePackage(0x805);
        var newer = CreatePackage(0x805);
        newer.Data[8] = new PackageDataBool { Name = "Added", Data = true };
        Assert.IsType<PackageDataFloat>(newer.Data[0]).Data = 25;
        newer.ProcedureTree[1].DataInputIndices[0] = 8;
        var handler = new PackageTemplateGraphHandler();

        var difference = handler.FormatDifference(
            handler.GetValue(older),
            handler.GetValue(newer));

        Assert.Contains("Data added:[8]", difference);
        Assert.Contains("Data changed:[0]", difference);
        Assert.Contains("Tree changed:[1]", difference);
        Assert.DoesNotContain("Metadata only", difference);
        Assert.Equal(
            handler.FormatIdentity(handler.GetValue(older)),
            handler.FormatIdentity(handler.GetValue(CreatePackage(0x805))));
    }

    [Fact]
    public void DeepDiveDiagnosticsStayBoundedForLargePackageGraphs()
    {
        var smaller = CreatePackage(0x806);
        var larger = CreatePackage(0x806);
        for (sbyte index = 6; index < 99; index++)
        {
            larger.Data[index] = new APackageData { Name = $"Metadata {index}" };
        }
        for (var index = 0; index < 100; index++)
        {
            larger.ProcedureTree.Add(new PackageBranch
            {
                BranchType = "Procedure",
                ProcedureType = $"Procedure {index}"
            });
        }
        var handler = new PackageTemplateGraphHandler();

        LogCollector.SetRecordLoggingContext(deepDiveRecord: true, detailedRecord: true);
        try
        {
            var formatted = handler.FormatValue(handler.GetValue(larger));
            var difference = handler.FormatDifference(
                handler.GetValue(smaller),
                handler.GetValue(larger));

            Assert.True(formatted.Length < 1_000, $"Compact graph summary was {formatted.Length} characters.");
            Assert.True(difference.Length < 5_000, $"Graph difference was {difference.Length} characters.");
            Assert.Contains("...(+", difference);
            Assert.Contains("Tree added:", difference);
        }
        finally
        {
            LogCollector.SetRecordLoggingContext(deepDiveRecord: false, detailedRecord: false);
        }
    }

    [Fact]
    public void BinaryRoundTripKeepsIndexValueAssociationsAndCanonicalKeyOrder()
    {
        var package = CreatePackage(0x802);
        var patchPackage = new Package(package.FormKey, SkyrimRelease.SkyrimSE);
        var handler = new PackageTemplateGraphHandler();
        handler.SetValue(patchPackage, handler.GetValue(package));

        var patch = new SkyrimMod(TestModKey, SkyrimRelease.SkyrimSE);
        patch.Packages.Add(patchPackage);
        using var stream = new MemoryStream();
        patch.WriteToBinary(stream);
        var bytes = stream.ToArray();

        var packageTemplateStart = FindSubrecord(bytes, "PKCU", 0);
        var markerStart = FindSubrecord(bytes, "XNAM", packageTemplateStart + 6);
        Assert.True(packageTemplateStart >= 0);
        Assert.True(markerStart > packageTemplateStart);

        Assert.Equal(["Float", "Int", "Bool"],
            ReadSubrecordStrings(bytes, "ANAM", packageTemplateStart, markerStart));
        Assert.Equal([0, 3, 5],
            ReadSingleByteSubrecords(bytes, "UNAM", packageTemplateStart, markerStart));

        stream.Position = 0;
        using var overlay = SkyrimMod.CreateFromBinaryOverlay(
            stream,
            SkyrimRelease.SkyrimSE,
            TestModKey);
        var roundTripped = Assert.Single(overlay.Packages);

        Assert.Equal(12.5f, Assert.IsAssignableFrom<IPackageDataFloatGetter>(roundTripped.Data[0]).Data);
        Assert.Equal("Metadata only", roundTripped.Data[2].Name);
        Assert.Equal(42u, Assert.IsAssignableFrom<IPackageDataIntGetter>(roundTripped.Data[3]).Data);
        Assert.True(Assert.IsAssignableFrom<IPackageDataBoolGetter>(roundTripped.Data[5]).Data);
        Assert.Equal(["Sequence", "Procedure"], roundTripped.ProcedureTree.Select(branch => branch.BranchType));
        Assert.Equal([0, 3, 5, 255], roundTripped.ProcedureTree[1].DataInputIndices);
        Assert.True(handler.AreValuesEqual(handler.GetValue(patchPackage), handler.GetValue(roundTripped)));
    }

    private static Package CreatePackage(uint formId)
    {
        var package = new Package(new FormKey(TestModKey, formId), SkyrimRelease.SkyrimSE)
        {
            DataInputVersion = 17,
            XnamMarker = new MemorySlice<byte>([0])
        };
        package.PackageTemplate.SetTo(new FormKey(TestModKey, 0x700));

        // Deliberately insert these out of key order. Mutagen writes serializable
        // values and their UNAM indexes in canonical key order.
        package.Data[5] = new PackageDataBool { Name = "Enabled", Data = true };
        package.Data[0] = new PackageDataFloat { Name = "Distance", Data = 12.5f };
        package.Data[3] = new PackageDataInt { Name = "Count", Data = 42 };
        package.Data[2] = new APackageData { Name = "Metadata only" };

        package.ProcedureTree.Add(new PackageBranch
        {
            BranchType = "Sequence",
            Root = new PackageRoot
            {
                BranchCount = 2,
                Flags = PackageRoot.Flag.RepeatWhenComplete
            }
        });

        var procedure = new PackageBranch
        {
            BranchType = "Procedure",
            ProcedureType = "Travel",
            Flags = 0
        };
        procedure.DataInputIndices.AddRange([0, 3, 5, 255]);
        procedure.Conditions.Add(new ConditionFloat
        {
            CompareOperator = CompareOperator.EqualTo,
            ComparisonValue = 1,
            Data = new GetNumericPackageDataConditionData
            {
                PackageDataIndex = 5
            }
        });
        package.ProcedureTree.Add(procedure);

        return package;
    }

    private static int FindSubrecord(byte[] data, string type, int start)
    {
        var tag = Encoding.ASCII.GetBytes(type);
        for (var index = Math.Max(0, start); index <= data.Length - tag.Length; index++)
        {
            if (data.AsSpan(index, tag.Length).SequenceEqual(tag))
            {
                return index;
            }
        }

        return -1;
    }

    private static string[] ReadSubrecordStrings(
        byte[] data,
        string type,
        int start,
        int end)
    {
        var values = new List<string>();
        var offset = start;
        while ((offset = FindSubrecord(data, type, offset)) >= 0 && offset < end)
        {
            var length = BitConverter.ToUInt16(data, offset + 4);
            values.Add(Encoding.UTF8.GetString(data, offset + 6, length).TrimEnd('\0'));
            offset += 6 + length;
        }

        return values.ToArray();
    }

    private static byte[] ReadSingleByteSubrecords(
        byte[] data,
        string type,
        int start,
        int end)
    {
        var values = new List<byte>();
        var offset = start;
        while ((offset = FindSubrecord(data, type, offset)) >= 0 && offset < end)
        {
            Assert.Equal(1, BitConverter.ToUInt16(data, offset + 4));
            values.Add(data[offset + 6]);
            offset += 7;
        }

        return values.ToArray();
    }
}
