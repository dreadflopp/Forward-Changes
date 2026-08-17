using System.Collections.Generic;
using Mutagen.Bethesda.Skyrim;
using ForwardChanges.PropertyHandlers.Abstracts;

namespace ForwardChanges.PropertyHandlers.Climate
{
    public class WeatherTypesHandler : AbstractListPropertyHandler<IWeatherTypeGetter>
    {
        public override string PropertyName => "WeatherTypes";

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
