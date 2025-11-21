using UnityEngine;
using SeasonController = Class444;

namespace SAIN.Components;

public class LocationClass : GameWorldBase, IGameWorldClass
{
    private const string WEATHER_INTERFACE = "ginterface29_0";
    public bool WinterActive => Season == ESeason.Winter;
    public ESeason Season { get; private set; }
    public ELocation Location { get; private set; }

    public LocationClass(GameWorldComponent component) : base(component)
    {
    }

    public void Init()
    {
    }

    public void ManualUpdate(float currentTime, float deltaTime)
    {
        findLocation();
        findWeather(currentTime, deltaTime);
    }

    public void Dispose()
    {
    }

    private void findWeather(float currentTime, float deltaTime)
    {
        if (_weatherFound)
        {
            return;
        }
        if (_nextCheckWeatherTime > Time.time)
        {
            return;
        }
        _nextCheckWeatherTime = Time.time + 0.5f;

        if (SeasonController.Controller == null)
        {
            return;
        }

        Season = SeasonController.Controller.Season;
        Logger.LogDebug($"Got Season {Season}");
        _weatherFound = true;
    }

    private void findLocation()
    {
        if (!_foundLocation)
        {
            Location = parseLocation();
        }
    }

    private ELocation parseLocation()
    {
        ELocation Location = ELocation.None;
        string locationString = GameWorld.GameWorld?.LocationId;
        if (locationString.IsNullOrEmpty())
        {
            return Location;
        }

        switch (locationString.ToLower())
        {
            case "bigmap":
                Location = ELocation.Customs;
                break;

            case "factory4_day":
                Location = ELocation.Factory;
                break;

            case "factory4_night":
                Location = ELocation.FactoryNight;
                break;

            case "interchange":
                Location = ELocation.Interchange;
                break;

            case "laboratory":
                Location = ELocation.Labs;
                break;

            case "lighthouse":
                Location = ELocation.Lighthouse;
                break;

            case "rezervbase":
                Location = ELocation.Reserve;
                break;

            case "sandbox":
                Location = ELocation.GroundZero;
                break;

            case "sandbox_high":
                Location = ELocation.GroundZero;
                break;

            case "shoreline":
                Location = ELocation.Shoreline;
                break;

            case "tarkovstreets":
                Location = ELocation.Streets;
                break;

            case "woods":
                Location = ELocation.Streets;
                break;

            case "terminal":
                Location = ELocation.Terminal;
                break;

            case "town":
                Location = ELocation.Town;
                break;

            case "labyrinth":
                Location = ELocation.Labyrinth;
                break;

            default:
                Logger.LogError($"{locationString}");
                Location = ELocation.None;
                break;
        }

        _foundLocation = Location != ELocation.None;
        return Location;
    }

    private bool _weatherFound;
    private float _nextCheckWeatherTime;
    private bool _foundLocation;
}