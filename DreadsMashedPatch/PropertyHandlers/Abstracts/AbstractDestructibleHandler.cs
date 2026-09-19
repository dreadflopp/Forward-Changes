using System;
using System.Linq;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Records;
using ForwardChanges.PropertyHandlers.Abstracts;
using ForwardChanges.PropertyHandlers.General;

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

                // Destructible is one atomic property. Preserve its Data, Stages,
                // stage models, and nested links when forwarding it.
                SetDestructible(typedRecord, value.DeepCopy());
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

            return DestructibleMixIn.Equals(value1, value2);
        }

        public override string FormatValue(object? value)
        {
            if (value == null) return "null";
            if (value is not IDestructibleGetter destructible) return base.FormatValue(value);

            var data = destructible.Data == null
                ? "null"
                : $"Health={destructible.Data.Health}, DESTCount={destructible.Data.DESTCount}, VATSTargetable={destructible.Data.VATSTargetable}, Unknown={destructible.Data.Unknown}";
            var stages = string.Join(", ", destructible.Stages.Select((stage, index) =>
            {
                var stageData = stage.Data == null
                    ? "Data=null"
                    : $"HealthPercent={stage.Data.HealthPercent}, Index={stage.Data.Index}, ModelDamageStage={stage.Data.ModelDamageStage}, Flags={stage.Data.Flags}, SelfDamagePerSecond={stage.Data.SelfDamagePerSecond}, Explosion={stage.Data.Explosion.FormKey}, Debris={stage.Data.Debris.FormKey}, DebrisCount={stage.Data.DebrisCount}";
                var model = stage.Model == null || stage.Model.File.IsNull
                    ? "null"
                    : AssetPathHelper.Format(stage.Model.File);
                return $"#{index}({stageData}, Model={model})";
            }));

            return $"Destructible(Data: {data}, Stages[{destructible.Stages.Count}]: [{stages}])";
        }

        protected abstract IDestructibleGetter? GetDestructible(TRecordGetter record);
        protected abstract void SetDestructible(TRecord record, Destructible? value);
    }
}
