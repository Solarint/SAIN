using EFT.Interactive;
using SAIN.Helpers;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes.Mover;

public readonly struct DoorLocations
{
    private static readonly Vector3 handleUpOffset = Vector3.up * 0.25f;
    private static readonly Vector3 floorDownOffset = Vector3.down;

    public DoorLocations(NavMeshDoorLink link)
    {
        Vector3 linkPos = link.transform.position;
        DoorPivotPoint = linkPos + floorDownOffset;
        DoorHandleOpenFloorPoint = link.Open2 + floorDownOffset;
        DoorHandleCloseFloorPoint = link.Close2_Normal + floorDownOffset;

        DoorHandleOpenLookPoint = link.Open2 + handleUpOffset;
        DoorHandleCloseLookPoint = link.Close2_Normal + handleUpOffset;
        NeutralDoorPoint = link.MidClose + handleUpOffset;

        const float backupPointDistCoef = 1.25f;
        Vector3 openDir = DoorHandleOpenFloorPoint - DoorPivotPoint;
        Vector3 closeDir = DoorHandleCloseFloorPoint - DoorPivotPoint;
        Vector3 backupPoint = DoorPivotPoint + ((openDir + closeDir) * backupPointDistCoef);
        if (NavMesh.SamplePosition(backupPoint, out NavMeshHit hit, 0.5f, -1))
        {
            backupPoint = hit.position;
        }
        DoorBackupPoint = backupPoint;
    }

    public readonly Vector3 DoorPivotPoint;
    public readonly Vector3 DoorHandleOpenLookPoint;
    public readonly Vector3 DoorHandleOpenFloorPoint;
    public readonly Vector3 DoorHandleCloseLookPoint;
    public readonly Vector3 DoorHandleCloseFloorPoint;
    public readonly Vector3 DoorBackupPoint;
    public readonly Vector3 NeutralDoorPoint;
}

public struct DoorDataStruct(NavMeshDoorLink link)
{
    private const float DOORS_POSSIBLE_OPEN_DISTANCE_SQR = 3f * 3f;
    private const float DOORS_POSSIBLE_CLOSE_DISTANCE_SQR = 3f * 3f;
    private const float DOOR_INTERACTION_INTERVAL = 3.5f;
    private const float DOOR_OPEN_INTERVAL = 5f;
    private const float DOOR_CLOSE_INTERVAL = 5f;

    public readonly bool CanInteractByTime(float time)
    {
        if (time - LastInteractTime < DOOR_INTERACTION_INTERVAL)
        {
            return false;
        }
        if (time - LastOpenTime < DOOR_OPEN_INTERVAL)
        {
            return false;
        }
        if (time - LastCloseTime < DOOR_CLOSE_INTERVAL)
        {
            return false;
        }
        return true;
    }

    public void OnInteract(EDoorState desiredState)
    {
        LastInteractTime = Time.time;
        switch (desiredState)
        {
            case EDoorState.Open:
                LastOpenTime = Time.time;
                break;
            case EDoorState.Shut:
                LastCloseTime = Time.time;
                break;
        }
    }

    public readonly int Id => Link.Id;
    public float LastInteractTime;
    public float LastOpenTime;
    public float LastCloseTime;
    public NavMeshDoorLink Link = link;
    public Door Door = link.Door;
    public float CurrentSqrMagnitude;
    public Vector3 Direction;
    public Vector3 DirectionNormal;

    public void ManualUpdate(Vector3 botPosition)
    {
        Direction = Link.MidClose - botPosition;
        //Direction = CenterPoint - from;
        DirectionNormal = Direction.normalized;
        CurrentSqrMagnitude = Direction.sqrMagnitude;
    }

    public readonly bool InRangeToInteract(Door door)
    {
        return door != null && door.DoorState switch {
            EDoorState.Open => CurrentSqrMagnitude < DOORS_POSSIBLE_CLOSE_DISTANCE_SQR,
            EDoorState.Shut => CurrentSqrMagnitude < DOORS_POSSIBLE_OPEN_DISTANCE_SQR,
            _ => false,
        };
    }
}

public class DoorData
{
    private const float DOOR_INTERACTION_INTERVAL = 2.5f;
    private const float DOOR_OPEN_INTERVAL = 5f;
    private const float DOOR_CLOSE_INTERVAL = 5f;

    public DoorData(NavMeshDoorLink link)
    {
        Link = link;
        LinkPosition = link.transform.position;
        Door = link.Door;
    }

    public bool CanInteractByTime()
    {
        float time = Time.time;
        if (time - LastInteractTime < DOOR_INTERACTION_INTERVAL)
        {
            return false;
        }
        if (time - LastOpenTime < DOOR_OPEN_INTERVAL)
        {
            return false;
        }
        if (time - LastCloseTime < DOOR_CLOSE_INTERVAL)
        {
            return false;
        }
        return true;
    }

    public float LastInteractTime { get; set; }
    public float LastOpenTime { get; set; }
    public float LastCloseTime { get; set; }

    public NavMeshDoorLink Link { get; }
    public Vector3 LinkPosition { get; }
    public Door Door { get; }

    public float CurrentSqrMagnitude { get; private set; }

    public void ManualUpdate(Vector3 botPosition)
    {
        Direction = LinkPosition - botPosition;
        //Direction = CenterPoint - from;
        DirectionNormal = Direction.normalized;
        CurrentSqrMagnitude = Direction.sqrMagnitude;
    }

    //public Vector3 CenterPoint {
    //    get
    //    {
    //        switch (Door.DoorState)
    //        {
    //            case EDoorState.Open:
    //                return Link.MidOpen;
    //
    //            default:
    //                return Link.MidClose;
    //        }
    //    }
    //}

    public Vector3 Direction { get; private set; }
    public Vector3 DirectionNormal { get; private set; }
    public float DotProduct { get; set; }
}