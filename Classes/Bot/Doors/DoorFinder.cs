using EFT;
using EFT.Interactive;
using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Mover;

public class DoorFinderNew(DoorOpener opener) : BotSubClass<DoorOpener>(opener)
{
    private const float DOOR_UPDATE_INTERVAL = 0.5f;

    public List<DoorDataStruct> InteractionDoors { get; } = [];
    public List<DoorDataStruct> AllDoors { get; } = [];
    public NavGraphVoxelSimple CurrentVoxel { get; private set; }

    public void UpdateDoors(Vector3 botPosition, Vector3 corner)
    {
        float time = Time.time;
        if (_nextDoorUpdateTime < time)
        {
            _nextDoorUpdateTime = time + DOOR_UPDATE_INTERVAL;
            BotOwner.AIData.SetPosToVoxel(botPosition);

            var lastVoxel = CurrentVoxel;
            CurrentVoxel = BotOwner.VoxelesPersonalData.CurVoxel;

            if (lastVoxel != CurrentVoxel)
            {
                AllDoors.Clear();
                if (CurrentVoxel != null)
                {
                    Logger.LogDebug($"{CurrentVoxel.DoorLinks.Count} doors in voxel");
                    foreach (var link in CurrentVoxel.DoorLinks)
                    {
                        if (IsDoorOpenable(link.Door))
                        {
                            AllDoors.Add(new DoorDataStruct(link));
                        }
                    }
                    DebugGizmos.DrawSphere(CurrentVoxel.Position, 3f, Color.green, 3f);
                    foreach (GroupPoint point in CurrentVoxel.Points)
                    {
                        DebugGizmos.DrawSphere(point.Position, 3f, Color.green, 3f);
                        DebugGizmos.DrawLine(point.Position, CurrentVoxel.Position, Color.blue, 3f);
                    }
                }
                //Logger.LogDebug($"{AllDoors.Count} door data created");
            }

            InteractionDoors.Clear();
            for (int i = 0; i < AllDoors.Count; i++)
            {
                DoorDataStruct door = AllDoors[i];
                door.ManualUpdate(botPosition);
                if (door.InRangeToInteract(door.Door) && door.CanInteractByTime(time))
                {
                    InteractionDoors.Add(door);
                }
                AllDoors[i] = door;
            }
        }
    }

    private static bool IsDoorOpenable(Door door)
    {
        if (!door.enabled || !door.gameObject.activeInHierarchy || !door.Operatable)
        {
            return false;
        }
        //if (!ModDetection.ProjectFikaLoaded &&
        //    GlobalSettingsClass.Instance.General.Doors.DisableAllDoors &&
        //    GameWorldComponent.Instance.Doors.DisableDoor(door))
        //{
        //    return false;
        //}
        return true;
    }

    private float _nextDoorUpdateTime;
}