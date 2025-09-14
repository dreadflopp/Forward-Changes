using System;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Abstracts
{
    public abstract class AbstractDestructibleHandler<TRecordGetter, TRecord> : AbstractPropertyHandler<IDestructibleGetter?>
        where TRecordGetter : class, IMajorRecordGetter
        where TRecord : class, IMajorRecord
    {
        public override string PropertyName => "Destructible";

        public override IDestructibleGetter? GetValue(IMajorRecordGetter record)
        {
            if (record is TRecordGetter typedRecord)
            {
                return GetDestructible(typedRecord);
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement {typeof(TRecordGetter).Name} for {PropertyName}");
            }
            return null;
        }

        public override void SetValue(IMajorRecord record, IDestructibleGetter? value)
        {
            if (record is TRecord typedRecord)
            {
                if (value == null)
                {
                    SetDestructible(typedRecord, null);
                    return;
                }

                // Create a new Destructible instance
                var newDestructible = new Destructible();

                // Copy Data if it exists (the 4 properties we care about for property forwarding)
                if (value.Data != null)
                {
                    newDestructible.Data = new DestructableData
                    {
                        Health = value.Data.Health,
                        DESTCount = value.Data.DESTCount,
                        VATSTargetable = value.Data.VATSTargetable,
                        Unknown = value.Data.Unknown
                    };
                }

                // Note: Stages are complex objects that are not commonly needed for property forwarding
                // and would require specialized deep copying logic. For now, we focus on the simple Data properties.

                SetDestructible(typedRecord, newDestructible);
            }
            else
            {
                Console.WriteLine($"Error: Record does not implement {typeof(TRecord).Name} for {PropertyName}");
            }
        }

        public override bool AreValuesEqual(IDestructibleGetter? value1, IDestructibleGetter? value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Compare Data (the 4 properties we care about for property forwarding)
            if (value1.Data == null && value2.Data == null) { }
            else if (value1.Data == null || value2.Data == null) return false;
            else
            {
                if (value1.Data.Health != value2.Data.Health) return false;
                if (value1.Data.DESTCount != value2.Data.DESTCount) return false;
                if (value1.Data.VATSTargetable != value2.Data.VATSTargetable) return false;
                if (value1.Data.Unknown != value2.Data.Unknown) return false;
            }

            // Note: Stages comparison is skipped as they are complex objects not commonly needed for property forwarding

            return true;
        }

        protected abstract IDestructibleGetter? GetDestructible(TRecordGetter record);
        protected abstract void SetDestructible(TRecord record, Destructible? value);
    }
}