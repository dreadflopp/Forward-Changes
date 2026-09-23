using DreadsMashedPatch.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Assets;

namespace DreadsMashedPatch.PropertyHandlers.TextureSet;

public sealed record TextureDefinition(
    AssetLinkGetter<SkyrimTextureAssetType>? Diffuse,
    AssetLinkGetter<SkyrimTextureAssetType>? NormalOrGloss,
    AssetLinkGetter<SkyrimTextureAssetType>? EnvironmentMaskOrSubsurfaceTint,
    AssetLinkGetter<SkyrimTextureAssetType>? GlowOrDetailMap,
    AssetLinkGetter<SkyrimTextureAssetType>? Height,
    AssetLinkGetter<SkyrimTextureAssetType>? Environment,
    AssetLinkGetter<SkyrimTextureAssetType>? Multilayer,
    AssetLinkGetter<SkyrimTextureAssetType>? BacklightMaskOrSpecular,
    Mutagen.Bethesda.Skyrim.TextureSet.Flag? Flags);

/// <summary>
/// Treats TX00-TX07 and the typed DNAM flags as one authored texture definition. DNAM copying and
/// equality continue to use the project-approved nullable flag handler.
/// </summary>
public sealed class TextureDefinitionHandler : AbstractPropertyHandler<TextureDefinition>
{
    private readonly DiffuseHandler _diffuse = new();
    private readonly NormalOrGlossHandler _normalOrGloss = new();
    private readonly EnvironmentMaskOrSubsurfaceTintHandler _environmentMask = new();
    private readonly GlowOrDetailMapHandler _glow = new();
    private readonly HeightHandler _height = new();
    private readonly EnvironmentHandler _environment = new();
    private readonly MultilayerHandler _multilayer = new();
    private readonly BacklightMaskOrSpecularHandler _backlight = new();
    private readonly FlagsHandler _flags = new();

    public override string PropertyName => "TextureDefinition";

    public override TextureDefinition GetValue(IMajorRecordGetter record) => new(
        _diffuse.GetValue(record),
        _normalOrGloss.GetValue(record),
        _environmentMask.GetValue(record),
        _glow.GetValue(record),
        _height.GetValue(record),
        _environment.GetValue(record),
        _multilayer.GetValue(record),
        _backlight.GetValue(record),
        _flags.GetValue(record));

    public override void SetValue(IMajorRecord record, TextureDefinition? value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        _diffuse.SetValue(record, value.Diffuse);
        _normalOrGloss.SetValue(record, value.NormalOrGloss);
        _environmentMask.SetValue(record, value.EnvironmentMaskOrSubsurfaceTint);
        _glow.SetValue(record, value.GlowOrDetailMap);
        _height.SetValue(record, value.Height);
        _environment.SetValue(record, value.Environment);
        _multilayer.SetValue(record, value.Multilayer);
        _backlight.SetValue(record, value.BacklightMaskOrSpecular);
        _flags.SetValue(record, value.Flags);
    }

    public override bool AreValuesEqual(TextureDefinition? value1, TextureDefinition? value2)
    {
        if (value1 == null || value2 == null)
        {
            return value1 == null && value2 == null;
        }

        return _diffuse.AreValuesEqual(value1.Diffuse, value2.Diffuse)
            && _normalOrGloss.AreValuesEqual(value1.NormalOrGloss, value2.NormalOrGloss)
            && _environmentMask.AreValuesEqual(
                value1.EnvironmentMaskOrSubsurfaceTint,
                value2.EnvironmentMaskOrSubsurfaceTint)
            && _glow.AreValuesEqual(value1.GlowOrDetailMap, value2.GlowOrDetailMap)
            && _height.AreValuesEqual(value1.Height, value2.Height)
            && _environment.AreValuesEqual(value1.Environment, value2.Environment)
            && _multilayer.AreValuesEqual(value1.Multilayer, value2.Multilayer)
            && _backlight.AreValuesEqual(value1.BacklightMaskOrSpecular, value2.BacklightMaskOrSpecular)
            && _flags.AreValuesEqual(value1.Flags, value2.Flags);
    }

    public override string FormatValue(object? value)
    {
        if (value is not TextureDefinition definition)
        {
            return value?.ToString() ?? "null";
        }

        return $"Diffuse={_diffuse.FormatValue(definition.Diffuse)}, " +
               $"Normal={_normalOrGloss.FormatValue(definition.NormalOrGloss)}, " +
               $"EnvironmentMask={_environmentMask.FormatValue(definition.EnvironmentMaskOrSubsurfaceTint)}, " +
               $"Glow={_glow.FormatValue(definition.GlowOrDetailMap)}, " +
               $"Height={_height.FormatValue(definition.Height)}, " +
               $"Environment={_environment.FormatValue(definition.Environment)}, " +
               $"Multilayer={_multilayer.FormatValue(definition.Multilayer)}, " +
               $"Backlight={_backlight.FormatValue(definition.BacklightMaskOrSpecular)}, " +
               $"Flags={_flags.FormatValue(definition.Flags)}";
    }
}
