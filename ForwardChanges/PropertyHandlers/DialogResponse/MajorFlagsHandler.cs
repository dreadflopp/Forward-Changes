using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.DialogResponse
{
    public class MajorFlagsHandler : AbstractFlagPropertyHandler<Mutagen.Bethesda.Skyrim.DialogResponses.MajorFlag>
    {
        public override string PropertyName => "MajorFlags";

        public override void SetValue(IMajorRecord record, Mutagen.Bethesda.Skyrim.DialogResponses.MajorFlag value)
        {
            if (record is IDialogResponses dialogResponses)
            {
                dialogResponses.MajorFlags = value;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogResponses for {PropertyName}");
            }
        }

        public override Mutagen.Bethesda.Skyrim.DialogResponses.MajorFlag GetValue(IMajorRecordGetter record)
        {
            if (record is IDialogResponsesGetter dialogResponses)
            {
                return dialogResponses.MajorFlags;
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement IDialogResponsesGetter for {PropertyName}");
            }
            return default(Mutagen.Bethesda.Skyrim.DialogResponses.MajorFlag);
        }

        protected override Mutagen.Bethesda.Skyrim.DialogResponses.MajorFlag[] GetAllFlags()
        {
            return Enum.GetValues<Mutagen.Bethesda.Skyrim.DialogResponses.MajorFlag>();
        }

        protected override bool IsFlagSet(Mutagen.Bethesda.Skyrim.DialogResponses.MajorFlag flags, Mutagen.Bethesda.Skyrim.DialogResponses.MajorFlag flag)
        {
            return (flags & flag) == flag;
        }

        protected override Mutagen.Bethesda.Skyrim.DialogResponses.MajorFlag SetFlag(Mutagen.Bethesda.Skyrim.DialogResponses.MajorFlag flags, Mutagen.Bethesda.Skyrim.DialogResponses.MajorFlag flag, bool value)
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
