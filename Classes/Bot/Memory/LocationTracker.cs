using EFT;
using SAIN.Components;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Memory;

public class LocationTracker : BotBase
{
    public Collider BotZoneCollider => BotZone?.Collider;
    public AIPlaceInfo BotZone => BotOwner.AIData.PlaceInfo;
    public bool IsIndoors { get; private set; }

    public LocationTracker(BotComponent sain) : base(sain)
    {
    }

    public override void ManualUpdate()
    {
        if (_checkIndoorsTime < Time.time)
        {
            _checkIndoorsTime = Time.time + 0.2f;
            IsIndoors = Player.AIData.EnvironmentId != 0;
        }
        base.ManualUpdate();
    }

    private float _checkIndoorsTime;
}
