using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Weapon
{
    public class DestructibleHandler : AbstractDestructibleHandler<IWeaponGetter, IWeapon>
    {
        protected override IDestructibleGetter? GetDestructible(IWeaponGetter record)
        {
            return record.Destructible;
        }

        protected override void SetDestructible(IWeapon record, Destructible? value)
        {
            record.Destructible = value;
        }
    }
}