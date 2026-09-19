using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.MiscItem
{
    public class VirtualMachineAdapterHandler : AbstractVirtualMachineAdapterHandler<IMiscItemGetter, IMiscItem, IVirtualMachineAdapterGetter, VirtualMachineAdapter>
    {
        protected override IVirtualMachineAdapterGetter? GetVirtualMachineAdapter(IMiscItemGetter record)
        {
            return record.VirtualMachineAdapter;
        }

        protected override void SetVirtualMachineAdapter(IMiscItem record, VirtualMachineAdapter? value)
        {
            record.VirtualMachineAdapter = value;
        }

        protected override VirtualMachineAdapter CreateNewAdapter()
        {
            return new VirtualMachineAdapter();
        }
    }
}
