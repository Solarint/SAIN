using EFT;
using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Types.Jobs;

public interface IBotRaycastJobSingleOwner
{
    public IPlayer Owner { get; }
}

public interface IBotRaycastJobMultiOwner
{
    public IPlayer[] Owners { get; }
}

public interface IBotRaycastJobSingleTarget
{
    public IPlayer Target { get; }
}

public interface IBotRaycastJobMultiTarget
{
    public IPlayer[] Targets { get; }
}

public interface IDisposableJobFor : IJobFor, IDisposable
{
}

public interface IRaycastJob : IDisposable
{
    public JobHandle Schedule(int MaxCommandsPerJob = 8);

    public void Complete();

    public bool IsScheduled { get; }
    public bool IsCompleted { get; }
    public bool IsCreated { get; }
    public JobHandle Handle { get; }
    public NativeArray<RaycastHit> Hits { get; }
    public NativeArray<RaycastCommand> Commands { get; }
    public int TotalRaycasts { get; }
    public LayerMask Mask { get; }
}

public interface ISpherecastJob : IDisposable
{
    public JobHandle Schedule(int MaxCommandsPerJob = 8);

    public void Complete();

    public bool IsScheduled { get; }
    public bool IsCompleted { get; }
    public bool IsCreated { get; }
    public JobHandle Handle { get; }
    public NativeArray<RaycastHit> Hits { get; }
    public NativeArray<SpherecastCommand> Commands { get; }
    public int TotalRaycasts { get; }
    public LayerMask Mask { get; }
}