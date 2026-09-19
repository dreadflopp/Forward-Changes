using System;
using DreadsMashedPatch.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace DreadsMashedPatch.PropertyHandlers.EffectShader
{
    public class EffectShaderDataHandler : AbstractPropertyHandler<IEffectShaderGetter>
    {
        private static readonly Mutagen.Bethesda.Skyrim.EffectShader.TranslationMask DataComparisonMask = new(defaultOn: true)
        {
            MajorRecordFlagsRaw = false,
            FormKey = false,
            VersionControl = false,
            EditorID = false,
            FormVersion = false,
            Version2 = false,
            SkyrimMajorRecordFlags = false,
            FillTexture = false,
            ParticleShaderTexture = false,
            HolesTexture = false,
            MembranePaletteTexture = false,
            ParticlePaletteTexture = false,
            DATADataTypeState = false
        };

        public override string PropertyName => "EffectShaderData";

        public override IEffectShaderGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IEffectShaderGetter effectShaderRecord)
            {
                return effectShaderRecord;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IEffectShaderGetter for {PropertyName}");
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, IEffectShaderGetter? value)
        {
            if (record is IEffectShader effectShaderRecord && value != null)
            {
                // Copy all properties from Unknown onwards
                effectShaderRecord.Unknown = value.Unknown;
                effectShaderRecord.MembraneSourceBlendMode = value.MembraneSourceBlendMode;
                effectShaderRecord.MembraneBlendOperation = value.MembraneBlendOperation;
                effectShaderRecord.MembraneZTest = value.MembraneZTest;
                effectShaderRecord.FillColorKey1 = value.FillColorKey1;
                effectShaderRecord.FillAlphaFadeInTime = value.FillAlphaFadeInTime;
                effectShaderRecord.FillFullAlphaTime = value.FillFullAlphaTime;
                effectShaderRecord.FillFadeOutTime = value.FillFadeOutTime;
                effectShaderRecord.FillPersistentAlphaRatio = value.FillPersistentAlphaRatio;
                effectShaderRecord.FillAlphaPulseAmplitude = value.FillAlphaPulseAmplitude;
                effectShaderRecord.FillAlphaPulseFrequency = value.FillAlphaPulseFrequency;
                effectShaderRecord.FillTextureAnimationSpeedU = value.FillTextureAnimationSpeedU;
                effectShaderRecord.FillTextureAnimationSpeedV = value.FillTextureAnimationSpeedV;
                effectShaderRecord.EdgeEffectFallOff = value.EdgeEffectFallOff;
                effectShaderRecord.EdgeEffectColor = value.EdgeEffectColor;
                effectShaderRecord.EdgeEffectAlphaFadeInTime = value.EdgeEffectAlphaFadeInTime;
                effectShaderRecord.EdgeEffectFullAlphaTime = value.EdgeEffectFullAlphaTime;
                effectShaderRecord.EdgeEffectAlphaFadeOutTime = value.EdgeEffectAlphaFadeOutTime;
                effectShaderRecord.EdgeEffectPersistentAlphaRatio = value.EdgeEffectPersistentAlphaRatio;
                effectShaderRecord.EdgeEffectAlphaPulseAmplitude = value.EdgeEffectAlphaPulseAmplitude;
                effectShaderRecord.EdgeEffectAlphaPulseFrequency = value.EdgeEffectAlphaPulseFrequency;
                effectShaderRecord.FillFullAlphaRatio = value.FillFullAlphaRatio;
                effectShaderRecord.EdgeEffectFullAlphaRatio = value.EdgeEffectFullAlphaRatio;
                effectShaderRecord.MembraneDestBlendMode = value.MembraneDestBlendMode;
                effectShaderRecord.ParticleSourceBlendMode = value.ParticleSourceBlendMode;
                effectShaderRecord.ParticleBlendOperation = value.ParticleBlendOperation;
                effectShaderRecord.ParticleZTest = value.ParticleZTest;
                effectShaderRecord.ParticleDestBlendMode = value.ParticleDestBlendMode;
                effectShaderRecord.ParticleBirthRampUpTime = value.ParticleBirthRampUpTime;
                effectShaderRecord.ParticleFullBirthTime = value.ParticleFullBirthTime;
                effectShaderRecord.ParticleBirthRampDownTime = value.ParticleBirthRampDownTime;
                effectShaderRecord.ParticleFullBirthRatio = value.ParticleFullBirthRatio;
                effectShaderRecord.ParticlePeristentCount = value.ParticlePeristentCount;
                effectShaderRecord.ParticleLifetime = value.ParticleLifetime;
                effectShaderRecord.ParticleLifetimePlusMinus = value.ParticleLifetimePlusMinus;
                effectShaderRecord.ParticleInitialSpeedAlongNormal = value.ParticleInitialSpeedAlongNormal;
                effectShaderRecord.ParticleAccelerationAlongNormal = value.ParticleAccelerationAlongNormal;
                effectShaderRecord.ParticleInitialVelocity1 = value.ParticleInitialVelocity1;
                effectShaderRecord.ParticleInitialVelocity2 = value.ParticleInitialVelocity2;
                effectShaderRecord.ParticleInitialVelocity3 = value.ParticleInitialVelocity3;
                effectShaderRecord.ParticleAcceleration1 = value.ParticleAcceleration1;
                effectShaderRecord.ParticleAcceleration2 = value.ParticleAcceleration2;
                effectShaderRecord.ParticleAcceleration3 = value.ParticleAcceleration3;
                effectShaderRecord.ParticleScaleKey1 = value.ParticleScaleKey1;
                effectShaderRecord.ParticleScaleKey2 = value.ParticleScaleKey2;
                effectShaderRecord.ParticleScaleKey1Time = value.ParticleScaleKey1Time;
                effectShaderRecord.ParticleScaleKey2Time = value.ParticleScaleKey2Time;
                effectShaderRecord.ColorKey1 = value.ColorKey1;
                effectShaderRecord.ColorKey2 = value.ColorKey2;
                effectShaderRecord.ColorKey3 = value.ColorKey3;
                effectShaderRecord.ColorKey1Alpha = value.ColorKey1Alpha;
                effectShaderRecord.ColorKey2Alpha = value.ColorKey2Alpha;
                effectShaderRecord.ColorKey3Alpha = value.ColorKey3Alpha;
                effectShaderRecord.ColorKey1Time = value.ColorKey1Time;
                effectShaderRecord.ColorKey2Time = value.ColorKey2Time;
                effectShaderRecord.ColorKey3Time = value.ColorKey3Time;
                effectShaderRecord.ParticleInitialSpeedAlongNormalPlusMinus = value.ParticleInitialSpeedAlongNormalPlusMinus;
                effectShaderRecord.ParticleInitialRotationDegree = value.ParticleInitialRotationDegree;
                effectShaderRecord.ParticleInitialRotationDegreePlusMinus = value.ParticleInitialRotationDegreePlusMinus;
                effectShaderRecord.ParticleRotationSpeedDegreePerSec = value.ParticleRotationSpeedDegreePerSec;
                effectShaderRecord.ParticleRotationSpeedDegreePerSecPlusMinus = value.ParticleRotationSpeedDegreePerSecPlusMinus;

                // Handle form links
                if (value.AddonModels != null && !value.AddonModels.FormKey.IsNull)
                {
                    effectShaderRecord.AddonModels = new FormLink<IDebrisGetter>(value.AddonModels.FormKey);
                }
                else
                {
                    effectShaderRecord.AddonModels.Clear();
                }

                effectShaderRecord.HolesStartTime = value.HolesStartTime;
                effectShaderRecord.HolesEndTime = value.HolesEndTime;
                effectShaderRecord.HolesStartValue = value.HolesStartValue;
                effectShaderRecord.HolesEndValue = value.HolesEndValue;
                effectShaderRecord.EdgeWidth = value.EdgeWidth;
                effectShaderRecord.EdgeColor = value.EdgeColor;
                effectShaderRecord.ExplosionWindSpeed = value.ExplosionWindSpeed;
                effectShaderRecord.TextureCountU = value.TextureCountU;
                effectShaderRecord.TextureCountV = value.TextureCountV;
                effectShaderRecord.AddonModelsFadeInTime = value.AddonModelsFadeInTime;
                effectShaderRecord.AddonModelsFadeOutTime = value.AddonModelsFadeOutTime;
                effectShaderRecord.AddonModelsScaleStart = value.AddonModelsScaleStart;
                effectShaderRecord.AddonModelsScaleEnd = value.AddonModelsScaleEnd;
                effectShaderRecord.AddonModelsScaleInTime = value.AddonModelsScaleInTime;
                effectShaderRecord.AddonModelsScaleOutTime = value.AddonModelsScaleOutTime;

                // Handle AmbientSound form link
                if (value.AmbientSound != null && !value.AmbientSound.FormKey.IsNull)
                {
                    effectShaderRecord.AmbientSound = new FormLink<ISoundGetter>(value.AmbientSound.FormKey);
                }
                else
                {
                    effectShaderRecord.AmbientSound.Clear();
                }

                effectShaderRecord.FillColorKey2 = value.FillColorKey2;
                effectShaderRecord.FillColorKey3 = value.FillColorKey3;
                effectShaderRecord.FillColorKey1Scale = value.FillColorKey1Scale;
                effectShaderRecord.FillColorKey2Scale = value.FillColorKey2Scale;
                effectShaderRecord.FillColorKey3Scale = value.FillColorKey3Scale;
                effectShaderRecord.FillColorKey1Time = value.FillColorKey1Time;
                effectShaderRecord.FillColorKey2Time = value.FillColorKey2Time;
                effectShaderRecord.FillColorKey3Time = value.FillColorKey3Time;
                effectShaderRecord.ColorScale = value.ColorScale;
                effectShaderRecord.BirthPositionOffset = value.BirthPositionOffset;
                effectShaderRecord.BirthPositionOffsetRangePlusMinus = value.BirthPositionOffsetRangePlusMinus;
                effectShaderRecord.ParticleAnimatedStartFrame = value.ParticleAnimatedStartFrame;
                effectShaderRecord.ParticleAnimatedStartFrameVariation = value.ParticleAnimatedStartFrameVariation;
                effectShaderRecord.ParticleAnimatedEndFrame = value.ParticleAnimatedEndFrame;
                effectShaderRecord.ParticleAnimatedLoopStartFrame = value.ParticleAnimatedLoopStartFrame;
                effectShaderRecord.ParticleAnimatedLoopStartVariation = value.ParticleAnimatedLoopStartVariation;
                effectShaderRecord.ParticleAnimatedFrameCount = value.ParticleAnimatedFrameCount;
                effectShaderRecord.ParticleAnimatedFrameCountVariation = value.ParticleAnimatedFrameCountVariation;
                effectShaderRecord.Flags = value.Flags;
                effectShaderRecord.FillTextureScaleU = value.FillTextureScaleU;
                effectShaderRecord.FillTextureScaleV = value.FillTextureScaleV;
                effectShaderRecord.SceneGraphEmitDepthLimit = value.SceneGraphEmitDepthLimit;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IEffectShader for {PropertyName}");
            }
        }

        // The generated equality handles semantic DATA atomically. The setter stays specialized
        // because generated DeepCopyIn also copies the separately handled texture subrecords.
        public override bool AreValuesEqual(IEffectShaderGetter? value1, IEffectShaderGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            return EffectShaderMixIn.Equals(value1, value2, DataComparisonMask);
        }
    }
}
