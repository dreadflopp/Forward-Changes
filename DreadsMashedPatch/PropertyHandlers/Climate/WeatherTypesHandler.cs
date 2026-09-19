using System.Collections.Generic;
using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Climate
{
    public class WeatherTypesHandler : AbstractListPropertyHandler<IWeatherTypeGetter>
    {
        public override string PropertyName => "WeatherTypes";
        public override ListSemantics Semantics => ListSemantics.SortedKeyed;

        protected override bool IsItemIdentityEqual(IWeatherTypeGetter? left, IWeatherTypeGetter? right) =>
            left?.Weather.FormKey == right?.Weather.FormKey;

        protected override IReadOnlyList<object?> GetSortKey(IWeatherTypeGetter item) => [item.Weather.FormKey];

        public override List<IWeatherTypeGetter>? GetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecordGetter record)
        {
            if (record is IClimateGetter climate)
            {
                return climate.WeatherTypes?.ToList();
            }

            return null;
        }

        public override void SetValue(Mutagen.Bethesda.Plugins.Records.IMajorRecord record, List<IWeatherTypeGetter>? value)
        {
            if (record is not IClimate climate)
            {
                return;
            }

            if (climate.WeatherTypes == null)
            {
                return;
            }

            climate.WeatherTypes.Clear();
            if (value == null)
            {
                return;
            }

            foreach (var weatherType in value)
            {
                if (weatherType == null) continue;
                climate.WeatherTypes.Add(weatherType.DeepCopy());
            }
        }
    }
}
