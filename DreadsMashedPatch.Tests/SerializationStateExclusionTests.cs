using DreadsMashedPatch.RecordHandlers;
using DreadsMashedPatch.RecordHandlers.Abstracts;
using System.Text.RegularExpressions;
using Xunit;

namespace DreadsMashedPatch.Tests;

public sealed class SerializationStateExclusionTests
{
    [Fact]
    public void RecordHandlersDoNotRegisterDeferredUnknownFields()
    {
        AbstractRecordHandler[] handlers =
        [
            new ArmorAddonRecordHandler(),
            new ClassRecordHandler(),
            new GrassRecordHandler(),
            new ImageSpaceAdapterRecordHandler(),
            new ImpactRecordHandler(),
            new LightingTemplateRecordHandler(),
            new MagicEffectRecordHandler(),
            new PackageRecordHandler(),
            new PlacedObjectRecordHandler(),
            new QuestRecordHandler(),
            new RaceRecordHandler(),
            new RelationshipRecordHandler(),
            new ReverbParametersRecordHandler(),
            new TreeRecordHandler(),
            new WaterRecordHandler(),
            new WeaponRecordHandler(),
            new WeatherRecordHandler()
        ];

        foreach (var handler in handlers)
        {
            Assert.DoesNotContain(handler.PropertyHandlers.Keys, key =>
            {
                var leaf = key[(key.LastIndexOf('.') + 1)..];
                return Regex.IsMatch(leaf, "^Unknown(?:[0-9]+|[0-9][A-F])?$", RegexOptions.IgnoreCase);
            });
        }
    }

    [Fact]
    public void RecordHandlersDoNotRegisterMutagenSerializationState()
    {
        AbstractRecordHandler[] handlers =
        [
            new AmmunitionRecordHandler(),
            new ArmorAddonRecordHandler(),
            new ArmorRecordHandler(),
            new CameraShotRecordHandler(),
            new CombatStyleRecordHandler(),
            new ExplosionRecordHandler(),
            new LightingTemplateRecordHandler(),
            new MaterialObjectRecordHandler(),
            new MovementTypeRecordHandler(),
            new ProjectileRecordHandler(),
            new RaceRecordHandler(),
            new ShaderParticleGeometryRecordHandler(),
            new WaterRecordHandler(),
            new WeatherRecordHandler()
        ];

        foreach (var handler in handlers)
        {
            Assert.DoesNotContain(handler.PropertyHandlers.Keys, key =>
                key.EndsWith("DataTypeState", StringComparison.Ordinal)
                || key.Contains(".ActsLike", StringComparison.Ordinal)
                || key.StartsWith("Exporting", StringComparison.Ordinal));
        }
    }
}
