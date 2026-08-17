using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Message
{
    public class MenuButtonsHandler : AbstractListPropertyHandler<IMessageButtonGetter>
    {
        public override string PropertyName => "MenuButtons";

        protected override ListOrdering Ordering => ListOrdering.PreserveModOrder;

        public override void SetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecord record, List<IMessageButtonGetter>? value)
        {
            if (record is not IMessage message)
            {
                return;
            }

            message.MenuButtons.Clear();
            if (value == null)
            {
                return;
            }

            foreach (var button in value)
            {
                message.MenuButtons.Add(button.DeepCopy());
            }
        }

        public override List<IMessageButtonGetter>? GetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecordGetter record)
        {
            if (record is IMessageGetter message)
            {
                return message.MenuButtons?.ToList();
            }

            return null;
        }

        protected override bool IsItemEqual(IMessageButtonGetter? item1, IMessageButtonGetter? item2)
        {
            return object.Equals(item1, item2);
        }
    }
}