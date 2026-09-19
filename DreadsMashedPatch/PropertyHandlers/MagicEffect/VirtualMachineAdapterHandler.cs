using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.Interfaces;

namespace ForwardChanges.PropertyHandlers.MagicEffect
{
    public class VirtualMachineAdapterHandler : AbstractVirtualMachineAdapterHandler<IMagicEffectGetter, IMagicEffect, IVirtualMachineAdapterGetter, IVirtualMachineAdapter>
    {
        protected override IVirtualMachineAdapterGetter? GetVirtualMachineAdapter(IMagicEffectGetter record)
        {
            return record.VirtualMachineAdapter;
        }

        protected override void SetVirtualMachineAdapter(IMagicEffect record, IVirtualMachineAdapter? value)
        {
            if (value is VirtualMachineAdapter concreteValue)
            {
                record.VirtualMachineAdapter = concreteValue;
            }
            else if (value != null)
            {
                // Create a new VirtualMachineAdapter and copy the data
                var newAdapter = new VirtualMachineAdapter();
                newAdapter.DeepCopyIn(value);
                record.VirtualMachineAdapter = newAdapter;
            }
            else
            {
                record.VirtualMachineAdapter = new VirtualMachineAdapter();
            }
        }

        protected override IVirtualMachineAdapter CreateNewAdapter()
        {
            return new VirtualMachineAdapter();
        }
    }
}
