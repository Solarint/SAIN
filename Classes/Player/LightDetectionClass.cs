using BSG.CameraEffects;
using Comfort.Common;
using EFT;
using SAIN.Helpers;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components.PlayerComponentSpace;

public class LightDetectionClass(PlayerComponent component) : PlayerComponentBase(component)
{
    public List<Vector3> LightPoints { get; } = [];

    public bool CheckIsBeamVisible(FlashLightClass EnemyFlashlight)
    {
        // If this isn't visible light, and the bot doesn't have night vision, ignore it
        if (!EnemyFlashlight.WhiteLight &&
            !EnemyFlashlight.Laser &&
            Player.AIData?.BotOwner?.NightVision?.UsingNow == false)
        {
            return false;
        }
        if (EnemyFlashlight.LightDetection.LightPoints.Count <= 0)
        {
            return false;
        }
        return true;
    }

    public void TryToInvestigate(IPlayer Player)
    {
        Vector3 estimatedPosition = EstimatePosition(Player.Position, PlayerComponent.GetDistanceToPlayer(Player.ProfileId), 10f);
        var botComponent = PlayerComponent.BotComponent;
        if (botComponent != null)
        {
            botComponent.Squad.SquadInfo.AddPointToSearch
                (estimatedPosition,
                25f,
                botComponent,
                AISoundType.step,
                Player,
                SAIN.BotController.Classes.Squad.ESearchPointType.Flashlight);
        }
        else
        {
            PlayerComponent.BotOwner?.BotsGroup.AddPointToSearch(estimatedPosition, 20f, PlayerComponent.BotOwner, true, false);
        }
    }

    public static Vector3 EstimatePosition(Vector3 playerPos, float distance, float dispersion)
    {
        Vector3 estimatedPosition = playerPos;
        float maxDispersion = Mathf.Clamp(distance, 0f, 50f);
        float positionDispersion = maxDispersion / dispersion;
        float x = EFTMath.Random(-positionDispersion, positionDispersion);
        float z = EFTMath.Random(-positionDispersion, positionDispersion);
        return new Vector3(estimatedPosition.x + x, estimatedPosition.y, estimatedPosition.z + z);
    }
}