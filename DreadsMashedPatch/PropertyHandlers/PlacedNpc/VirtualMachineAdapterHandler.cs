using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.PlacedNpc
{
    public class VirtualMachineAdapterHandler : AbstractVirtualMachineAdapterHandler<IPlacedNpcGetter, IPlacedNpc, IVirtualMachineAdapterGetter, VirtualMachineAdapter>
    {
        protected override IVirtualMachineAdapterGetter? GetVirtualMachineAdapter(IPlacedNpcGetter record)
        {
            return record.VirtualMachineAdapter;
        }

        protected override void SetVirtualMachineAdapter(IPlacedNpc record, VirtualMachineAdapter? value)
        {
            record.VirtualMachineAdapter = value;
        }

        protected override VirtualMachineAdapter CreateNewAdapter()
        {
            return new VirtualMachineAdapter();
        }
    }
}