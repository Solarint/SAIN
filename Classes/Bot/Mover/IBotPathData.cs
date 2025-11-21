using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes.Mover;

public enum EBotMovementState
{
    Walk,
    Sprint,
    Crawl,
}

public interface IBotPathData : IDisposable
{
    public bool Moving { get; }
    public bool Running { get; }

    public IBotPathFinder PathFinder { get; }

    /// <summary>
    /// Reasons for sprinting or not if the bot wants to sprint
    /// </summary>
    public EBotSprintStatus CurrentSprintStatus { get; }

    /// <summary>
    /// Sprint Urgency sets different thresholds for how bots manage stamina
    /// </summary>
    public ESprintUrgency SprintUrgency { get; }

    /// <summary>
    /// Sprint Urgency sets different thresholds for how bots manage stamina
    /// </summary>
    /// <param name="urgency"></param>
    public void UpdateSprint(ESprintUrgency urgency);

    /// <summary>
    /// Whether the path finder is currently trying to sprint or not.
    /// </summary>
    public bool WantToSprint { get; }

    public void RequestStartSprint(ESprintUrgency urgency, string reason);
    public void RequestEndSprint(ESprintUrgency urgency, string reason);


    public string SprintReason { get; }

    public bool Crawling { get; }
    public void RequestStartCrawl();
    public void RequestEndCrawl();

    public int CurrentIndex { get; }
    public CornerMoveData CurrentCornerMoveData { get; }
    public EBotMoveStatus Status { get; }
    public Vector3 StartPosition { get; }
    public Vector3 Destination { get; }
    public List<BotPathCorner> PathCorners { get; }
    public List<Vector3> PathPoints { get; }
    public float TimeStarted { get; }
    public float PathLength { get; }
    public bool OnLastCorner { get; }
    public float CurrentCornerDistanceSqr { get; }
    public float DestinationReachDistance { get; }

    public NavMeshPathStatus PathStatus { get; }
    public NavMeshPath NavMeshPath { get; }

    public void Start();
    public void Pause(float duration);
    public void UnPause();
    public void Cancel(float delay = 0.25f);
    public void RecalcPath();

    public BotPathCorner GetCurrentCorner();
    public BotPathCorner GetLastCorner();
}