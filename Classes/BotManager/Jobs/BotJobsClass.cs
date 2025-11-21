using SAIN.Components.BotControllerSpace.Classes.Raycasts;
using SAIN.Plugin;
using SAIN.Preset;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components;

public class BotJobsClass(BotManagerComponent botController) : BotManagerBase(botController)
{
    public DirectionDataJob PlayerDistancesJob { get; } = new DirectionDataJob(botController);
    public VisionRaycastJob VisionJob { get; } = new VisionRaycastJob(botController);
    public EnemyPlaceRaycastJob EnemyPlaceJob { get; } = new EnemyPlaceRaycastJob(botController);

    public void Dispose()
    {
        VisionJob.Dispose();
        EnemyPlaceJob.Dispose();
        PlayerDistancesJob.Dispose();
    }

}