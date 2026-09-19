using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class VirtualMachineAdapterHandler : AbstractVirtualMachineAdapterHandler<IWeaponGetter, IWeapon, IVirtualMachineAdapterGetter, VirtualMachineAdapter>
    {
        protected override IVirtualMachineAdapterGetter? GetVirtualMachineAdapter(IWeaponGetter record)
        {
            return record.VirtualMachineAdapter;
        }

        protected override void SetVirtualMachineAdapter(IWeapon record, VirtualMachineAdapter? value)
        {
            record.VirtualMachineAdapter = value;
        }

        protected override VirtualMachineAdapter CreateNewAdapter()
        {
            return new VirtualMachineAdapter();
        }
    }
}