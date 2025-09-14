using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Npc
{
    public class SoundHandler : AbstractPropertyHandler<IANpcSoundDefinitionGetter?>
    {
        public override string PropertyName => "Sound";

        public override IANpcSoundDefinitionGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is INpcGetter npcRecord)
            {
                return npcRecord.Sound;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpcGetter for {PropertyName}");
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, IANpcSoundDefinitionGetter? value)
        {
            if (record is INpc npcRecord)
            {
                if (value == null)
                {
                    npcRecord.Sound = null;
                    return;
                }

                // Handle different concrete implementations
                if (value is INpcInheritSoundGetter inheritSound)
                {
                    var newInheritSound = new NpcInheritSound
                    {
                        InheritsSoundsFrom = new FormLinkNullable<INpcGetter>(inheritSound.InheritsSoundsFrom.FormKey)
                    };
                    npcRecord.Sound = newInheritSound;
                }
                else if (value is INpcSoundTypesGetter soundTypes)
                {
                    var newSoundTypes = new NpcSoundTypes();

                    // Deep copy the Types list
                    if (soundTypes.Types != null)
                    {
                        foreach (var soundType in soundTypes.Types)
                        {
                            if (soundType != null)
                            {
                                var newSoundType = new NpcSoundType
                                {
                                    Type = soundType.Type
                                };

                                // Deep copy the Sounds list
                                if (soundType.Sounds != null)
                                {
                                    foreach (var sound in soundType.Sounds)
                                    {
                                        if (sound != null)
                                        {
                                            var newSound = new NpcSound
                                            {
                                                Sound = new FormLinkNullable<ISoundDescriptorGetter>(sound.Sound.FormKey),
                                                SoundChance = sound.SoundChance
                                            };
                                            newSoundType.Sounds.Add(newSound);
                                        }
                                    }
                                }

                                newSoundTypes.Types.Add(newSoundType);
                            }
                        }
                    }

                    npcRecord.Sound = newSoundTypes;
                }
                else
                {
                    // Unknown implementation - set to null
                    npcRecord.Sound = null;
                }
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement INpc for {PropertyName}");
            }
        }

        public override bool AreValuesEqual(IANpcSoundDefinitionGetter? value1, IANpcSoundDefinitionGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Handle different concrete implementations
            if (value1 is INpcInheritSoundGetter inheritSound1 && value2 is INpcInheritSoundGetter inheritSound2)
            {
                return inheritSound1.InheritsSoundsFrom.FormKey == inheritSound2.InheritsSoundsFrom.FormKey;
            }
            else if (value1 is INpcSoundTypesGetter soundTypes1 && value2 is INpcSoundTypesGetter soundTypes2)
            {
                // Compare Types list
                if (soundTypes1.Types?.Count != soundTypes2.Types?.Count) return false;

                if (soundTypes1.Types == null && soundTypes2.Types == null) return true;
                if (soundTypes1.Types == null || soundTypes2.Types == null) return false;

                // Compare each sound type
                for (int i = 0; i < soundTypes1.Types.Count; i++)
                {
                    var type1 = soundTypes1.Types[i];
                    var type2 = soundTypes2.Types[i];

                    if (type1 == null && type2 == null) continue;
                    if (type1 == null || type2 == null) return false;

                    if (type1.Type != type2.Type) return false;

                    // Compare Sounds list
                    if (type1.Sounds?.Count != type2.Sounds?.Count) return false;

                    // Compare each sound
                    if (type1.Sounds != null && type2.Sounds != null)
                    {
                        for (int j = 0; j < type1.Sounds.Count; j++)
                        {
                            var sound1 = type1.Sounds[j];
                            var sound2 = type2.Sounds[j];

                            if (sound1 == null && sound2 == null) continue;
                            if (sound1 == null || sound2 == null) return false;

                            if (sound1.Sound.FormKey != sound2.Sound.FormKey) return false;
                            if (sound1.SoundChance != sound2.SoundChance) return false;
                        }
                    }
                }

                return true;
            }
            else
            {
                // Different types or unknown implementations
                return false;
            }
        }

        public override string FormatValue(object? value)
        {
            if (value is not IANpcSoundDefinitionGetter soundDef)
            {
                return value?.ToString() ?? "null";
            }

            if (soundDef is INpcInheritSoundGetter inheritSound)
            {
                return $"InheritSound({inheritSound.InheritsSoundsFrom.FormKey})";
            }
            else if (soundDef is INpcSoundTypesGetter soundTypes)
            {
                var count = soundTypes.Types?.Count ?? 0;
                return $"SoundTypes({count} types)";
            }
            else
            {
                return $"UnknownSoundType({soundDef.GetType().Name})";
            }
        }
    }
}