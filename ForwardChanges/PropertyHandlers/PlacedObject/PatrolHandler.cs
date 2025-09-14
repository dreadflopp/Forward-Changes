using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Plugins;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.PlacedObject
{
    public class PatrolHandler : AbstractPropertyHandler<IPatrolGetter?>
    {
        public override string PropertyName => "Patrol";

        public override void SetValue(IMajorRecord record, IPatrolGetter? value)
        {
            if (record is IPlacedObject placedObjectRecord)
            {
                if (value != null)
                {
                    // Create new Patrol and copy properties
                    var newPatrol = new Patrol
                    {
                        IdleTime = value.IdleTime,
                        Idle = new FormLink<IIdleAnimationGetter>(value.Idle.FormKey)
                        // Note: SCHR and SCTX are ReadOnlyMemorySlice<byte> and are skipped
                    };

                    // Copy Topics collection
                    if (value.Topics != null)
                    {
                        foreach (var topic in value.Topics)
                        {
                            // Check the type of topic and create appropriate instance
                            if (topic is ITopicReferenceGetter topicRef)
                            {
                                var newTopicRef = new TopicReference
                                {
                                    Reference = new FormLink<IDialogTopicGetter>(topicRef.Reference.FormKey)
                                };
                                newPatrol.Topics.Add(newTopicRef);
                            }
                            else if (topic is ITopicReferenceSubtypeGetter topicSubtype)
                            {
                                var newTopicSubtype = new TopicReferenceSubtype
                                {
                                    Subtype = topicSubtype.Subtype
                                };
                                newPatrol.Topics.Add(newTopicSubtype);
                            }
                        }
                    }

                    placedObjectRecord.Patrol = newPatrol;
                }
                else
                {
                    placedObjectRecord.Patrol = null;
                }
            }
        }

        public override IPatrolGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is IPlacedObjectGetter placedObjectRecord)
            {
                return placedObjectRecord.Patrol;
            }
            Console.WriteLine($"Error: Record is not a PlacedObject for {PropertyName}");
            return null;
        }

        public override bool AreValuesEqual(IPatrolGetter? value1, IPatrolGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare simple properties
            if (value1.IdleTime != value2.IdleTime) return false;
            if (!value1.Idle.FormKey.Equals(value2.Idle.FormKey)) return false;

            // Compare Topics collection
            if (value1.Topics?.Count != value2.Topics?.Count) return false;

            if (value1.Topics != null && value2.Topics != null)
            {
                for (int i = 0; i < value1.Topics.Count; i++)
                {
                    var topic1 = value1.Topics[i];
                    var topic2 = value2.Topics[i];

                    // Compare based on type
                    if (topic1 is ITopicReferenceGetter topicRef1 && topic2 is ITopicReferenceGetter topicRef2)
                    {
                        if (!topicRef1.Reference.FormKey.Equals(topicRef2.Reference.FormKey)) return false;
                    }
                    else if (topic1 is ITopicReferenceSubtypeGetter topicSubtype1 && topic2 is ITopicReferenceSubtypeGetter topicSubtype2)
                    {
                        if (topicSubtype1.Subtype != topicSubtype2.Subtype) return false;
                    }
                    else
                    {
                        // Different types, not equal
                        return false;
                    }
                }
            }

            // Note: SCHR and SCTX are ReadOnlyMemorySlice<byte> and are not compared

            return true;
        }
    }
}