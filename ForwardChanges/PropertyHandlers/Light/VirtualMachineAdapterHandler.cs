using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Light
{
    public class VirtualMachineAdapterHandler : AbstractVirtualMachineAdapterHandler<ILightGetter, Mutagen.Bethesda.Skyrim.Light, IVirtualMachineAdapterGetter, VirtualMachineAdapter>
    {
        public override string PropertyName => "VirtualMachineAdapter";

        protected override IVirtualMachineAdapterGetter? GetVirtualMachineAdapter(ILightGetter record)
        {
            return record.VirtualMachineAdapter;
        }

        protected override void SetVirtualMachineAdapter(Mutagen.Bethesda.Skyrim.Light record, VirtualMachineAdapter? value)
        {
            record.VirtualMachineAdapter = value;
        }

        protected override VirtualMachineAdapter CreateNewAdapter()
        {
            return new VirtualMachineAdapter();
        }
    }
}

