using DreadsMashedPatch.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace DreadsMashedPatch.PropertyHandlers.ActorValueInformation
{
    public class PerkTreeHandler : AbstractPropertyHandler<IReadOnlyList<IActorValuePerkNodeGetter>>
    {
        public override string PropertyName => "PerkTree";

        public override IReadOnlyList<IActorValuePerkNodeGetter>? GetValue(IMajorRecordGetter record)
        {
            return TryCastRecord<IActorValueInformationGetter>(record, PropertyName)?.PerkTree;
        }

        public override void SetValue(IMajorRecord record, IReadOnlyList<IActorValuePerkNodeGetter>? value)
        {
            var actorValueInformation = TryCastRecord<IActorValueInformation>(record, PropertyName);
            if (actorValueInformation == null)
            {
                return;
            }

            actorValueInformation.PerkTree.Clear();
            if (value == null)
            {
                return;
            }

            foreach (var perkNode in value)
            {
                actorValueInformation.PerkTree.Add(perkNode.DeepCopy());
            }
        }

        public override bool AreValuesEqual(
            IReadOnlyList<IActorValuePerkNodeGetter>? value1,
            IReadOnlyList<IActorValuePerkNodeGetter>? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;
            if (value1.Count != value2.Count) return false;

            for (var i = 0; i < value1.Count; i++)
            {
                if (!ActorValuePerkNodeMixIn.Equals(value1[i], value2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override string FormatValue(object? value)
        {
            return value is IReadOnlyCollection<IActorValuePerkNodeGetter> nodes
                ? $"{PropertyName}({nodes.Count} nodes)"
                : base.FormatValue(value);
        }
    }
}
