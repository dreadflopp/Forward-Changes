using DreadsMashedPatch.RecordHandlers;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace DreadsMashedPatch.Tests;

public class IntentionalExclusionTests
{
    [Fact]
    public void RuntimeAndOpaquePropertiesAreNotRegistered()
    {
        var exclusions = new (AbstractRecordHandler Handler, string[] Properties)[]
        {
            (new ActorValueInformationRecordHandler(), ["CNAM"]),
            (new ArmorAddonRecordHandler(),
            [
                "Unknown", "Unknown2", "WorldModel.Male.Data", "WorldModel.Female.Data",
                "FirstPersonModel.Male.Data", "FirstPersonModel.Female.Data"
            ]),
            (new CellRecordHandler(), ["WaterHeight", "Landscape", "NavigationMeshes"]),
            (new ClassRecordHandler(), ["Unknown", "Unknown2"]),
            (new DialogResponseRecordHandler(), ["PreviousDialog", "UnknownData"]),
            (new GrassRecordHandler(), ["Unknown", "Unknown2", "Unknown3"]),
            (new ImageSpaceAdapterRecordHandler(),
            [
                "Unknown08", "Unknown48", "Unknown09", "Unknown49", "Unknown0A", "Unknown4A",
                "Unknown0B", "Unknown4B", "Unknown0C", "Unknown4C", "Unknown0D", "Unknown4D",
                "Unknown0E", "Unknown4E", "Unknown0F", "Unknown4F", "Unknown10", "Unknown50",
                "Unknown14", "Unknown54"
            ]),
            (new ImpactRecordHandler(), ["Unknown"]),
            (new LightingTemplateRecordHandler(), ["Unknown"]),
            (new MagicEffectRecordHandler(), ["Unknown1"]),
            (new PackageRecordHandler(), ["Unknown", "Unknown2", "Unknown3", "Unknown4"]),
            (new PlacedObjectRecordHandler(), ["Unknown"]),
            (new QuestRecordHandler(), ["Unknown", "QuestFormVersion"]),
            (new RaceRecordHandler(), ["Unknown"]),
            (new RelationshipRecordHandler(), ["Unknown"]),
            (new TreeRecordHandler(), ["Unknown"]),
            (new WaterRecordHandler(), ["Unknown", "Unknown2", "Unknown3", "Unknown4", "Unknown5", "Unknown6", "Unknown7"]),
            (new WeaponRecordHandler(), ["Data.Unknown", "Data.Unknown2", "Data.Unknown3", "Data.Unknown4", "Data.Unknown5"]),
            (new WeatherRecordHandler(), ["Unknown"])
        };

        foreach (var (handler, properties) in exclusions)
        {
            foreach (var property in properties)
            {
                Assert.DoesNotContain(property, handler.PropertyHandlers.Keys);
            }
        }
    }

    [Fact]
    public void RuntimeManagedRecordTypesAreExplicitlyExcluded()
    {
        var excludedTypes = new[]
        {
            typeof(IDefaultObjectManagerGetter),
            typeof(ILandscapeTextureGetter),
            typeof(ILandscapeGetter),
            typeof(IImageSpaceAdapterGetter)
        };

        foreach (var recordType in excludedTypes)
        {
            Assert.DoesNotContain(recordType, Program.SupportedRecordTypes);
            Assert.True(Program.ExcludedRecordTypes.ContainsKey(recordType));
            Assert.False(string.IsNullOrWhiteSpace(Program.ExcludedRecordTypes[recordType]));
        }
    }
}
