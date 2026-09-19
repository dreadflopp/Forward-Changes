using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Ingredient
{
    public class VirtualMachineAdapterHandler : AbstractVirtualMachineAdapterHandler<IIngredientGetter, IIngredient, IVirtualMachineAdapterGetter, VirtualMachineAdapter>
    {
        protected override IVirtualMachineAdapterGetter? GetVirtualMachineAdapter(IIngredientGetter record)
        {
            return record.VirtualMachineAdapter;
        }

        protected override void SetVirtualMachineAdapter(IIngredient record, VirtualMachineAdapter? value)
        {
            record.VirtualMachineAdapter = value;
        }

        protected override VirtualMachineAdapter CreateNewAdapter()
        {
            return new VirtualMachineAdapter();
        }
    }
}