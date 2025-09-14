using System;
using System.Drawing;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.EffectShader
{
    public class EffectShaderDataHandler : AbstractPropertyHandler<IEffectShaderGetter>
    {
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

        public override bool AreValuesEqual(IEffectShaderGetter? value1, IEffectShaderGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare all properties from Unknown onwards
            if (value1.Unknown != value2.Unknown) return false;
            if (value1.MembraneSourceBlendMode != value2.MembraneSourceBlendMode) return false;
            if (value1.MembraneBlendOperation != value2.MembraneBlendOperation) return false;
            if (value1.MembraneZTest != value2.MembraneZTest) return false;
            if (value1.FillColorKey1.ToArgb() != value2.FillColorKey1.ToArgb()) return false;
            if (Math.Abs(value1.FillAlphaFadeInTime - value2.FillAlphaFadeInTime) >= 0.001f) return false;
            if (Math.Abs(value1.FillFullAlphaTime - value2.FillFullAlphaTime) >= 0.001f) return false;
            if (Math.Abs(value1.FillFadeOutTime - value2.FillFadeOutTime) >= 0.001f) return false;
            if (Math.Abs(value1.FillPersistentAlphaRatio - value2.FillPersistentAlphaRatio) >= 0.001f) return false;
            if (Math.Abs(value1.FillAlphaPulseAmplitude - value2.FillAlphaPulseAmplitude) >= 0.001f) return false;
            if (Math.Abs(value1.FillAlphaPulseFrequency - value2.FillAlphaPulseFrequency) >= 0.001f) return false;
            if (Math.Abs(value1.FillTextureAnimationSpeedU - value2.FillTextureAnimationSpeedU) >= 0.001f) return false;
            if (Math.Abs(value1.FillTextureAnimationSpeedV - value2.FillTextureAnimationSpeedV) >= 0.001f) return false;
            if (Math.Abs(value1.EdgeEffectFallOff - value2.EdgeEffectFallOff) >= 0.001f) return false;
            if (value1.EdgeEffectColor.ToArgb() != value2.EdgeEffectColor.ToArgb()) return false;
            if (Math.Abs(value1.EdgeEffectAlphaFadeInTime - value2.EdgeEffectAlphaFadeInTime) >= 0.001f) return false;
            if (Math.Abs(value1.EdgeEffectFullAlphaTime - value2.EdgeEffectFullAlphaTime) >= 0.001f) return false;
            if (Math.Abs(value1.EdgeEffectAlphaFadeOutTime - value2.EdgeEffectAlphaFadeOutTime) >= 0.001f) return false;
            if (Math.Abs(value1.EdgeEffectPersistentAlphaRatio - value2.EdgeEffectPersistentAlphaRatio) >= 0.001f) return false;
            if (Math.Abs(value1.EdgeEffectAlphaPulseAmplitude - value2.EdgeEffectAlphaPulseAmplitude) >= 0.001f) return false;
            if (Math.Abs(value1.EdgeEffectAlphaPulseFrequency - value2.EdgeEffectAlphaPulseFrequency) >= 0.001f) return false;
            if (Math.Abs(value1.FillFullAlphaRatio - value2.FillFullAlphaRatio) >= 0.001f) return false;
            if (Math.Abs(value1.EdgeEffectFullAlphaRatio - value2.EdgeEffectFullAlphaRatio) >= 0.001f) return false;
            if (value1.MembraneDestBlendMode != value2.MembraneDestBlendMode) return false;
            if (value1.ParticleSourceBlendMode != value2.ParticleSourceBlendMode) return false;
            if (value1.ParticleBlendOperation != value2.ParticleBlendOperation) return false;
            if (value1.ParticleZTest != value2.ParticleZTest) return false;
            if (value1.ParticleDestBlendMode != value2.ParticleDestBlendMode) return false;
            if (Math.Abs(value1.ParticleBirthRampUpTime - value2.ParticleBirthRampUpTime) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleFullBirthTime - value2.ParticleFullBirthTime) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleBirthRampDownTime - value2.ParticleBirthRampDownTime) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleFullBirthRatio - value2.ParticleFullBirthRatio) >= 0.001f) return false;
            if (Math.Abs(value1.ParticlePeristentCount - value2.ParticlePeristentCount) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleLifetime - value2.ParticleLifetime) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleLifetimePlusMinus - value2.ParticleLifetimePlusMinus) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleInitialSpeedAlongNormal - value2.ParticleInitialSpeedAlongNormal) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleAccelerationAlongNormal - value2.ParticleAccelerationAlongNormal) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleInitialVelocity1 - value2.ParticleInitialVelocity1) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleInitialVelocity2 - value2.ParticleInitialVelocity2) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleInitialVelocity3 - value2.ParticleInitialVelocity3) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleAcceleration1 - value2.ParticleAcceleration1) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleAcceleration2 - value2.ParticleAcceleration2) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleAcceleration3 - value2.ParticleAcceleration3) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleScaleKey1 - value2.ParticleScaleKey1) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleScaleKey2 - value2.ParticleScaleKey2) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleScaleKey1Time - value2.ParticleScaleKey1Time) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleScaleKey2Time - value2.ParticleScaleKey2Time) >= 0.001f) return false;
            if (value1.ColorKey1.ToArgb() != value2.ColorKey1.ToArgb()) return false;
            if (value1.ColorKey2.ToArgb() != value2.ColorKey2.ToArgb()) return false;
            if (value1.ColorKey3.ToArgb() != value2.ColorKey3.ToArgb()) return false;
            if (Math.Abs(value1.ColorKey1Alpha - value2.ColorKey1Alpha) >= 0.001f) return false;
            if (Math.Abs(value1.ColorKey2Alpha - value2.ColorKey2Alpha) >= 0.001f) return false;
            if (Math.Abs(value1.ColorKey3Alpha - value2.ColorKey3Alpha) >= 0.001f) return false;
            if (Math.Abs(value1.ColorKey1Time - value2.ColorKey1Time) >= 0.001f) return false;
            if (Math.Abs(value1.ColorKey2Time - value2.ColorKey2Time) >= 0.001f) return false;
            if (Math.Abs(value1.ColorKey3Time - value2.ColorKey3Time) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleInitialSpeedAlongNormalPlusMinus - value2.ParticleInitialSpeedAlongNormalPlusMinus) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleInitialRotationDegree - value2.ParticleInitialRotationDegree) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleInitialRotationDegreePlusMinus - value2.ParticleInitialRotationDegreePlusMinus) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleRotationSpeedDegreePerSec - value2.ParticleRotationSpeedDegreePerSec) >= 0.001f) return false;
            if (Math.Abs(value1.ParticleRotationSpeedDegreePerSecPlusMinus - value2.ParticleRotationSpeedDegreePerSecPlusMinus) >= 0.001f) return false;

            // Compare form links
            if (!value1.AddonModels.FormKey.Equals(value2.AddonModels.FormKey)) return false;

            if (Math.Abs(value1.HolesStartTime - value2.HolesStartTime) >= 0.001f) return false;
            if (Math.Abs(value1.HolesEndTime - value2.HolesEndTime) >= 0.001f) return false;
            if (Math.Abs(value1.HolesStartValue - value2.HolesStartValue) >= 0.001f) return false;
            if (Math.Abs(value1.HolesEndValue - value2.HolesEndValue) >= 0.001f) return false;
            if (Math.Abs(value1.EdgeWidth - value2.EdgeWidth) >= 0.001f) return false;
            if (value1.EdgeColor.ToArgb() != value2.EdgeColor.ToArgb()) return false;
            if (Math.Abs(value1.ExplosionWindSpeed - value2.ExplosionWindSpeed) >= 0.001f) return false;
            if (value1.TextureCountU != value2.TextureCountU) return false;
            if (value1.TextureCountV != value2.TextureCountV) return false;
            if (Math.Abs(value1.AddonModelsFadeInTime - value2.AddonModelsFadeInTime) >= 0.001f) return false;
            if (Math.Abs(value1.AddonModelsFadeOutTime - value2.AddonModelsFadeOutTime) >= 0.001f) return false;
            if (Math.Abs(value1.AddonModelsScaleStart - value2.AddonModelsScaleStart) >= 0.001f) return false;
            if (Math.Abs(value1.AddonModelsScaleEnd - value2.AddonModelsScaleEnd) >= 0.001f) return false;
            if (Math.Abs(value1.AddonModelsScaleInTime - value2.AddonModelsScaleInTime) >= 0.001f) return false;
            if (Math.Abs(value1.AddonModelsScaleOutTime - value2.AddonModelsScaleOutTime) >= 0.001f) return false;

            if (!value1.AmbientSound.FormKey.Equals(value2.AmbientSound.FormKey)) return false;

            if (value1.FillColorKey2.ToArgb() != value2.FillColorKey2.ToArgb()) return false;
            if (value1.FillColorKey3.ToArgb() != value2.FillColorKey3.ToArgb()) return false;
            if (Math.Abs(value1.FillColorKey1Scale - value2.FillColorKey1Scale) >= 0.001f) return false;
            if (Math.Abs(value1.FillColorKey2Scale - value2.FillColorKey2Scale) >= 0.001f) return false;
            if (Math.Abs(value1.FillColorKey3Scale - value2.FillColorKey3Scale) >= 0.001f) return false;
            if (Math.Abs(value1.FillColorKey1Time - value2.FillColorKey1Time) >= 0.001f) return false;
            if (Math.Abs(value1.FillColorKey2Time - value2.FillColorKey2Time) >= 0.001f) return false;
            if (Math.Abs(value1.FillColorKey3Time - value2.FillColorKey3Time) >= 0.001f) return false;
            if (Math.Abs(value1.ColorScale - value2.ColorScale) >= 0.001f) return false;
            if (Math.Abs(value1.BirthPositionOffset - value2.BirthPositionOffset) >= 0.001f) return false;
            if (Math.Abs(value1.BirthPositionOffsetRangePlusMinus - value2.BirthPositionOffsetRangePlusMinus) >= 0.001f) return false;
            if (value1.ParticleAnimatedStartFrame != value2.ParticleAnimatedStartFrame) return false;
            if (value1.ParticleAnimatedStartFrameVariation != value2.ParticleAnimatedStartFrameVariation) return false;
            if (value1.ParticleAnimatedEndFrame != value2.ParticleAnimatedEndFrame) return false;
            if (value1.ParticleAnimatedLoopStartFrame != value2.ParticleAnimatedLoopStartFrame) return false;
            if (value1.ParticleAnimatedLoopStartVariation != value2.ParticleAnimatedLoopStartVariation) return false;
            if (value1.ParticleAnimatedFrameCount != value2.ParticleAnimatedFrameCount) return false;
            if (value1.ParticleAnimatedFrameCountVariation != value2.ParticleAnimatedFrameCountVariation) return false;
            if (value1.Flags != value2.Flags) return false;
            if (Math.Abs(value1.FillTextureScaleU - value2.FillTextureScaleU) >= 0.001f) return false;
            if (Math.Abs(value1.FillTextureScaleV - value2.FillTextureScaleV) >= 0.001f) return false;
            if (value1.SceneGraphEmitDepthLimit != value2.SceneGraphEmitDepthLimit) return false;

            return true;
        }
    }
}