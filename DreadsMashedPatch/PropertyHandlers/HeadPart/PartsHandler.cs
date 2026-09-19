using DreadsMashedPatch.PropertyHandlers.Abstracts;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using DreadsMashedPatch.PropertyHandlers.General;

namespace DreadsMashedPatch.PropertyHandlers.HeadPart;

public class PartsHandler : AbstractListPropertyHandler<IPartGetter>
{
    public override string PropertyName => "Parts";

    public override ListSemantics Semantics => ListSemantics.AlignedOrdered;

    public override List<IPartGetter>? GetValue(IMajorRecordGetter record)
    {
        return record is IHeadPartGetter headPart
            ? headPart.Parts.ToList()
            : null;
    }

    public override void SetValue(IMajorRecord record, List<IPartGetter>? value)
    {
        if (record is not IHeadPart headPart)
        {
            return;
        }

        headPart.Parts.Clear();
        if (value == null)
        {
            return;
        }

        foreach (var part in value)
        {
            headPart.Parts.Add(part.DeepCopy());
        }
    }

    public override bool AreValuesEqual(List<IPartGetter>? value1, List<IPartGetter>? value2)
    {
        if (value1 == null || value2 == null)
        {
            return value1 == null && value2 == null;
        }

        return value1.Count == value2.Count
            && value1.Zip(value2, IsItemEqual).All(equal => equal);
    }

    protected override bool IsItemEqual(IPartGetter? item1, IPartGetter? item2)
    {
        if (item1 == null || item2 == null)
        {
            return item1 == null && item2 == null;
        }

        return item1.PartType == item2.PartType
            && AssetPathHelper.AreEqual(item1.FileName, item2.FileName);
    }
}
