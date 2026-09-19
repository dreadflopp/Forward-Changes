using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogBranch
{
    public class FlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.DialogBranch.Flag>
    {
        public override string PropertyName => "Flags";

        public override Mutagen.Bethesda.Skyrim.DialogBranch.Flag GetValue(IMajorRecordGetter record)
        {
            if (record is IDialogBranchGetter dialogBranch)
            {
                return dialogBranch.Flags ?? 0;
            }
            return 0;
        }

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.DialogBranch.Flag value)
        {
            if (record is IDialogBranch dialogBranch)
            {
                dialogBranch.Flags = value;
            }
        }

        protected override Mutagen.Bethesda.Skyrim.DialogBranch.Flag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.DialogBranch.Flag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.DialogBranch.Flag flags, Mutagen.Bethesda.Skyrim.DialogBranch.Flag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Mutagen.Bethesda.Skyrim.DialogBranch.Flag SetFlag(Mutagen.Bethesda.Skyrim.DialogBranch.Flag flags, Mutagen.Bethesda.Skyrim.DialogBranch.Flag flag, bool value)
        {
            if (value)
            {
                return flags | flag;
            }
            else
            {
                return flags & ~flag;
            }
        }
    }
}
