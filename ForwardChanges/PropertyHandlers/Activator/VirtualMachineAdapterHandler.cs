using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Activator
{
    public class VirtualMachineAdapterHandler : AbstractVirtualMachineAdapterHandler<IActivatorGetter, IActivator, IVirtualMachineAdapterGetter, IVirtualMachineAdapter>
    {
        protected override IVirtualMachineAdapterGetter? GetVirtualMachineAdapter(IActivatorGetter record)
        {
            return record.VirtualMachineAdapter;
        }

        protected override void SetVirtualMachineAdapter(IActivator record, IVirtualMachineAdapter? value)
        {
            record.VirtualMachineAdapter = (VirtualMachineAdapter?)value;
        }

        protected override IVirtualMachineAdapter CreateNewAdapter()
        {
            return new VirtualMachineAdapter();
        }
    }
}
