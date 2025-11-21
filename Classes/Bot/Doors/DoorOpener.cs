using EFT;
using EFT.Interactive;
using SAIN.Components;
using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Mover;

public class DoorOpener : BotComponentClassBase
{
    public bool Interacting { get; private set; }

    public EInteractionType InteractionType { get; private set; }

    public DoorOpener(BotComponent sain) : base(sain)
    {
        TickRequirement = ESAINTickState.OnlyNoSleep;
    }

    private const float DOOR_UPDATE_INTERVAL = 0.5f;

    private List<DoorDataStruct> _interactionDoors { get; } = [];
    private List<DoorDataStruct> _allDoors { get; } = [];
    public NavGraphVoxelSimple CurrentVoxel { get; private set; }

    public bool TryInteractWithDoor(EInteractionType interactionType, float time, DoorDataStruct data)
    {
        if (!InteractWithDoor(ref data, interactionType))
        {
#if DEBUG
            Logger.LogDebug($"[{Bot.name}]:[{data.Door.Id}] failed to interact with door");
#endif
            Clear();
            return false;
        }
        _interactionDoors[_interactionDoorIndex] = data;
        Interacting = true;
        ActiveDoor = data;
        InteractionType = interactionType;
        _doorInteractionEndTime = time + (IsDoorPullOpen(data, Bot.NavMeshPosition) ? 1.25f : 1f);
        Bot.Player.MovementContext.IgnoreInteractionCollision(data.Door.Collider, true);
        return true;
    }

    public DoorDataStruct GetActiveDoor()
    {
        if (_interactionDoors.Count > 0 && _interactionDoorIndex < _interactionDoors.Count)
        {
            return _interactionDoors[_interactionDoorIndex];
        }
        return ActiveDoor;
    }

    public bool SelectDoor(out EInteractionType interactionType, out DoorDataStruct currentDoor, IBotPathData pathData)
    {
        const float RAY_LENGTH = 3f;
        Vector3 botPosition = Bot.Position;
        float time = Time.time;

        if (Interacting)
        {
            if (_doorInteractionEndTime < time)
            {
                Clear();

                interactionType = EInteractionType.Open; // Default to open if interaction is done
                currentDoor = default;
                return false;
            }
            else
            {
                interactionType = InteractionType;
                currentDoor = _interactionDoors[_interactionDoorIndex];
                return true;
            }
        }

        SearchForDoors(botPosition, time);

        if (_interactionDoors.Count == 0)
        {
            interactionType = EInteractionType.Open; // Default to open if no doors found
            currentDoor = ActiveDoor;
            return false;
        }

        CornerMoveData moveData = pathData.CurrentCornerMoveData;
        Ray ray = new() {
            origin = botPosition + Vector3.up,
            direction = moveData.CornerDirectionFromBotNormal * RAY_LENGTH,
        };

        DoorDataStruct data = ActiveDoor;
        if (!RaycastToDoors(out interactionType, ref data, out int index, RAY_LENGTH, ray, _interactionDoors))
        {
            currentDoor = ActiveDoor;
            return false;
        }
        _interactionDoorIndex = index;
        _interactionDoors[index] = data;
        //bool value = TryInteractWithDoor(interactionType, time, ref data);
        currentDoor = data;
        return true;
    }

    private void Clear()
    {
        Bot.Player.MovementContext.IgnoreInteractionCollision(ActiveDoor.Door?.Collider, false);
        Interacting = false;
        _interactionDoorIndex = 0;
        ActiveDoor = new();
        _doorInteractionEndTime = 0;
        InteractionType = EInteractionType.Open;
    }

    private int _interactionDoorIndex;

    private void SearchForDoors(Vector3 botPosition, float time)
    {
        if (_nextDoorUpdateTime < time)
        {
            _nextDoorUpdateTime = time + DOOR_UPDATE_INTERVAL;
            BotOwner.AIData.SetPosToVoxel(botPosition);

            var lastVoxel = CurrentVoxel;
            CurrentVoxel = BotOwner.VoxelesPersonalData.CurVoxel;

            if (lastVoxel != CurrentVoxel)
            {
                _allDoors.Clear();
                if (CurrentVoxel != null)
                {
                    //Logger.LogDebug($"{CurrentVoxel.DoorLinks.Count} doors in voxel");
                    foreach (var link in CurrentVoxel.DoorLinks)
                    {
                        if (IsDoorOpenable(link.Door))
                        {
                            _allDoors.Add(new DoorDataStruct(link));
                        }
                    }
                }
                //Logger.LogDebug($"{AllDoors.Count} door data created");
            }

            _interactionDoors.Clear();
            for (int i = 0; i < _allDoors.Count; i++)
            {
                DoorDataStruct door = _allDoors[i];
                door.ManualUpdate(botPosition);
                if (door.InRangeToInteract(door.Door) && door.CanInteractByTime(time))
                {
                    _interactionDoors.Add(door);
                }
                _allDoors[i] = door;
            }
        }
    }

    private static bool FindDoorFromCollider(Collider collider, out DoorDataStruct data, out int index, List<DoorDataStruct> doors)
    {
        if (collider == null)
        {
            data = default;
            index = -1;
            return false;
        }
        data = default;
        for (index = 0; index < doors.Count; index++)
        {
            data = doors[index];
            if (data.Door.Collider == collider)
            {
                return true;
            }
            if (collider.GetComponent<NavMeshDoorLink>() == data.Link)
            {
                Logger.LogDebug($"Found NavMeshDoorLink from collider");
                return true;
            }
        }
        return false;
    }

    private static bool RaycastToDoors(out EInteractionType interactionType, ref DoorDataStruct data, out int index, float RAY_LENGTH, Ray ray, List<DoorDataStruct> doors)
    {
        const float SPHERECAST_RADIUS = 0.15f;
        const float SPHERECAST_DISTANCE = 1.5f;
        if (Physics.SphereCast(ray, SPHERECAST_RADIUS, out RaycastHit hit, SPHERECAST_DISTANCE, LayerMaskClass.PlayerStaticDoorMask))
        {
#if DEBUG
            DebugGizmos.DrawLine(ray.origin, hit.point, Color.red, 0.25f, 30f, true);
#endif
            if (FindDoorFromCollider(hit.collider, out data, out index, doors))
            {
#if DEBUG
                Logger.LogDebug($"Found door from hit collider [PlayerStaticDoorMask] [{hit.collider.name}]");
#endif
                if (data.Door.DoorState == EDoorState.Open)
                {
                    interactionType = EInteractionType.Close;
                    return true;
                }
                if (data.Door.DoorState == EDoorState.Shut)
                {
                    interactionType = EInteractionType.Open;
                    return true;
                }
            }
            else
            {
                //Logger.LogDebug($"Failed to find door, but we hit something on [PlayerStaticDoorMask] [{hit.collider.name}]");
            }
        }
        if (Physics.SphereCast(ray, SPHERECAST_RADIUS, out hit, SPHERECAST_DISTANCE, LayerMaskClass.DoorLayer))
        {
            DebugGizmos.DrawLine(ray.origin, hit.point, Color.red, 0.25f, 30f, true);
            if (FindDoorFromCollider(hit.collider, out data, out index, doors))
            {
#if DEBUG
                Logger.LogDebug($"Found door from hit collider [DoorLayer] [{hit.collider.name}]");
#endif
                if (data.Door.DoorState == EDoorState.Open)
                {
                    interactionType = EInteractionType.Close;
                    return true;
                }
                if (data.Door.DoorState == EDoorState.Shut)
                {
                    interactionType = EInteractionType.Open;
                    return true;
                }
            }
            else
            {
                //Logger.LogDebug($"Failed to find door, but we hit something on [DoorLayer] [{hit.collider.name}]");
            }
        }
        for (index = 0; index < doors.Count; index++)
        {
            data = doors[index];
            if (data.CurrentSqrMagnitude > RAY_LENGTH * RAY_LENGTH) continue;
            if (!CanInteract(data.Link)) continue;
            Collider doorCollider = data.Door.Collider;
            if (doorCollider == null) continue;
            if (doorCollider.Raycast(ray, out hit, SPHERECAST_DISTANCE))
            {
#if DEBUG
                Logger.LogDebug($"hit door  [Door.collider.Raycast]");
                DebugGizmos.DrawLine(ray.origin, hit.point, Color.red, 0.25f, 30f, true);
#endif
                if (data.Door.DoorState == EDoorState.Open)
                {
                    interactionType = EInteractionType.Close;
                    return true;
                }
                if (data.Door.DoorState == EDoorState.Shut)
                {
                    interactionType = EInteractionType.Open;
                    return true;
                }
            }
        }
        interactionType = EInteractionType.Open; // Default to open if no doors found
        return false;
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

    private static bool CanInteract(NavMeshDoorLink link)
    {
        if (!link.ShallInteract())
        {
            return false;
        }
        if (!link.Door.enabled || !link.Door.gameObject.activeInHierarchy || !link.Door.Operatable)
        {
            return false;
        }
        return true;
    }

    private bool InteractWithDoor(ref DoorDataStruct data, EInteractionType type)
    {
        if (data.Door == null) return false;
        switch (data.Door.DoorState)
        {
            case EDoorState.Shut:
                data.LastCloseTime = Time.time;
                break;

            case EDoorState.Open:
                data.LastOpenTime = Time.time;
                break;

            default:
                return false;
        }
        data.LastInteractTime = Time.time;
        Player.MovementContext.ResetCanUsePropState();
        var gstruct = Door.Interact(Player, type);
        if (gstruct.Succeeded)
        {
            //Logger.LogDebug("Success");
            switch (type)
            {
                case EInteractionType.Breach:
                    Player.vmethod_0(data.Door, gstruct.Value, null);
                    break;

                default:
                    Player.vmethod_1(data.Door, gstruct.Value);
                    break;
            }
            return true;
        }
        return false;
    }

    public bool ShallKickOpen(Door door, EInteractionType Etype)
    {
        if (Etype != EInteractionType.Open)
        {
            return false;
        }
        if (!WantToKick())
        {
            return false;
        }
        var breakInParameters = door.GetBreakInParameters(Bot.Position);
        return door.BreachSuccessRoll(breakInParameters.InteractionPosition);
    }

    private bool WantToKick()
    {
        var enemy = Bot.GoalEnemy;
        if (enemy != null)
        {
            if (Bot.Info.PersonalitySettings.General.KickOpenAllDoors)
            {
                return true;
            }
            if (BotOwner.Memory.IsUnderFire)
            {
                return true;
            }
            float? timeSinceSeen = enemy.TimeSinceSeen;
            if (timeSinceSeen != null)
            {
                if (timeSinceSeen.Value < 3f)
                {
                    return true;
                }
                if (timeSinceSeen.Value < 5f && enemy.InLineOfSight)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool IsDoorPullOpen(DoorDataStruct doorData, Vector3 botPosition)
    {
        Vector3 doorOpenPos = doorData.Link.Open2;
        Vector3 doorPos = doorData.Link.transform.position;
        Vector3 openDirection = (doorOpenPos - doorPos).normalized;
        Vector3 botDirection = (botPosition - doorPos).normalized;
        float dotProduct = Vector3.Dot(openDirection, botDirection);
        return dotProduct > 0;
    }

    public DoorDataStruct ActiveDoor = new();
    private float _doorInteractionEndTime;
}