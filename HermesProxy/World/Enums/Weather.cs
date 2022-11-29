namespace HermesProxy.World.Enums
{
    // Used only in Vanilla
    public enum WeatherType
    {
        Fine = 0,
        Rain = 1,
        Snow = 2,
        Storm = 3
    };
    // Used in TBC and newer
    public enum WeatherState
    {
        Fine = 0,
        Fog = 1,
        Drizzle = 2,
        LightRain = 3,
        MediumRain = 4,
        HeavyRain = 5,
        LightSnow = 6,
        MediumSnow = 7,
        HeavySnow = 8,
        LightSandstorm = 22,
        MediumSandstorm = 41,
        HeavySandstorm = 42,
        Thunders = 86,
        BlackRain = 90,
        BlackSnow = 106
    }

    public static class Weather
    {
        public static WeatherState ConvertWeatherTypeToWeatherState(WeatherType type, float grade)
        {
            switch (type)
            {
                case WeatherType.Fine:
                    return WeatherState.Fine;
                case WeatherType.Rain:
                    if (grade <= 0.25f)
                        return WeatherState.Drizzle;
                    else if (grade <= 0.3f)
                        return WeatherState.LightRain;
                    else if (grade <= 0.6f)
                        return WeatherState.MediumRain;
                    else
                        return WeatherState.HeavyRain;
                case WeatherType.Snow:
                    if (grade <= 0.3f)
                        return WeatherState.LightSnow;
                    else if (grade <= 0.6f)
                        return WeatherState.MediumSnow;
                    else
                        return WeatherState.HeavySnow;
                case WeatherType.Storm:
                    if (grade <= 0.3f)
                        return WeatherState.LightSandstorm;
                    else if (grade <= 0.6f)
                        return WeatherState.MediumSandstorm;
                    else
                        return WeatherState.HeavySandstorm;
            }
            return WeatherState.Fine;
        }
    }
}
